using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AnimalShelterApp.Shared;
using Microsoft.Extensions.Configuration;

namespace AnimalShelterApp.Services
{
    /// <summary>
    /// Service for interacting with Firestore database
    /// </summary>
    public class FirestoreService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _projectId = string.Empty;
        
        public FirestoreService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _projectId = _configuration["Firebase:projectId"] ?? string.Empty;
        }
        
        /// <summary>
        /// Creates a new shelter document in Firestore
        /// </summary>
        public async Task<bool> CreateShelterAsync(Shelter shelter, string authToken)
        {
            try
            {
                var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters?documentId={shelter.Id}";
                
                var content = new
                {
                    fields = new
                    {
                        name = new { stringValue = shelter.Name },
                        address = new { stringValue = shelter.Address }
                    }
                };
                
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(content)
                };
                
                // Add auth token
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    // Log the detailed error
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"HTTP Error: {(int)response.StatusCode} {response.StatusCode}");
                    Console.WriteLine($"Error response: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating shelter: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Creates a new user profile document in Firestore
        /// </summary>
        public async Task<bool> CreateUserProfileAsync(UserProfile userProfile, string authToken)
        {
            try
            {
                // Creating a document with a specific ID using the documentId query parameter
                var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/users?documentId={userProfile.Uid}";
                
                // Removing the name field and just specifying fields
                var content = new
                {
                    fields = new
                    {
                        email = new { stringValue = userProfile.Email },
                        displayName = new { stringValue = userProfile.DisplayName },
                        shelterId = new { stringValue = userProfile.ShelterId },
                        uid = new { stringValue = userProfile.Uid }
                    }
                };
                
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(content)
                };
                
                // Add auth token
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    // Log the detailed error
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"HTTP Error: {(int)response.StatusCode} {response.StatusCode}");
                    Console.WriteLine($"Error response: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user profile: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets a user profile from Firestore by user ID
        /// </summary>
        public async Task<UserProfile> GetUserProfileAsync(string uid, string authToken)
        {
            try
            {
                var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/users/{uid}";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                
                // Add auth token
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<JsonElement>();
                    
                    // Parse the Firestore document
                    var fields = content.GetProperty("fields");
                    
                    return new UserProfile
                    {
                        Uid = fields.GetProperty("uid").GetProperty("stringValue").GetString() ?? string.Empty,
                        Email = fields.GetProperty("email").GetProperty("stringValue").GetString() ?? string.Empty,
                        DisplayName = fields.GetProperty("displayName").GetProperty("stringValue").GetString() ?? string.Empty,
                        ShelterId = fields.GetProperty("shelterId").GetProperty("stringValue").GetString() ?? string.Empty
                    };
                }
                
                return new UserProfile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user profile: {ex.Message}");
                return new UserProfile();
            }
        }

    /// <summary>
    /// Gets a shelter from Firestore by ID
    /// </summary>
    public async Task<Shelter?> GetShelterAsync(string shelterId, string authToken)
    {
        try
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            // Add auth token
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                // Parse the Firestore document
                var fields = content.GetProperty("fields");
                
                var nameValue = fields.GetProperty("name").GetProperty("stringValue").GetString() ?? "Unknown Shelter";
                var addressValue = fields.GetProperty("address").GetProperty("stringValue").GetString() ?? "No address provided";
                
                return new Shelter
                {
                    Id = shelterId,
                    Name = nameValue,
                    Address = addressValue
                };
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting shelter: {ex.Message}");
            return null;
        }
    }
    }
}