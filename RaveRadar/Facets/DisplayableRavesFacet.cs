using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RaveRadar.Types;

namespace RaveRadar.Facets
{
    public class DisplayableRavesFacet : Facet<Rave>
    {
        public DisplayableRavesFacet()
        {
            predicate = p => p.IsApproved && p.VenueID != null;
        }
    }
}