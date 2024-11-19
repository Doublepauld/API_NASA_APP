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
        public string Id { get; set; }
        public string Name { get; set; }
        public double EstimatedDiameter { get; set; }
        public string ApproachDate { get; set; }
        public double RelativeVelocity { get; set; }
        public double DistanceFromEarth { get; set; }
    }
}
