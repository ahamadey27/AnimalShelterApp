using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalShelterApp.Shared
{
    //Represents a log entry, recording that a dose was administered.
    //This is the historical record of what was given, when, and by whom.
    //It's the core of the application's audit trail.
    public class DoseLog
    {
        // The unique identifier for this specific log entry.
        public string Id { get; set; } = string.Empty;

        // Links back to the schedule that prompted this dose, providing context.
        public string ScheduledDoseId { get; set; } = string.Empty;

        // The ID of the animal who received the dose.
        // This is "denormalized" data, included for easier and faster querying of an animal's history.
        public string AnimalId { get; set; } = string.Empty;

        // The name of the medication given.
        // Also denormalized to prevent having to look up the original medication record for every log entry.
        public string MedicationName { get; set; } = string.Empty;

        // The dosage that was administered.
        // Denormalized so the report shows the exact dosage given at that time.
        public string Dosage { get; set; } = string.Empty;

        // The exact timestamp when the medication was logged as given. This is crucial for auditing.
        public DateTime TimeAdministered { get; set; }

        // The unique ID (Uid) of the user who administered the dose.
        // This creates accountability.
        public string AdministeredByUid { get; set; } = string.Empty;

        // A flag to confirm the dose was given.
        // In the future, this could be used to explicitly log skipped doses (e.g., WasGiven = false).
        public bool WasGiven { get; set; }

    }
}