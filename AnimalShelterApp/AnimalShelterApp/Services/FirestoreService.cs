using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AnimalShelterApp.Shared;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

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

        // Helper method to build Firestore fields for Animal
        private object BuildAnimalFields(Animal animal)
        {
            var fields = new Dictionary<string, object>
            {
                { "name", new { stringValue = animal.Name ?? "" } },
                { "species", new { stringValue = animal.Species ?? "" } },
                { "breed", new { stringValue = animal.Breed ?? "" } },
                {"color", new {stringValue = animal.Color ?? ""}},
                { "photoUrl", new { stringValue = animal.PhotoUrl ?? "" } },
                { "isActive", new { booleanValue = animal.IsActive } }
            };

            if (animal.DateOfBirth.HasValue)
            {
                fields.Add("dateOfBirth", new { timestampValue = animal.DateOfBirth.Value.ToUniversalTime().ToString("o") });
            }

            return fields;
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
                                    Color = fields.TryGetProperty("color", out var colorField)
                                        ? colorField.GetProperty("stringValue").GetString() ?? string.Empty
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
                        IsActive = fields.TryGetProperty("isActive", out var activeField) &&
                            activeField.TryGetProperty("booleanValue", out var boolField) && boolField.GetBoolean()
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

                    // Use the document name (last segment) as the ID
                    if (doc.TryGetProperty("name", out var nameProp))
                    {
                        var segments = nameProp.GetString()?.Split('/');
                        animal.Id = segments != null ? segments[^1] : animalId;
                    }
                    else
                    {
                        animal.Id = animalId;
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

        public async Task<bool> DeleteAnimalAsync(string shelterId, string animalId, string authToken)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/animals/{animalId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }


        public async Task<bool> AnimalExistsAsync(string shelterId, string animalId, string authToken)
        {
            var animal = await GetAnimalAsync(shelterId, animalId, authToken);
            return animal != null;
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

        //Methods for medication services
        public async Task<List<Medication>> GetMedicationsAsync(string shelterId, string token)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/medications";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            var medications = new List<Medication>();

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (responseBody.TryGetProperty("documents", out JsonElement documents))
                {
                    foreach (var doc in documents.EnumerateArray())
                    {
                        var fields = doc.GetProperty("fields");
                        var med = new Medication
                        {
                            Id = doc.GetProperty("name").GetString()?.Split('/').Last() ?? "",
                            Name = fields.TryGetProperty("name", out var nameField) ? nameField.GetProperty("stringValue").GetString() ?? "" : "",
                            DefaultDosage = fields.TryGetProperty("defaultDosage", out var dosageField) ? dosageField.GetProperty("stringValue").GetString() ?? "" : "",
                            Instructions = fields.TryGetProperty("instructions", out var instrField) ? instrField.GetProperty("stringValue").GetString() ?? "" : ""
                        };
                        medications.Add(med);
                    }
                }
            }
            return medications;
        }
        public async Task<bool> CreateMedicationAsync(string shelterId, Medication med, string token)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/medications?documentId={med.Id}";
            var content = new
            {
                fields = new
                {
                    name = new { stringValue = med.Name ?? "" },
                    defaultDosage = new { stringValue = med.DefaultDosage ?? "" },
                    instructions = new { stringValue = med.Instructions ?? "" }
                }
            };
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(content)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateMedicationAsync(string shelterId, Medication med, string token)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/medications/{med.Id}";
            var content = new
            {
                fields = new
                {
                    name = new { stringValue = med.Name ?? "" },
                    defaultDosage = new { stringValue = med.DefaultDosage ?? "" },
                    instructions = new { stringValue = med.Instructions ?? "" }
                }
            };
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = JsonContent.Create(content)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMedicationAsync(string shelterId, string medId, string token)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/medications/{medId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // Add these two new methods for scheduled doses

        public async Task<bool> CreateScheduledDoseAsync(string shelterId, ScheduledDose dose, string token)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/schedule?documentId={dose.Id}";
            var payload = new
            {
                fields = new
                {
                    animalId = new { stringValue = dose.AnimalId },
                    medicationId = new { stringValue = dose.MedicationId },
                    dosage = new { stringValue = dose.Dosage },
                    timeOfDay = new { stringValue = dose.TimeOfDay },
                    notes = new { stringValue = dose.Notes ?? "" }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error creating scheduled dose: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<List<ScheduledDose>> GetScheduledDosesForAnimalAsync(string shelterId, string animalId, string token)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}:runQuery";
            var payload = new
            {
                structuredQuery = new
                {
                    from = new[] { new { collectionId = "schedule" } },
                    where = new
                    {
                        fieldFilter = new
                        {
                            field = new { fieldPath = "animalId" },
                            op = "EQUAL",
                            value = new { stringValue = animalId }
                        }
                    },
                    orderBy = new[] { new { field = new { fieldPath = "timeOfDay" }, direction = "ASCENDING" } }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            var doses = new List<ScheduledDose>();

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error getting scheduled doses: {response.StatusCode} - {errorContent}");
                return doses;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonResponse);

            foreach (var element in jsonDoc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("document", out var doc) && doc.TryGetProperty("fields", out var fields))
                {
                    var dose = new ScheduledDose
                    {
                        Id = doc.GetProperty("name").GetString()?.Split('/').Last() ?? "",
                        AnimalId = fields.TryGetProperty("animalId", out var animalIdProp) ? animalIdProp.GetProperty("stringValue").GetString() ?? "" : "",
                        MedicationId = fields.TryGetProperty("medicationId", out var medIdProp) ? medIdProp.GetProperty("stringValue").GetString() ?? "" : "",
                        Dosage = fields.TryGetProperty("dosage", out var dosageProp) ? dosageProp.GetProperty("stringValue").GetString() ?? "" : "",
                        TimeOfDay = fields.TryGetProperty("timeOfDay", out var timeProp) ? timeProp.GetProperty("stringValue").GetString() ?? "" : "",
                        Notes = fields.TryGetProperty("notes", out var notesProp) ? notesProp.GetProperty("stringValue").GetString() ?? "" : ""
                    };
                    doses.Add(dose);
                }
            }
            return doses;
        }

        /// <summary>
        /// Gets all scheduled doses for an entire shelter, ordered by time.
        /// </summary>

        public async Task<List<ScheduledDose>> GetAllScheduledDosesAsync(string shelterId, string token)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}:runQuery";
            var payload = new
            {
                structuredQuery = new
                {
                    from = new[] { new { collectionId = "schedule" } },
                    orderBy = new[] { new { field = new { fieldPath = "timeOfDay" }, direction = "ASCENDING" } }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            var doses = new List<ScheduledDose>();

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error getting all scheduled doses: {response.StatusCode} - {errorContent}");
                return doses;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonResponse);

            foreach (var element in jsonDoc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("document", out var doc) && doc.TryGetProperty("fields", out var fields))
                {
                    var dose = new ScheduledDose
                    {
                        Id = doc.GetProperty("name").GetString()?.Split('/').Last() ?? "",
                        AnimalId = fields.TryGetProperty("animalId", out var animalIdProp) ? animalIdProp.GetProperty("stringValue").GetString() ?? "" : "",
                        MedicationId = fields.TryGetProperty("medicationId", out var medIdProp) ? medIdProp.GetProperty("stringValue").GetString() ?? "" : "",
                        Dosage = fields.TryGetProperty("dosage", out var dosageProp) ? dosageProp.GetProperty("stringValue").GetString() ?? "" : "",
                        TimeOfDay = fields.TryGetProperty("timeOfDay", out var timeProp) ? timeProp.GetProperty("stringValue").GetString() ?? "" : "",
                        Notes = fields.TryGetProperty("notes", out var notesProp) ? notesProp.GetProperty("stringValue").GetString() ?? "" : ""
                    };
                    doses.Add(dose);
                }
            }
            return doses;
        }

        /// <summary>
        /// Creates a new dose log entry in Firestore.
        /// </summary>
        public async Task<bool> CreateDoseLogAsync(string shelterId, DoseLog log, string token)
        {
            log.Id = Guid.NewGuid().ToString("N");
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}/logs?documentId={log.Id}";

            var payload = new
            {
                fields = new
                {
                    scheduledDoseId = new { stringValue = log.ScheduledDoseId },
                    animalId = new { stringValue = log.AnimalId },
                    medicationName = new { stringValue = log.MedicationName },
                    dosage = new { stringValue = log.Dosage },
                    timeAdministered = new { timestampValue = log.TimeAdministered.ToUniversalTime().ToString("o") },
                    administeredByUid = new { stringValue = log.AdministeredByUid },
                    wasGiven = new { booleanValue = log.WasGiven }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            // --- Start of Added Logging ---
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Successfully created dose log entry.");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to create dose log. Status: {response.StatusCode}, Response: {errorContent}");
            }
            // --- End of Added Logging ---

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Gets all dose logs for a specific date.
        /// </summary>
        public async Task<List<DoseLog>> GetDoseLogsForDateAsync(string shelterId, DateTime date, string token)
        {
            var startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);

            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/shelters/{shelterId}:runQuery";
            var payload = new
            {
                structuredQuery = new
                {
                    from = new[] { new { collectionId = "logs" } },
                    where = new
                    {
                        compositeFilter = new
                        {
                            op = "AND",
                            filters = new[]
                            {
                                new {
                                    fieldFilter = new {
                                        field = new { fieldPath = "timeAdministered" },
                                        op = "GREATER_THAN_OR_EQUAL",
                                        value = new { timestampValue = startOfDay.ToString("o") }
                                    }
                                },
                                new {
                                    fieldFilter = new {
                                        field = new { fieldPath = "timeAdministered" },
                                        op = "LESS_THAN",
                                        value = new { timestampValue = endOfDay.ToString("o") }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            var logs = new List<DoseLog>();

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error getting dose logs: {response.StatusCode} - {errorContent}");
                return logs;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonResponse);

            foreach (var element in jsonDoc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("document", out var doc) && doc.TryGetProperty("fields", out var fields))
                {
                    var log = new DoseLog
                    {
                        Id = doc.GetProperty("name").GetString()?.Split('/').Last() ?? "",
                        ScheduledDoseId = fields.TryGetProperty("scheduledDoseId", out var sdId) ? sdId.GetProperty("stringValue").GetString() ?? "" : "",
                        AnimalId = fields.TryGetProperty("animalId", out var animalId) ? animalId.GetProperty("stringValue").GetString() ?? "" : "",
                        MedicationName = fields.TryGetProperty("medicationName", out var medName) ? medName.GetProperty("stringValue").GetString() ?? "" : "",
                        Dosage = fields.TryGetProperty("dosage", out var dosage) ? dosage.GetProperty("stringValue").GetString() ?? "" : "",
                        AdministeredByUid = fields.TryGetProperty("administeredByUid", out var adminId) ? adminId.GetProperty("stringValue").GetString() ?? "" : "",
                        WasGiven = fields.TryGetProperty("wasGiven", out var wasGivenProp)
                                   && wasGivenProp.TryGetProperty("booleanValue", out var boolProp)
                                   && boolProp.GetBoolean()
                    };

                    if (fields.TryGetProperty("timeAdministered", out var timeProp) && timeProp.TryGetProperty("timestampValue", out var tsVal))
                    {
                        log.TimeAdministered = DateTime.Parse(tsVal.GetString() ?? "");
                    }

                    logs.Add(log);
                }
            }
            return logs;
        }
    }
}