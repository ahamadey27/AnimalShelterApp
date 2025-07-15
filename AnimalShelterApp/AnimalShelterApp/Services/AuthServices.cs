using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using AnimalShelterApp.Shared;

namespace AnimalShelterApp.Services
{
    /// <summary>
    /// Provides authentication-related functionality for the application
    /// using Firebase Authentication.
    /// </summary>
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IConfiguration _configuration;
        private readonly FirestoreService _firestoreService;
        
        // User data that persists throughout the session
        private UserProfile? _currentUser;
        private string? _token;
        
        public event Action? OnAuthStateChanged;

        public AuthService(HttpClient httpClient, IConfiguration configuration, FirestoreService firestoreService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _firestoreService = firestoreService;
            _apiKey = _configuration["Firebase:apiKey"];
        }

        /// <summary>
        /// Gets the currently authenticated user
        /// </summary>
        public UserProfile? CurrentUser => _currentUser;

        /// <summary>
        /// Gets the authentication token for the current user
        /// </summary>
        public string? Token => _token;

        /// <summary>
        /// Login with email and password
        /// </summary>
        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                // Prepare the request body
                var request = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                // Send the login request to Firebase Authentication REST API
                var response = await _httpClient.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}",
                    request);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var idToken = responseContent.GetProperty("idToken").GetString();
                    var uid = responseContent.GetProperty("localId").GetString();
                    
                    // Store the token
                    _token = idToken;
                    
                    // Get the user profile from Firestore
                    _currentUser = await _firestoreService.GetUserProfileAsync(uid, _token);
                    
                    // Notify subscribers that auth state has changed
                    OnAuthStateChanged?.Invoke();
                    
                    return _currentUser != null;
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<JsonElement>();
                    Console.WriteLine($"Login failed: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Register a new user with email and password
        /// </summary>
        public async Task<bool> RegisterAsync(string email, string password, string displayName, string shelterName, string shelterAddress)
        {
            try
            {
                // Register the user with Firebase Authentication
                var authRequest = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}",
                    authRequest);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var idToken = responseContent.GetProperty("idToken").GetString();
                    var uid = responseContent.GetProperty("localId").GetString();
                    
                    _token = idToken;

                    // Create a new shelter
                    var newShelter = new Shelter
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = shelterName,
                        Address = shelterAddress
                    };
                    
                    // Add the shelter to Firestore
                    var shelterCreated = await _firestoreService.CreateShelterAsync(newShelter, _token);
                    
                    if (!shelterCreated)
                    {
                        return false;
                    }
                    
                    // Create a user profile
                    _currentUser = new UserProfile
                    {
                        Uid = uid,
                        Email = email,
                        DisplayName = displayName,
                        ShelterId = newShelter.Id
                    };
                    
                    // Add the user profile to Firestore
                    var profileCreated = await _firestoreService.CreateUserProfileAsync(_currentUser, _token);
                    
                    if (!profileCreated)
                    {
                        return false;
                    }
                    
                    // Notify subscribers that auth state has changed
                    OnAuthStateChanged?.Invoke();
                    
                    return true;
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<JsonElement>();
                    
                    // Log more detailed error information
                    string errorMessage = "Unknown error";
                    if (error.TryGetProperty("error", out JsonElement errorDetails))
                    {
                        if (errorDetails.TryGetProperty("message", out JsonElement message))
                        {
                            errorMessage = message.GetString() ?? "Unknown error";
                        }
                    }
                    
                    Console.WriteLine($"Registration failed: {errorMessage}");
                    Console.WriteLine($"Full error details: {error}");
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Logout the current user
        /// </summary>
        public void Logout()
        {
            _currentUser = null;
            _token = null;
            OnAuthStateChanged?.Invoke();
        }
    }
}