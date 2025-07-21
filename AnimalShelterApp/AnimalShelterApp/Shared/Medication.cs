using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalShelterApp.Shared
{
    // Represents a type of medication that can be administered.
    // This defines a medication's properties, which can then be scheduled for an animal.
    public class Medication
    {
        // The unique identifier for the medication.
        public string Id { get; set; } = "";

        // The commercial or common name of the medication (e.g., "Amoxicillin", "Rimadyl").
        public string Name { get; set; } = "";

        // The standard or most common dosage for this medication (e.g., "250mg", "5ml").
        // This can be overridden in the ScheduledDose.
        public string DefaultDosage { get; set; } = "";

        // General instructions for administering the medication, such as "Give with food".
        public string Instructions { get; set; } = "";

    }
}