using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using RaveRadar.Types;

namespace RaveRadar
{
    public class RaveRadarContext : DbContext
    {
        public DbSet<Rave> Raves { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<AuthorizedUser> AuthorizedUsers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        public void UpsertRaveDetails(IList<RaveDetails> theRaves)
        {
            using (RaveRadarContext _db = new RaveRadarContext())
            {
                UpsertRaveDetails(theRaves, _db);
            }
        }
        public void UpsertRaveDetails(IList<RaveDetails> theRaves, RaveRadarContext _db)
        {
            foreach (RaveDetails aRave in theRaves)
            {
                this.Database.ExecuteSqlCommand("exec UPSERT_Rave @RaveID, @OwnerID, @VenueID, @Name, @PicURL, @StartTime, @EndTime, @IsDateOnly, @Location, @IsApproved, @SubmitterID",  
                    new SqlParameter("@RaveID", aRave.RaveID),
                    new SqlParameter("@OwnerID", aRave.Rave.OwnerID),
                    new SqlParameter("@VenueID", (object)aRave.Rave.VenueID ?? DBNull.Value),
                    new SqlParameter("@Name", aRave.Rave.Name),
                    new SqlParameter("@PicURL", (object)aRave.Rave.PicURL ?? DBNull.Value),
                    new SqlParameter("@StartTime", aRave.Rave.StartTime),
                    new SqlParameter("@EndTime", (object)aRave.Rave.EndTime ?? DBNull.Value),
                    new SqlParameter("@IsDateOnly", aRave.Rave.IsDateOnly),
                    new SqlParameter("@Location", (object)aRave.Rave.Location),
                    new SqlParameter("@IsApproved", aRave.Rave.IsApproved),
                    new SqlParameter("@SubmitterID", (object)aRave.Rave.SubmitterID ?? DBNull.Value));

                UpsertVenue(aRave.Venue);
            }
        }

        public void UpsertVenue(Venue theVenue)
        {
            using (RaveRadarContext _db = new RaveRadarContext())
            {
                UpsertVenue(theVenue, _db);
            }
        }
        public void UpsertVenue(Venue theVenue, RaveRadarContext _db)
        {
            if (theVenue != null)
            {
                this.Database.ExecuteSqlCommand("exec UPSERT_Venue @VenueID, @Name, @PicURL, @Street, @City, @State, @Zip, @Country, @Latitude, @Longitude",
                    new SqlParameter("@VenueID", (object)theVenue.VenueID),
                    new SqlParameter("@Name", (object)theVenue.Name),
                    new SqlParameter("@PicURL", (object)theVenue.PicURL ?? DBNull.Value),
                    new SqlParameter("@Street", (object)theVenue.Street ?? DBNull.Value),
                    new SqlParameter("@City", (object)theVenue.City ?? DBNull.Value),
                    new SqlParameter("@State", (object)theVenue.State ?? DBNull.Value),
                    new SqlParameter("@Zip", (object)theVenue.Zip ?? DBNull.Value),
                    new SqlParameter("@Country", (object)theVenue.Country ?? DBNull.Value),
                    new SqlParameter("@Latitude", (object)theVenue.Latitude ?? DBNull.Value),
                    new SqlParameter("@Longitude", (object)theVenue.Longitude ?? DBNull.Value));
            }
        }
        public void UpsertVenue(IList<Venue> theVenues)
        {
            using (RaveRadarContext _db = new RaveRadarContext())
            {
                UpsertVenue(theVenues, _db);
            }
        }
        public void UpsertVenue(IList<Venue> theVenues, RaveRadarContext _db)
        {
            foreach (Venue aVenue in theVenues)
            {
                UpsertVenue(aVenue, _db);
            }
        }

        public void UpsertOwner(Owner theOwner)
        {
            using (RaveRadarContext _db = new RaveRadarContext())
            {
                UpsertOwner(theOwner, _db);
            }
        }
        public void UpsertOwner(Owner theOwner, RaveRadarContext _db)
        {
            if (theOwner != null)
            {
                this.Database.ExecuteSqlCommand("exec UPSERT_Owner @OwnerID, @AccessToken, @Name, @IsCompany, @IsTrusted",
                    new SqlParameter("@OwnerID", (object)theOwner.OwnerID),
                    new SqlParameter("@AccessToken", (object)theOwner.AccessToken ?? DBNull.Value),
                    new SqlParameter("@Name", (object)theOwner.Name),
                    new SqlParameter("@IsCompany", (object)theOwner.IsCompany),
                    new SqlParameter("@IsTrusted", (object)theOwner.IsTrusted));
            }
        }
        public void UpsertOwner(IList<Owner> theOwners)
        {
            using (RaveRadarContext _db = new RaveRadarContext())
            {
                UpsertOwner(theOwners, _db);
            }
        }
        public void UpsertOwner(IList<Owner> theOwners, RaveRadarContext _db)
        {
            foreach (Owner aOwner in theOwners)
            {
                UpsertOwner(aOwner, _db);
            }
        }

        public void UpsertAuthorizedUser(AuthorizedUser theUser)
        {
            using (RaveRadarContext _db = new RaveRadarContext())
            {
                UpsertAuthorizedUser(theUser, _db);
            }
        }
        public void UpsertAuthorizedUser(AuthorizedUser theUser, RaveRadarContext _db)
        {
            if (theUser != null)
            {
                this.Database.ExecuteSqlCommand("exec UPSERT_AuthorizedUser @UserID, @AccessToken, @Name",
                    new SqlParameter("@UserID", (object)theUser.UserID),
                    new SqlParameter("@AccessToken", (object)theUser.AccessToken),
                    new SqlParameter("@Name", (object)theUser.Name));
            }
        }
        public void UpsertAuthorizedUser(IList<AuthorizedUser> theUsers)
        {
            using (RaveRadarContext _db = new RaveRadarContext())
            {
                UpsertAuthorizedUser(theUsers, _db);
            }
        }
        public void UpsertAuthorizedUser(IList<AuthorizedUser> theUsers, RaveRadarContext _db)
        {
            foreach (AuthorizedUser aUser in theUsers)
            {
                UpsertAuthorizedUser(aUser, _db);
            }
        }
        
    }
}