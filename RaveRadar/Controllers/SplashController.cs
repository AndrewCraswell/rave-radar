using RaveRadar.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using RaveRadar.Types;
using RaveRadar.Models;

namespace RaveRadar.Controllers
{
    public class SplashController : Controller
    {

        [HttpGet]
        [CompressContentFilter]
        public ActionResult Beta()
        {
            Subscription sub = new Subscription();
            return View(sub);
        }

        [HttpPost]
        public ActionResult Beta(Subscription sub)
        {
            // Validate the email address
            try
            {
                MailAddress validateEmail = new MailAddress(sub.Email);
            } catch (Exception)
            {
                return PartialView("AlertMessage", new AlertMessage("Oops!", "That isn't a valid email address.", AlertType.Error));
            }

            // Make sure this isn't a duplicate entry and add it to the database
            using (RaveRadarContext _db = new RaveRadarContext())
            {
                if (_db.Subscriptions.Any(s => String.Compare(s.Email, sub.Email, true) == 0))
                {
                    return PartialView("AlertMessage", new AlertMessage("Uh Oh!", "The email you entered is already signed up for updates.", AlertType.Error));
                }
                else
                {
                    _db.Subscriptions.Add(sub);
                    _db.SaveChanges();
                    return PartialView("AlertMessage", new AlertMessage("Done!", "Thank you for your interest in Rave Radar. We'll let you know when it's ready for you.", AlertType.Success));
                }
            }
        }

        [HttpGet]
        [CompressContentFilter]
        public ActionResult Maintenance()
        {
            return View();
        }

    }
}
