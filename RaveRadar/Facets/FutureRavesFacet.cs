using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RaveRadar.Types;

namespace RaveRadar.Facets
{
    public class FutureRavesFacet : Facet<Rave>
    {
        public FutureRavesFacet()
        {
            predicate = p => (p.EndTime != null && p.EndTime > DateTime.Now) || p.StartTime >= DateTime.Now;
        }
    }
}