using System.Net.Mail;
using System.Configuration;
using System;

namespace RaveRadar.Helpers
{
    public class MailHelper
    {
        /// <summary>
        /// Sends an mail message
        /// </summary>
        /// <param name="from">Sender address</param>
        /// <param name="to">Recepient address</param>
        /// <param name="bcc">Bcc recepient</param>
        /// <param name="cc">Cc recepient</param>
        /// <param name="subject">Subject of mail message</param>
        /// <param name="body">Body of mail message</param>
        public static void SendMailMessage(string from, string to, string bcc, string cc, string subject, string body)
        {
            // Instantiate a new instance of MailMessage
            MailMessage mMailMessage = new MailMessage();

            // Set the sender address of the mail message
            mMailMessage.From = new MailAddress(from);
            // Set the recepient address of the mail message
            mMailMessage.To.Add(new MailAddress(to));

            // Check if the bcc value is null or an empty string
            if ((bcc != null) && (bcc != string.Empty))
            {
                // Set the Bcc address of the mail message
                mMailMessage.Bcc.Add(new MailAddress(bcc));
            }

            // Check if the cc value is null or an empty value
            if ((cc != null) && (cc != string.Empty))
            {
                // Set the CC address of the mail message
                mMailMessage.CC.Add(new MailAddress(cc));
            }

            // Set the subject of the mail message
            mMailMessage.Subject = subject;
            // Set the body of the mail message
            mMailMessage.Body = body;

            // Secify the format of the body as HTML
            mMailMessage.IsBodyHtml = true;
            // Set the priority of the mail message to normal
            mMailMessage.Priority = MailPriority.Normal;

            string username = ConfigurationManager.AppSettings.Get("EmailUsername");
            string password = ConfigurationManager.AppSettings.Get("EmailPassword");

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.raveradar.com"; //Or Your SMTP Server Address
            //smtp.Port = 587;
            //smtp.EnableSsl = true;
            //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential(username, password);
            smtp.Send(mMailMessage);
        }

        public static void SendErrorMessage(string errorMsg)
        {
            string[] adminEmails = ConfigurationManager.AppSettings.Get("AdminEmails").Split(',');

            foreach (string email in adminEmails)
            {
                SendMailMessage("support@raveradar.com", email, null, null, errorMsg, string.Empty);
            }
        }

        public static void SendFeedbackMessage(string name, string feedbackMsg, string fromEmail)
        {
            MailAddress from;
            if (IsEmailValid(fromEmail))
            {
                from = new MailAddress(fromEmail);
            }
            else
            {
                from = new MailAddress("feedback@raveradar.com");
            }

            string subject = String.Format("[Feedback Submission{0}]", String.IsNullOrWhiteSpace(name) ? String.Empty : String.Concat(" - ", name));
            SendMailMessage(from.Address, "support@raveradar.com", null, null, subject, feedbackMsg);
        }

        public static bool IsEmailValid(string email)
        {
            try
            {
                MailAddress test = new MailAddress(email);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}