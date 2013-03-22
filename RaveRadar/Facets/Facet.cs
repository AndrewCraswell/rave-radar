using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace RaveRadar.Facets
{
    public abstract class Facet<T> : IFacet<T>
    {
        protected Expression<Func<T, bool>> predicate;

        public Expression<Func<T, bool>> Predicate
        {
            get { return predicate; }
        }

        public IQueryable<T> Filter(IQueryable<T> entities)
        {
            return entities.Where(predicate);
        }
    }
}