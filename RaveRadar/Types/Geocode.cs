using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RaveRadar.Types
{
    public class Geocode
    {
        #region Public Members
        public double Latitude
        {
            get;
            private set;
        }

        public double Longitude
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        public Geocode(double latitude, double longitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
        #endregion
    }
}