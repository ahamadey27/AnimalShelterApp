using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalShelterApp.Shared
{
    // Used to deserialize the wwwroot/sample-data/breeds.json file

    public class SpeciesData
    {
        public string Name { get; set; } = "";
        public List<string> Breeds { get; set; } = new();
    }

    public class BreedInfo
    {
        public List<SpeciesData> Species { get; set; } = new();
    }
}