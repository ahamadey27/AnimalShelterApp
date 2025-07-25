using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalShelterApp.Shared
{
    // Represents a single animal within a shelter.
    // This will be stored in a sub-collection under a specific shelter document.
    public class Animal
    {
        // The unique identifier for the animal, mapping to the Firestore document ID.
        public string Id { get; set; } = string.Empty;

        // The given name of the animal.
        public string Name { get; set; } = string.Empty;

        // The species of the animal (e.g., "Dog", "Cat", "Rabbit").
        public string Species { get; set; } = string.Empty;

        //Animal color
        public string Color { get; set; } = string.Empty;

        // The specific breed of the animal (e.g., "Labrador Retriever", "Domestic Shorthair").
        public string Breed { get; set; } = string.Empty;

        // The animal's date of birth.
        // This is a nullable DateTime (DateTime?) to allow for cases where the exact date of birth is unknown.
        public DateTime? DateOfBirth { get; set; }

        // A URL pointing to the animal's photo.
        // This photo will be uploaded to and stored in Firebase Storage.
        public string PhotoUrl { get; set; } = string.Empty;

        // A flag to indicate if the animal is currently active at the shelter.
        // This allows for "soft-deleting" or archiving records of adopted animals.
        public bool IsActive { get; set; }

        // The animal's weight value.
        public decimal? Weight { get; set; }

        // The unit of weight measurement (lbs or kg).
        public string WeightUnit { get; set; } = "lbs"; // Default to pounds

    }

    // Enum for weight units
    public enum WeightUnit
    {
        Pounds,
        Kilograms
    }
}