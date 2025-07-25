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
        public string Id { get; set; } = string.Empty;

        // Links this schedule to a specific animal via its ID.
        public string AnimalId { get; set; } = string.Empty;

        // Links this schedule to a specific medication via its ID.
        public string MedicationId { get; set; } = string.Empty;

        // The specific amount for this scheduled dose (e.g., "1 tablet", "10ml").
        // This can be different from the medication's default dosage.
        public string Dosage { get; set; } = string.Empty;

        // The time of day the dose should be given.
        // Storing this as a simple string in "HH:mm" (24-hour) format (e.g., "08:00", "22:30")
        // makes it easy to query and display without timezone complications.
        public string TimeOfDay { get; set; } = string.Empty;

        // Any special notes for this specific scheduled dose,
        // e.g., "Crush and mix with wet food".
        public string Notes { get; set; } = string.Empty;

        // The type of recurrence pattern for this medication
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Daily;

        // For "EveryXDays" pattern: how many days between doses (e.g., 2 for every other day)
        public int RecurrenceInterval { get; set; } = 1;

        // For "Weekly" pattern: which days of the week (e.g., Monday, Wednesday, Friday)
        public List<DayOfWeek> DaysOfWeek { get; set; } = new List<DayOfWeek>();

        // === MULTIPLE DAILY DOSES SUPPORT ===
        
        // Number of doses per day (1 = single dose, 2+ = multiple doses)
        public int DosesPerDay { get; set; } = 1;
        
        // Multiple time slots for doses throughout the day (replaces single TimeOfDay for multi-dose)
        // Examples: ["08:00", "14:00", "20:00"] for 3x daily
        public List<string> TimeSlots { get; set; } = new List<string>();
        
        // === FOOD RELATIONSHIP ===
        
        // How this medication should be given in relation to food
        public FoodRelationship FoodRelationship { get; set; } = FoodRelationship.DoesNotMatter;

    }

    // Enum for different types of recurring medication patterns
    public enum RecurrenceType
    {
        Daily,          // Every day
        EveryXDays,     // Every X days (every 2 days, every 3 days, etc.)
        Weekly,         // Specific days of the week (Mon, Wed, Fri)
        BiWeekly,       // Every 2 weeks
        Monthly,        // Once per month on specific date
        AsNeeded        // PRN - when required, no fixed schedule
    }

    // Enum for medication relationship to food
    public enum FoodRelationship
    {
        DoesNotMatter,  // No specific food requirement (default)
        WithFood,       // Must be given with food
        WithoutFood,    // Must be given on empty stomach
        BeforeMeal,     // Give 30-60 minutes before eating
        AfterMeal       // Give after eating
    }
}