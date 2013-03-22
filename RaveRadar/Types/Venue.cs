using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace RaveRadar.Types
{
    [Table("Venues")]
    public class Venue
    {
        #region Public Members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int64? VenueID
        {
            get;
            set;
        }

        [MaxLength(50)]
        public string Name
        {
            get;
            set;
        }

        [MaxLength(256)]
        public string PicURL
        {
            get;
            set;
        }

        [MaxLength(128)]
        public string Street
        {
            get;
            set;
        }

        [MaxLength(50)]
        public string City
        {
            get;
            set;
        }

        [MaxLength(50)]
        public string State
        {
            get;
            set;
        }

        [MaxLength(50)]
        public string Zip
        {
            get;
            set;
        }

        [MaxLength(50)]
        public string Country
        {
            get;
            set;
        }

        public double? Latitude
        {
            get;
            set;
        }

        public double? Longitude
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        public Venue()
        {
        }

        public Venue(long? id, string name, string pic, string street, string city, string state, string zip, string country, double? latitude, double? longitude)
        {
            VenueID    = id;
            Name       = name;
            PicURL     = pic;
            Street     = street;
            City       = city;
            State      = state;
            Zip        = zip;
            Country    = country;
            Latitude   = latitude;
            Longitude  = longitude;

            if (String.IsNullOrWhiteSpace(PicURL)) { SetPicUrlToDefaultIcon(); }
        }
        #endregion

        #region Public Methods
        public Geocode GetLocation()
        {
            Geocode location = null;
            if (Latitude != null && Longitude != null)
            {
                double lat = Latitude ?? 0;
                double lon = Longitude ?? 0;
                location = new Geocode(lat, lon);
            }

            return location;
        }

        public void SetLocation(Geocode location)
        {
            if (location != null)
            {
                SetLocation(location.Latitude, location.Longitude);   
            }
        }
        public void SetLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        public void GetLocationFromGoogle()
        {
            // If a long/lat/city is unknown, try to get it from Google
            if ((Latitude == null || Longitude == null || City == null) && Street != null)
            {
                // Construct the best address we can
                string city = City ?? string.Empty;
                string state = State ?? string.Empty;
                string zip = Zip ?? string.Empty;
                string country = Country ?? string.Empty;
                string compositeAddress = Street + ((!String.IsNullOrEmpty(city)) ? ", " + city : string.Empty) +
                                                                    ((!String.IsNullOrEmpty(state)) ? ", " + state : string.Empty) +
                                                                    ((!String.IsNullOrEmpty(zip)) ? ", " + zip : string.Empty) +
                                                                    ((!String.IsNullOrEmpty(country)) ? ", " + country : string.Empty);

                // If we were able to get a decent address, try to find a geolocation
                if (!String.IsNullOrEmpty(compositeAddress))
                {
                    Geocode location = Helpers.GoogleMapsAPIHelper.GetLocationFromAddress(compositeAddress);
                    SetLocation(location);
                }
            }
        }

        public void SetPicUrlToDefaultIcon()
        {
            PicURL = ConfigurationManager.AppSettings["RaveRadarDomain"] + ConfigurationManager.AppSettings["DefaultRaveIcon"];
        }
        #endregion
    }

    class EqualityComparer : IEqualityComparer<Venue>
    {
        public bool Equals(Venue x, Venue y)
        {
            return x.VenueID == y.VenueID;
        }

        public int GetHashCode(Venue obj)
        {
            return obj.VenueID.GetHashCode();
        }
    }
}