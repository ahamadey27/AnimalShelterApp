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
        private readonly IConfiguration _configuration;
        private readonly FirestoreService _firestoreService;
        private readonly IJSRuntime _jsRuntime;

        private UserProfile? _currentUser;
        private string? _token;

        public AuthService(HttpClient httpClient, IConfiguration configuration, FirestoreService firestoreService, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _firestoreService = firestoreService;
            _jsRuntime = jsRuntime;
            _apiKey = _configuration["Firebase:apiKey"] ?? throw new InvalidOperationException("Firebase API key not found.");
        }

        public UserProfile? CurrentUser => _currentUser;
        public string? Token => _token;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            _httpClient.DefaultRequestHeaders.Authorization = null;

            try
            {
                var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
                var userJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "userProfile");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userJson))
                {
                    var user = JsonSerializer.Deserialize<UserProfile>(userJson);
                    if (user != null)
                    {
                        identity = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name, user.DisplayName ?? string.Empty),
                            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                            new Claim(ClaimTypes.NameIdentifier, user.Uid),
                            new Claim("ShelterId", user.ShelterId)
                        }, "apiauth");

                        _currentUser = user;
                        _token = token;
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during authentication state retrieval: {ex.Message}");
            }

            var claimsPrincipal = new ClaimsPrincipal(identity);
            return new AuthenticationState(claimsPrincipal);
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
                    if (userProfile != null)
                    {
                        var userJson = JsonSerializer.Serialize(userProfile);
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", idToken);
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userProfile", userJson);

                        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                        return true;
                    }
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

                    if (idToken == null || uid == null) return false;

                    var newShelter = new Shelter { Id = Guid.NewGuid().ToString(), Name = shelterName, Address = shelterAddress };
                    var shelterCreated = await _firestoreService.CreateShelterAsync(newShelter, idToken);

                    if (!shelterCreated) return false;

                    var userProfile = new UserProfile { Uid = uid, Email = email, DisplayName = displayName, ShelterId = newShelter.Id };
                    var profileCreated = await _firestoreService.CreateUserProfileAsync(userProfile, idToken);

                    if (profileCreated)
                    {
                        var userJson = JsonSerializer.Serialize(userProfile);
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", idToken);
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userProfile", userJson);

                        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration exception: {ex.Message}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            _currentUser = null;
            _token = null;
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