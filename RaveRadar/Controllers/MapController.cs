using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RaveRadar.Facets;
using RaveRadar.Models;
using RaveRadar.ActionFilters;
using RaveRadar.Helpers;
using RaveRadar.Types;

namespace RaveRadar.Controllers
{
    [BetaFilter]
    [MaintenanceFilter]
    [CompressContentFilter]
    public class MapController : Controller
    {

        // GET: /Map/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MapErrorAlert()
        {
            return PartialView("MapErrorAlert");
        }

        #region Popovers
        [HttpGet]
        //[OutputCache(CacheProfile = "Cache15Minutes", VaryByParam="ids")]
        public ActionResult Popover(string ids)
        {
            IList<Int64> parsedIds = ParseIDsFromString(ids);
            if (parsedIds.Count > 1)
            {
                bool multipleVenues = false;
                using (RaveRadarContext _db = new RaveRadarContext())
                {
                    multipleVenues = _db.Raves.Where(r => parsedIds.Contains(r.ID)).Select(r => r.Location).Distinct().Count() > 1;
                }

                if (multipleVenues)
                {
                    // TODO: Return exact data to be consumed
                    IList<ClusterPinInfo> results = GetClusterPinInfo(parsedIds);
                    return PartialView("ClusterPopover", results);
                }
                else
                {
                    // TODO: Return exact data to be consumed
                    IList<RavePinInfo> results = GetRavePinInfo(parsedIds);
                    return PartialView("VenuePopover", results);
                }
                
            }
            else if (parsedIds.Count > 0)
            {
                // TODO: Return exact data to be consumed
                RavePinInfo result = GetRavePinInfo(parsedIds).First();
                return PartialView("EventPopover", result);
            }

            AlertMessage error = new AlertMessage("Uh oh!", "We couldn't find the event.", AlertType.Error, false);
            return PartialView("AlertMessage", error);
        }
        #endregion

        #region Modal Windows
        [HttpGet]
        [OutputCache(CacheProfile = "Cache30Days")]
        public ActionResult WelcomeModal()
        {
            return PartialView();
        }

        [HttpGet]
        [OutputCache(CacheProfile = "Cache30Days")]
        public ActionResult FeedbackModal()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult FeedbackModal(Feedback msg)
        {
            if (!String.IsNullOrWhiteSpace(msg.Email) && !MailHelper.IsEmailValid(msg.Email))
            {
                return PartialView("AlertMessage", new AlertMessage("Uh Oh!", "That email is invalid.", AlertType.Error, true));
            }

            if (String.IsNullOrWhiteSpace(msg.Message))
            {
                return PartialView("AlertMessage", new AlertMessage("Uh Oh!", "You didn't enter a message.", AlertType.Error, true));
            }

            MailHelper.SendFeedbackMessage(msg.Name, msg.Message, msg.Email);
            return new EmptyResult();
        }

        [HttpGet]
        //[OutputCache(CacheProfile = "Cache15Minutes", VaryByParam = "ids")]
        public ActionResult EventInfoModal(string ids)
        {
            IList<Int64> raveIds = ParseIDsFromString(ids);
            if (raveIds.Count > 1) // Return partial view which displays multiple events
            {
                IList<RavePinInfo> results = GetRavePinInfo(raveIds); // TODO: Return exact data fields
                return PartialView("VenueInfoModal", results);
            }
            else if (raveIds.Count > 0) // Return partial view for single event
            {
                RavePinInfo result = GetRavePinInfo(raveIds).First();  // TODO: Return exact data fields
                return PartialView("EventInfoModal", result);
            }

            // TODO: Return some sort of error modal
            return PartialView();
        }
        #endregion

        // TODO: Move all AJAX calls into a WebAPI
        #region AJAX Methods
        [HttpGet]
        //[OutputCache(CacheProfile = "Cache15Minutes")]
        public ActionResult AJAXGetRavePins()
        {
            FacetCollection<Rave> facets = new FacetCollection<Rave>();
            facets.Add(new FutureRavesFacet());
            facets.Add(new DisplayableRavesFacet());

            return Json(GetRavePins(facets), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        //[OutputCache(CacheProfile = "Cache15Minutes")]
        public ActionResult AJAXGetPinInfo(string ids)
        {
            IList<RavePinInfo> results = new List<RavePinInfo>();
            IList<Int64> parsedIds = ParseIDsFromString(ids);
            results = GetRavePinInfo(parsedIds);

            return Json(results, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Private Helpers
        private IList<RavePin> GetRavePins(FacetCollection<Rave> facets)
        {
            IList<RavePin> results = new List<RavePin>();
            using (RaveRadarContext _db = new RaveRadarContext())
            {
                IQueryable<Rave> filteredRaves = _db.Raves.AsQueryable();
                if (facets != null)
                {
                    filteredRaves = facets.Filter(filteredRaves);
                }

                results = (from r in filteredRaves
                          join v in _db.Venues on r.VenueID equals v.VenueID
                          orderby r.StartTime
                          select new RavePin
                              {
                                  ID = r.ID,
                                  PicURL = r.PicURL ?? string.Empty,
                                  Location = r.Location,
                                  Latitude = v.Latitude,
                                  Longitude = v.Longitude,
                              }).ToList();
            }

            return results;
        }

        private RavePinInfo GetRavePinInfo(Int64 id)
        {
            IList<Int64> ids = new List<Int64>();
            ids.Add(id);
            return GetRavePinInfo(ids).First();
        }
        private IList<RavePinInfo> GetRavePinInfo(IList<Int64> ids)
        {
            IList<RavePinInfo> results = new List<RavePinInfo>();
            using (RaveRadarContext _db = new RaveRadarContext())
            {
                results = (from r in _db.Raves
                           where ids.Contains(r.ID)
                           join o in _db.Owners on r.OwnerID equals o.OwnerID
                           select new RavePinInfo
                           {
                               ID = r.ID,
                               Name = r.Name.Length > 41 ? String.Concat(r.Name.Substring(0, 38).TrimEnd(), "...") : r.Name,
                               RaveID = r.RaveID,
                               VenueID = r.VenueID,
                               OwnerID = r.OwnerID,
                               Location = r.Location,
                               OwnerName = o.Name,
                               PicURL = r.PicURL,
                               StartTime = r.StartTime,
                               EndTime = r.EndTime,
                               IsDateOnly = r.IsDateOnly
                           }).OrderBy(r => r.Location).ThenBy(r => r.StartTime).ToList();
            }

            for (int i = 0; i < results.Count; i++)
            {
                string dateText, timeText;
                GetEventDateText(results[i].StartTime, results[i].EndTime, results[i].IsDateOnly, out dateText, out timeText);
                results[i].DateText = dateText;
                results[i].TimeText = timeText;
            }

            return results;
        }
        private ClusterPinInfo GetClusterPinInfo(Int64 id)
        {
            IList<Int64> ids = new List<Int64>();
            ids.Add(id);
            return GetClusterPinInfo(ids).First();
        }
        private IList<ClusterPinInfo> GetClusterPinInfo(IList<Int64> ids)
        {
            IList<ClusterPinInfo> results = new List<ClusterPinInfo>();

            using (RaveRadarContext _db = new RaveRadarContext())
            {
                results = (from r in _db.Raves
                           join v in _db.Venues on r.VenueID equals v.VenueID
                           where r.VenueID != null && ids.Contains(r.ID)
                           select new ClusterPinInfo {
                                VenueID = v.VenueID,
                                PicURL = v.PicURL,
                           }).Distinct().ToList();

                foreach (ClusterPinInfo cluster in results)
                {
                    cluster.Raves = _db.Raves.Where(r => r.VenueID == cluster.VenueID && ids.Contains(r.ID)).Distinct().OrderBy(r => r.StartTime).Select(r => new RaveMeta
                                    {
                                        ID = r.ID,
                                        RaveID = r.RaveID,
                                        PicURL = r.PicURL
                                    }).ToList();
                    cluster.Location = _db.Raves.Where(r => r.VenueID == cluster.VenueID && ids.Contains(r.ID)).Select(r => r.Location).FirstOrDefault();
                }
            }

            return results;
        }
        private void GetEventDateText(DateTime startTime, DateTime? endTime, bool isDateOnly, out string dateText, out string timeText)
        {
            string result = string.Empty;
            timeText = string.Empty;
            string startPeriod = startTime.ToString("tt").ToLower();
            if (endTime == null)
            {
                dateText = startTime.ToString("dddd, MMMM d, yyyy");
                if (!isDateOnly)
                {
                    timeText = String.Concat(startTime.ToString("h:mm"), startPeriod);
                }
            }
            else
            {
                DateTime endTimeValid = endTime ?? DateTime.Now;
                int daysDiff = (endTimeValid - startTime).Days;
                string endPeriod = endTimeValid.ToString("tt").ToLower();
                if (daysDiff > 1)
                {
                    dateText = String.Concat(startTime.ToString("MMMM d"), " at ", startTime.ToString("h:mm"), startPeriod, " until ", endTimeValid.ToString("MMMM d"), " at ", endTimeValid.ToString("h:mm"), endPeriod);
                }
                else
                {
                    dateText = startTime.ToString("dddd, MMMM d, yyyy");
                    timeText = String.Concat(startTime.ToString("h:mm"), startPeriod, " until ", endTimeValid.ToString("h:mm"), endPeriod);
                }
            }
        }

        static private IList<Int64> ParseIDsFromString(string ids)
        {
            IList<Int64> results = new List<Int64>();
            if (!String.IsNullOrWhiteSpace(ids))
            {
                foreach (string id in ids.Split(','))
                {
                    Int64 tempId = Int64.MinValue;
                    Int64.TryParse(id, out tempId);
                    if (tempId != Int64.MinValue)
                    {
                        results.Add(tempId);
                    }
                }
            }

            return results;
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}