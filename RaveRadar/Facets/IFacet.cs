using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace RaveRadar.Facets
{
    public interface IFacet<T>
    {
        Expression<Func<T, bool>> Predicate { get; }

        IQueryable<T> Filter(IQueryable<T> entity);
    }
}