using System;
using System.Xml.Linq;
using System.Web;
using RaveRadar.Types;

namespace RaveRadar.Helpers
{
    public class GoogleMapsAPIHelper
    {
        public static Geocode GetLocationFromAddress(string address)
        {
            // Use the Google Geocoding service to get information about the user-entered address
            // See http://code.google.com/apis/maps/documentation/geocoding/index.html for more info...
            var url = String.Format("http://maps.google.com/maps/api/geocode/xml?address={0}&sensor=false", HttpContext.Current.Server.UrlEncode(address));

            // Load the XML into an XElement object (whee, LINQ to XML!)
            XElement results = XElement.Load(url);

            // Check the status
            string status = results.Element("status").Value;
            if (status != "OK" && status != "ZERO_RESULTS")
            {
                // Whoops, something else was wrong with the request...
                throw new ApplicationException("There was an error with Google's Geocoding Service: " + status);
            }

            Geocode location = null;
            if (status != "ZERO_RESULTS")
            {
                results = results.Element("result").Element("geometry").Element("location");

                double latitude = Convert.ToDouble(results.Element("lat").Value);
                double longitude = Convert.ToDouble(results.Element("lng").Value);
                location = new Geocode(latitude, longitude);
            }

            return location;
        }
    }
}