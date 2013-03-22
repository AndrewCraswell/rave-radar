using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaveRadar.Facets
{
    public class FacetCollection<T>
    {
        private IList<Facet<T>> _facets;


        public FacetCollection()
        {
            _facets = new List<Facet<T>>();
        }


        public void Add(Facet<T> facet)
        {
            _facets.Add(facet);
        }

        public void Remove(Facet<T> facet)
        {
            _facets.Remove(facet);
        }

        public void Clear()
        {
            _facets.Clear();
        }

        public IQueryable<T> Filter(IQueryable<T> entities)
        {
            return FacetExecuter<T>.Execute(entities, _facets);
        }
    }
}