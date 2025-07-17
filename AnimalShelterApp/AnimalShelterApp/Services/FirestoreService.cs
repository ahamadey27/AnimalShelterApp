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

    /// <summary>
    /// Retrieves all animals for a specific shelter from Firestore
    /// </summary>
    public async Task<List<Animal>> GetAnimalsAsync(string shelterId, string authToken)
    {
        try
        {
            // Construct the URL to get all animals in the shelter's subcollection
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/animals";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            // Add auth token
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                // Check if the 'documents' property exists and is an array
                if (responseBody.TryGetProperty("documents", out JsonElement documents))
                {
                    var animalsList = new List<Animal>();
                    
                    // Process each document in the response
                    foreach (var doc in documents.EnumerateArray())
                    {
                        try
                        {
                            // Extract the document name (last part is the ID)
                            string documentPath = doc.GetProperty("name").GetString() ?? string.Empty;
                            string animalId = documentPath.Split('/').Last();
                            
                            // Extract fields
                            var fields = doc.GetProperty("fields");
                            
                            var animal = new Animal
                            {
                                Id = animalId,
                                Name = fields.TryGetProperty("name", out var nameField) 
                                    ? nameField.GetProperty("stringValue").GetString() ?? string.Empty 
                                    : string.Empty,
                                Species = fields.TryGetProperty("species", out var speciesField) 
                                    ? speciesField.GetProperty("stringValue").GetString() ?? string.Empty 
                                    : string.Empty,
                                Breed = fields.TryGetProperty("breed", out var breedField) 
                                    ? breedField.GetProperty("stringValue").GetString() ?? string.Empty 
                                    : string.Empty,
                                PhotoUrl = fields.TryGetProperty("photoUrl", out var photoField) 
                                    ? photoField.GetProperty("stringValue").GetString() ?? string.Empty 
                                    : string.Empty,
                                IsActive = fields.TryGetProperty("isActive", out var activeField) 
                                    && activeField.TryGetProperty("booleanValue", out var boolField) 
                                    && boolField.GetBoolean()
                            };
                            
                            // Parse date of birth if present
                            if (fields.TryGetProperty("dateOfBirth", out var dobField) && 
                                dobField.TryGetProperty("timestampValue", out var tsField))
                            {
                                string timestamp = tsField.GetString() ?? string.Empty;
                                if (!string.IsNullOrEmpty(timestamp) && DateTime.TryParse(timestamp, out DateTime dob))
                                {
                                    animal.DateOfBirth = dob;
                                }
                            }
                            
                            animalsList.Add(animal);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing animal document: {ex.Message}");
                            // Continue processing other animals even if one fails
                        }
                    }
                    
                    return animalsList;
                }
                else
                {
                    // No documents array in the response, return an empty list
                    Console.WriteLine("No animals found in the response");
                    return new List<Animal>();
                }
            }
            else
            {
                // Log error details
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"HTTP Error: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"Error response: {errorContent}");
                return new List<Animal>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving animals: {ex.Message}");
            return new List<Animal>();
        }
    }

    /// <summary>
    /// Gets a specific animal from Firestore
    /// </summary>
    public async Task<Animal?> GetAnimalAsync(string shelterId, string animalId, string authToken)
    {
        try
        {
            // Construct URL to get a specific animal document
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/animals/{animalId}";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                // Extract the fields from the document
                var fields = doc.GetProperty("fields");
                
                var animal = new Animal
                {
                    Id = animalId,
                    Name = fields.TryGetProperty("name", out var nameField) 
                        ? nameField.GetProperty("stringValue").GetString() ?? string.Empty 
                        : string.Empty,
                    Species = fields.TryGetProperty("species", out var speciesField) 
                        ? speciesField.GetProperty("stringValue").GetString() ?? string.Empty 
                        : string.Empty,
                    Breed = fields.TryGetProperty("breed", out var breedField) 
                        ? breedField.GetProperty("stringValue").GetString() ?? string.Empty 
                        : string.Empty,
                    PhotoUrl = fields.TryGetProperty("photoUrl", out var photoField) 
                        ? photoField.GetProperty("stringValue").GetString() ?? string.Empty 
                        : string.Empty,
                    IsActive = fields.TryGetProperty("isActive", out var activeField) 
                        && activeField.TryGetProperty("booleanValue", out var boolField) 
                        && boolField.GetBoolean()
                };
                
                // Parse date of birth if present
                if (fields.TryGetProperty("dateOfBirth", out var dobField) && 
                    dobField.TryGetProperty("timestampValue", out var tsField))
                {
                    string timestamp = tsField.GetString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(timestamp) && DateTime.TryParse(timestamp, out DateTime dob))
                    {
                        animal.DateOfBirth = dob;
                    }
                }
                
                return animal;
            }
            else
            {
                // Log error details
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"HTTP Error: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"Error response: {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving animal: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a new animal in Firestore
    /// </summary>
    public async Task<bool> CreateAnimalAsync(string shelterId, Animal animal, string authToken)
    {
        try
        {
            // Firestore API URL to create a document with a specific ID
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/animals?documentId={animal.Id}";
            
            var content = new { fields = BuildAnimalFields(animal) };
            
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(content)
            };
            
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error creating animal: {errorContent}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating animal: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Updates an existing animal in Firestore
    /// </summary>
    public async Task<bool> UpdateAnimalAsync(string shelterId, Animal animal, string authToken)
    {
        try
        {
            // Firestore API URL to update a document
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/animals/{animal.Id}";
            
            var content = new { fields = BuildAnimalFields(animal) };
            
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = JsonContent.Create(content)
            };
            
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error updating animal: {errorContent}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating animal: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Uploads a photo to Firebase Storage and returns the public URL
    /// </summary>
public async Task<string?> UploadAnimalPhotoAsync(string shelterId, string animalId, Stream photoStream, string fileName, string contentType, string authToken)
    {
        try
        {
            var storageBucket = _configuration["Firebase:storageBucket"];
            if (string.IsNullOrEmpty(storageBucket))
            {
                Console.WriteLine("Firebase storage bucket is not configured in appsettings.json");
                return null;
            }

            // Determine the file extension from the filename
            string fileExtension = System.IO.Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                // Fallback to contentType if extension is missing
                if (!string.IsNullOrEmpty(contentType))
                {
                    if (contentType == "image/png") fileExtension = ".png";
                    else if (contentType == "image/jpeg") fileExtension = ".jpg";
                }
                else
                {
                    fileExtension = ".jpg";
                }
            }

            // Define the object path in Firebase Storage
            var objectPath = $"shelters/{shelterId}/animals/{animalId}{fileExtension}";
            var url = $"https://firebasestorage.googleapis.com/v0/b/{storageBucket}/o/{Uri.EscapeDataString(objectPath)}";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StreamContent(photoStream)
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();
                var downloadToken = responseData.GetProperty("downloadTokens").GetString();
                // Construct the public URL
                return $"{url}?alt=media&token={downloadToken}";
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error uploading photo: {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading photo: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Helper method to build the 'fields' object for an animal
    /// </summary>
    private object BuildAnimalFields(Animal animal)
    {
        var fields = new Dictionary<string, object>
        {
            { "name", new { stringValue = animal.Name ?? "" } },
            { "species", new { stringValue = animal.Species ?? "" } },
            { "breed", new { stringValue = animal.Breed ?? "" } },
            { "photoUrl", new { stringValue = animal.PhotoUrl ?? "" } },
            { "isActive", new { booleanValue = animal.IsActive } }
        };

        if (animal.DateOfBirth.HasValue)
        {
            fields["dateOfBirth"] = new { timestampValue = animal.DateOfBirth.Value.ToUniversalTime().ToString("o") };
        }
        else
        {
            fields["dateOfBirth"] = new { nullValue = (object?)null };
        }

        return fields;
    }

    public async Task<string> AddAnimalAsync(string shelterId, Animal animal, string authToken)
    {
        // Firestore API URL to add a new animal document
        var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/animals";
        var content = new { fields = BuildAnimalFields(animal) };        

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(content)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
        string newId = responseBody.GetProperty("name").GetString().Split('/').Last();

        return newId;
    }
    }
}