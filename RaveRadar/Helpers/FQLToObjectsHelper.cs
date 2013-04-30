using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Facebook;
using System.Configuration;
using RaveRadar.Types;

namespace RaveRadar.Helpers
{
    public class FQLToObjectsHelper
    {
        static private FacebookClient _fb = new FacebookClient();
        static private string _accessToken;
        static private string _GenericAccessToken
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_accessToken))
                {
                    string accessToken = ConfigurationManager.AppSettings["RaveRadarGenericToken"];
                    dynamic refreshedToken = _fb.Get("oauth/access_token", new { client_id = ConfigurationManager.AppSettings["RaveRadarAppID"], client_secret = ConfigurationManager.AppSettings["RaveRadarAppSecret"], grant_type = "fb_exchange_token", fb_exchange_token = accessToken });
                    _accessToken = refreshedToken.access_token;
                }
                return _accessToken;
            }
        }
        

        static public RaveDetails GetRaveDetailsFromEventID(Int64 eventId)
        {
            IList<Int64> eventIds = new List<Int64>();
            eventIds.Add(eventId);

            return GetRaveDetailsFromEventIDs(eventIds).First();
        }
        static public IList<RaveDetails> GetRaveDetailsFromEventIDs(IEnumerable<Int64> eventIds)
        {
            _fb.AccessToken = _GenericAccessToken;

            string fields = "eid, name, pic_square, start_time, end_time, is_date_only, creator, update_time, location, venue";
            string eidList = string.Join(",", eventIds.ToArray());
            dynamic fqlResult = _fb.Get("fql", new { q = String.Format("SELECT {0} FROM event WHERE eid IN ({1})", fields, eidList) });
            JsonArray fqlEventResults = fqlResult.data;
            
            
            IList<RaveDetails> eventResults = new List<RaveDetails>();
            HashSet<Venue> venueResults = new HashSet<Venue>();
            HashSet<Rave> raveResults = new HashSet<Rave>();
            foreach (dynamic fqlEvent in fqlEventResults)
            {
                Int64? creatorId = fqlEvent.creator ?? 0;
                Int64? eventId = fqlEvent.eid;
                Int64? venueId = 0;
                if (fqlEvent.venue.Count > 0) 
                { 
                     dynamic jsonVenue = (JsonObject)fqlEvent.venue;
                     venueId = jsonVenue.id ?? 0;
                }
                
                Rave aRave = null;
                Venue aVenue = null;
                Owner aOwner = null;
                if (eventId != null)
                {
                    // Get the owner; no chance for duplications
                    aOwner = GetRaveOwnerFromUserID(creatorId);

                    // Before querying for the Venue, make sure we haven't already queried it
                    if (venueResults.Any(v => v.VenueID == venueId))
                    {
                        aVenue = venueResults.First(v => v.VenueID == venueId);
                    }
                    else if (fqlEvent.venue.Count > 0)
                    {
                        aVenue = GetVenueFromJsonObject((JsonObject)fqlEvent.venue);
                        if (aVenue == null) 
                        {
                          // If no facebook page exists for this Venue, create a new negative ID
                          // All negative ID's therefore show that the Venue does not exist on Facebook
													aVenue = new Venue();
                          using (RaveRadarContext _db = new RaveRadarContext())
                          {
                              long dbMin = _db.Venues.Min(v => v.VenueID).GetValueOrDefault(0);
                              long syncMin = venueResults.Min(v => v.VenueID).GetValueOrDefault(0);
                              aVenue.VenueID = (dbMin <= syncMin ? dbMin : syncMin) - 1;
															aVenue.Name = fqlEvent.location ?? string.Empty;
															aVenue.GetLocationFromGoogle(fqlEvent.location ?? string.Empty);

															// If no location found, reset venue to null
															if (aVenue.GetLocation() == null)
															{
																aVenue = null;
															}
                          }
                        }

												if (aVenue != null)
												{
													venueResults.Add(aVenue);
												}
                    }

                    // Before querying for the Rave, make sure we haven't already queried it
                    if (raveResults.Any(r => r.RaveID == eventId))
                    {
                        aRave = raveResults.First(r => r.RaveID == eventId);
                    }
                    else
                    {
                        aRave = new Rave();
                        aRave.RaveID = eventId ?? 0; // Should never happen
                        aRave.OwnerID = aOwner.OwnerID;
                        if (aVenue == null) { aRave.VenueID = null; } else { aRave.VenueID = aVenue.VenueID; }
                        aRave.Name = fqlEvent.name ?? string.Empty;
                        aRave.PicURL = fqlEvent.pic_square ?? ConfigurationManager.AppSettings["DefaultRaveIcon"].ToString();

                        aRave.StartTime = ConvertFacebookDateToLocal(fqlEvent.start_time, fqlEvent.is_date_only);
                        if (fqlEvent.end_time == null) { aRave.EndTime = null; }
                        else { aRave.EndTime = ConvertFacebookDateToLocal(fqlEvent.end_time, fqlEvent.is_date_only); }
                        aRave.IsDateOnly = fqlEvent.is_date_only;
                        aRave.Location = fqlEvent.location ?? string.Empty;
                        aRave.IsApproved = (aOwner == null ? false : aOwner.IsTrusted);
                        aRave.SubmitterID = null;
                        aRave.UpdateTime = DateTimeConvertor.FromUnixTime((double)fqlEvent.update_time);
                        raveResults.Add(aRave);
                    }
                }

                // Put all the details together to create one object
                eventResults.Add(new RaveDetails(aRave, aOwner, aVenue));
            }

            return eventResults;
        }

        static public Owner GetRaveOwnerFromUserID(Int64? userId)
        {
            Owner ownerResult = null;
            using (RaveRadarContext _db = new RaveRadarContext())
            {
                ownerResult = _db.Owners.Single(o => o.OwnerID == userId);
            }

            return ownerResult;
        }
        static public Owner GetRaveOwnerFromEventID(Int64 eventId)
        {
            var queries = 
                new Dictionary<string, object> 
                {
                    { "EventID", String.Format("SELECT creator FROM event WHERE eid = '{0}'", eventId) },
                    { "Owner", "SELECT id FROM profile WHERE id = #EventID" }
                };

            JsonArray fqlCreators = (JsonArray)_fb.Get(queries);
            return GetRaveOwnerFromUserID(((dynamic)fqlCreators[0]).id);
        }

        /// <summary>
        /// Queries Facebook for all active events.
        /// </summary>
        /// <returns>Dictionary containing the Event ID as the key, and last Updated Time as the value.</returns>
        static public IDictionary<Int64, DateTime> GetActiveEventIDsByOwner(List<Owner> owners)
        {
            // Combine all owner IDs who do not need tokens
            HashSet<Int64> noTokenIds = new HashSet<Int64>(owners.Where(o => String.IsNullOrEmpty(o.AccessToken ?? null)).Select(o => o.OwnerID).ToList());
            //owners.RemoveAll(o => noTokenIds.Contains(o.OwnerID));

            IDictionary<string, string> ownerIds = owners.ToDictionary(o => o.OwnerID.ToString(), o => o.AccessToken ?? null);
            //ownerIds.Add(string.Join(",", noTokenIds.ToArray()), string.Empty);
            JsonArray fqlRaveResults = new JsonArray();
            foreach (string ownerId in ownerIds.Keys)
            {
              // Query Facebook for all active events
              _fb.AccessToken = String.IsNullOrEmpty(ownerIds[ownerId]) ? _GenericAccessToken : ownerIds[ownerId];
              try
              {
                // TODO: This is a temp workaround for this FB platform bug: https://developers.facebook.com/bugs/501594683237232
                //dynamic fqlJsonResult = _fb.Get("fql", new { q = String.Format("SELECT eid, update_time FROM event WHERE creator IN ({0}) AND eid IN (SELECT eid FROM event_member WHERE uid IN({0})) AND (end_time >= now() OR (end_time = 'null' AND start_time >= now()))", ownerId) });
                dynamic fqlJsonResult = _fb.Get("fql", new { q = String.Format("SELECT eid, update_time, creator FROM event WHERE eid IN (SELECT eid FROM event_member WHERE uid = {0}) AND (end_time >= now() OR (end_time = 'null' AND start_time >= now()))", ownerId) });
                fqlRaveResults.AddRange(fqlJsonResult.data);
              }
              catch (FacebookOAuthException)
              {
                  // TODO: Re-enable error notifications
                  //MailHelper.SendErrorMessage(String.Concat("An owner (ID: ", ownerId, ") has an invalid access token. I was unable to query their Events from Facebook."));
              }
            }

            // Convert results to dictionary and filter duplicates
            IList<FacebookEventMeta> hotfixResults = new List<FacebookEventMeta>();
            foreach (dynamic rid in fqlRaveResults)
            {
              hotfixResults.Add(new FacebookEventMeta { EID = rid.eid, Creator = rid.creator, UpdateTime = DateTimeConvertor.FromUnixTime(rid.update_time) });
            }
            hotfixResults = hotfixResults.Distinct().ToList();
            hotfixResults = hotfixResults.Where(r => owners.Any(o => o.OwnerID == r.Creator)).ToList();
            IDictionary<Int64, DateTime> results = hotfixResults.ToDictionary(r => r.EID, r => r.UpdateTime);


            return results;
        }

        static private Venue GetVenueFromJsonObject(JsonObject theVenue)
        {
            dynamic tempVenue = theVenue;

            Venue aVenue = null;
            if (tempVenue.id != null || tempVenue.name != null) // If there is a Venue...
            {
                aVenue = new Venue();
                aVenue.VenueID = tempVenue.id ?? null;
                aVenue.PicURL = ConfigurationManager.AppSettings["DefaultVenueIcon"].ToString();
                aVenue.Name = tempVenue.name ?? string.Empty;
                aVenue.Street = tempVenue.street ?? string.Empty;
                aVenue.City = tempVenue.city ?? string.Empty;
                aVenue.State = tempVenue.state ?? string.Empty;
                aVenue.Zip = tempVenue.zip ?? string.Empty;
                aVenue.Country = tempVenue.country ?? string.Empty;
                aVenue.Latitude = tempVenue.latitude;
                aVenue.Longitude = tempVenue.longitude;

                // If the Venue is actually a Page, query the Venue details
                if (String.IsNullOrEmpty(String.Concat(aVenue.Street, aVenue.City, aVenue.State, aVenue.Zip, aVenue.Country)) && aVenue.GetLocation() == null)
                {
                    // If the ID was never specified, use the Venue name
                    aVenue = GetVenueFromPage(aVenue.VenueID, aVenue.Name);
                }
            }

            // We have successfully recieved a venue
            if (aVenue != null)
            {
                aVenue.GetLocationFromGoogle(); // Gets location from google if the location is unknown
            }

            return aVenue;
        }
        static private Venue GetVenueFromPage(long? pageId, string pageName = null)
        {
            Venue aVenue = null;
            
            if (pageId != null && pageName != null)
            {
                _fb.AccessToken = _GenericAccessToken;
                string whereClause = pageId == null ? String.Concat("name = '", pageName, "'") : String.Concat("page_id = '", pageId, "'");
                dynamic fqlResult = _fb.Get("fql", new { q = String.Format("SELECT name, location, pic_square FROM page WHERE {0}", whereClause) });

                try
                {
                    dynamic location = fqlResult.data[0].location;
                    dynamic name = fqlResult.data[0].name;
                    dynamic pic = fqlResult.data[0].pic_square;

                    if (!String.IsNullOrEmpty(String.Concat(location.latitude, location.longitude)) /*&& !String.IsNullOrEmpty(location.street)*/)
                    {
                        aVenue = new Venue(pageId, name, pic, location.street, location.city, location.state, location.zip, location.country, location.latitude, location.longitude);
                    }
                }
                catch (ArgumentOutOfRangeException) 
                {
                    // No data was returned; Facebook returned no results
                }              
            }

            return aVenue;
        }

        static private DateTime ConvertFacebookDateToLocal(string date, bool isDateOnly)
        {
            DateTime result = DateTime.Parse(date);
            if (!isDateOnly)
            {
                int offsetIndex = date.LastIndexOfAny(new[] { '+', '-' });
                if (offsetIndex > -1)
                {
                    result = DateTime.Parse(date.Substring(0, offsetIndex));
                }
            }

            return result;
        }
    }
}