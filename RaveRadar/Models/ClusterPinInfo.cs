using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaveRadar.Models
{
    public class ClusterPinInfo
    {
        public Int64? VenueID { get; set; }

        public string PicURL { get; set; }

        public string Location { get; set; }

        public IEnumerable<RaveMeta> Raves { get; set; }
    }
}