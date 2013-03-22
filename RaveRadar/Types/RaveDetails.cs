using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace RaveRadar.Types
{
    public class RaveDetails
    {
        #region Public Members
        public Int64 RaveID { get; set; }

        public Rave Rave { get; set; }

        public Owner Owner { get; set; }

        public Venue Venue { get; set; }
        #endregion

        #region Constructors
        public RaveDetails()
        {
        }

        public RaveDetails(Rave aRave, Owner aOwner, Venue aVenue)
        {
            RaveID = aRave.RaveID;
            Rave = aRave;
            Owner = aOwner;
            Venue = aVenue;
        }
        #endregion
    }
}