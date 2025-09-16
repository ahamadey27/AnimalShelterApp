using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using Microsoft.JSInterop;
using AnimalShelterApp.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace AnimalShelterApp.Services
{
    public class AuthService : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly FirestoreService _firestoreService;
        private readonly IJSRuntime _jsRuntime;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public AuthService(HttpClient httpClient, IConfiguration configuration, FirestoreService firestoreService, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Firebase:apiKey"] ?? throw new InvalidOperationException("Firebase API key not found.");
            _firestoreService = firestoreService;
            _jsRuntime = jsRuntime;
        }

        public UserProfile? CurrentUser { get; private set; }
        public string? Token { get; private set; }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
                var userJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "userProfile");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userJson))
                {
                    return new AuthenticationState(_anonymous);
                }

                var user = JsonSerializer.Deserialize<UserProfile>(userJson);
                if (user == null)
                {
                    return new AuthenticationState(_anonymous);
                }

                // Attach the token to the HttpClient for all subsequent requests
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.DisplayName ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.NameIdentifier, user.Uid),
                    new Claim("ShelterId", user.ShelterId)
                }, "apiauth");

                CurrentUser = user;
                Token = token;

                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var request = new { email, password, returnSecureToken = true };
                var response = await _httpClient.PostAsJsonAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}", request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var idToken = responseContent.GetProperty("idToken").GetString();
                    var uid = responseContent.GetProperty("localId").GetString();

                    if (idToken == null || uid == null) return false;

                    var userProfile = await _firestoreService.GetUserProfileAsync(uid, idToken);
                    if (userProfile == null)
                    {
                        Console.WriteLine("Login failed: user profile not found in Firestore.");
                        return false;
                    }

                    if (string.IsNullOrEmpty(userProfile.ShelterId))
                    {
                        Console.WriteLine("Login failed: user profile missing ShelterId.");
                        return false;
                    }

                    var userJson = JsonSerializer.Serialize(userProfile);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", idToken);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userProfile", userJson);

                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                    return true;
                }

                // Log response body for debugging when sign-in fails
                try
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Login failed. HTTP {(int)response.StatusCode} - {response.StatusCode}. Response: {errorBody}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Login failed and could not read response body: {ex.Message}");
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string email, string password, string displayName, string shelterName, string shelterAddress)
        {
            try
            {
                var authRequest = new { email, password, returnSecureToken = true };
                var response = await _httpClient.PostAsJsonAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}", authRequest);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var idToken = responseContent.GetProperty("idToken").GetString();
                    var uid = responseContent.GetProperty("localId").GetString();

                    if (idToken == null || uid == null) 
                    {
                        Console.WriteLine("Registration failed: Missing idToken or uid in response");
                        return false;
                    }

                    Console.WriteLine($"User authenticated successfully. UID: {uid}");
                    Console.WriteLine($"Token received: {idToken.Substring(0, Math.Min(20, idToken.Length))}...");

                    // Set authorization header for subsequent requests
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

                    var newShelter = new Shelter { Id = Guid.NewGuid().ToString(), Name = shelterName, Address = shelterAddress };
                    Console.WriteLine($"Attempting to create shelter with ID: {newShelter.Id}");
                    
                    var shelterCreated = await _firestoreService.CreateShelterAsync(newShelter, idToken);

                    if (!shelterCreated) 
                    {
                        Console.WriteLine("Failed to create shelter in Firestore");
                        return false;
                    }

                    Console.WriteLine("Shelter created successfully, now creating user profile");

                    var userProfile = new UserProfile { Uid = uid, Email = email, DisplayName = displayName, ShelterId = newShelter.Id };
                    var profileCreated = await _firestoreService.CreateUserProfileAsync(userProfile, idToken);

                    if (profileCreated)
                    {
                        var userJson = JsonSerializer.Serialize(userProfile);
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", idToken);
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userProfile", userJson);

                        CurrentUser = userProfile;
                        Token = idToken;
                        
                        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                        Console.WriteLine("Registration completed successfully");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Failed to create user profile in Firestore");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Firebase Auth registration failed: {response.StatusCode} - {errorContent}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            CurrentUser = null;
            Token = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userProfile");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task InitializeAuthState()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            await Task.CompletedTask;
        }
    }
}