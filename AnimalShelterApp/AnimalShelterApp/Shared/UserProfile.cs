using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalShelterApp.Shared
{
    /// Represents a user's profile information.
    /// This links the Firebase Authentication user to a specific shelter.
    public class UserProfile
    {
        // The user's unique ID from Firebase Authentication (often called UID).
        public string Uid { get; set; } = string.Empty;

        // The user's email address, which will be used for logging in.
        public string Email { get; set; } = string.Empty;

        // The user's preferred display name within the application.
        public string DisplayName { get; set; } = string.Empty;

        // A reference to the ID of the shelter this user belongs to.
        // This is the crucial link that connects a user to an organization's data.
        public string ShelterId { get; set; } = string.Empty;

    }
}