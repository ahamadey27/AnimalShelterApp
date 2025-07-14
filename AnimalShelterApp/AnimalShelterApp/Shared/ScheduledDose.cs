using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalShelterApp.Shared
{
    // Represents a single scheduled medication dose for an animal.
    // This defines the "what" and "when" for daily medication routines.
    // This is the model that drives the main dashboard.
    public class ScheduledDose
    {
        // The unique identifier for this specific schedule entry.
        public string Id { get; set; }

        // Links this schedule to a specific animal via its ID.
        public string AnimalId { get; set; }

        // Links this schedule to a specific medication via its ID.
        public string MedicationId { get; set; }

        // The specific amount for this scheduled dose (e.g., "1 tablet", "10ml").
        // This can be different from the medication's default dosage.
        public string Dosage { get; set; }

        // The time of day the dose should be given.
        // Storing this as a simple string in "HH:mm" (24-hour) format (e.g., "08:00", "22:30")
        // makes it easy to query and display without timezone complications.
        public string TimeOfDay { get; set; }

        // Any special notes for this specific scheduled dose,
        // e.g., "Crush and mix with wet food".
        public string Notes { get; set; }

    }
}