using System;
using System.Runtime.Serialization;

namespace RaveRadar.Models
{
    public class RavePin
    {
        public int ID { get; set; }

        public string PicURL { get; set; }

        public string Location { get; set; }

        public double? Longitude { get; set; }
        
        public double? Latitude { get; set; }
    }
}