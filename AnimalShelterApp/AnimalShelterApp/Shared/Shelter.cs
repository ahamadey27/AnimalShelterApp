using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalShelterApp.Shared
{
    /// <summary>
    /// Represents a single shelter or organization using the application.
    /// This will be a top-level collection in Firestore.
    public class Shelter
    {
        // The unique identifier for the shelter, mapping to the Firestore document ID.
        public string Id { get; set; } = string.Empty;

        // The official name of the shelter.
        public string Name { get; set; } = string.Empty;

        // The physical address of the shelter.
        public string Address { get; set; } = string.Empty;

    }
}