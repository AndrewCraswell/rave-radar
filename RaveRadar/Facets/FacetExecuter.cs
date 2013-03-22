using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaveRadar.Facets
{
    public static class FacetExecuter<T>
    {
        public static IQueryable<T> Execute(IQueryable<T> entities, IList<Facet<T>> facets)
        {
            return facets.Aggregate(entities, (current, facet) => current.Where(facet.Predicate));
        }

        public static IQueryable<T> Execute(IQueryable<T> entities, Facet<T> facet)
        {
            return entities.Where(facet.Predicate);
        }
    }
}