using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace API_testing.Models
{
    public class NearEarthObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public double EstimatedDiameter { get; set; }
        public double DistanceFromEarth { get; set; } // Distance in kilometers
        public bool PotentiallyHazardous { get; set; }

        public string PotentiallyHazardousTranslated => PotentiallyHazardous ? "Ano" : "Ne";
        public string ApproachDate { get; set; } // Date of closest approach
        public string OrbitingBody { get; set; } // Planet around which the object orbits
    }
}
