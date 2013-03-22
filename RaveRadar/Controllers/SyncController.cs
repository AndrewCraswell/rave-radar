using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RaveRadar.Types;
using RaveRadar.Helpers;

namespace RaveRadar.Controllers
{
  public class SyncController : ApiController
  {
    // GET api/Sync
    public int Get()
    {
      IList<RaveDetails> uncachedEvents = new List<RaveDetails>();
      using (RaveRadarContext _db = new RaveRadarContext())
      {
        List<Owner> owners = GetTrustedOwners(_db).ToList();
        IDictionary<Int64, DateTime> cachedEventsDictionary = _db.Raves.Where(r => r.StartTime >= DateTime.Now).ToDictionary(r => r.RaveID, r => r.UpdateTime);
        IDictionary<Int64, DateTime> fbActiveEvents = FQLToObjectsHelper.GetActiveEventIDsByOwner(owners); // Add submitted events to this collection

        // Decide which of the active events need to be cached
        IList<Int64> uncachedIds = new List<Int64>();
        IList<Int64> cachedIds = new List<Int64>();
        foreach (Int64 Id in fbActiveEvents.Keys)
        {
          if (!cachedEventsDictionary.ContainsKey(Id) || (fbActiveEvents[Id] > cachedEventsDictionary[Id]))
          {
            uncachedIds.Add(Id);
          }
          else
          {
            cachedIds.Add(Id);
          }
        }
        // TODO: Don't forget to eventually integrate events without trusted owners
        //  - Query DB raves for all submitted raves


        // Get uncached events
        uncachedEvents = FQLToObjectsHelper.GetRaveDetailsFromEventIDs(uncachedIds);

        // Cache the events
        _db.UpsertRaveDetails(uncachedEvents, _db);
      }

      return uncachedEvents.Count;
    }



    static private IList<Owner> GetTrustedOwners()
    {
        IList<Owner> owners;
        using (RaveRadarContext _db = new RaveRadarContext())
        {
            owners = GetTrustedOwners(_db).ToList();
        }

        return owners;
    }
    static private IQueryable<Owner> GetTrustedOwners(RaveRadarContext _db)
    {
        return _db.Owners.Where(o => o.IsTrusted).Distinct();
    }
  }
}