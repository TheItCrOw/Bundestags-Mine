using BundestagMine.SqlDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace BundestagMine.Utility
{
    public static class MailManager
    {
        private static SmtpClient GetSmtpClient() => new SmtpClient(ConfigManager.GetSmtpHost())
        {
            Port = ConfigManager.GetSmtpPort(),
            Credentials = new NetworkCredential(ConfigManager.GetSmtpUsername(), ConfigManager.GetSmtpPassword()),
            EnableSsl = ConfigManager.GetSmtpEnableSSL()
        };

        /// <summary>
        /// Gets the mail template with button and enters the given title and body
        /// </summary>
        /// <returns></returns>
        public static string CreateMailWithButtonHtml(string title, string body, string buttonText, string buttonUrl)
            => File.ReadAllText(ConfigManager.GetGenericMailTemplateWithButtonPath())
            .Replace("{TITLE}", title)
            .Replace("{BODY}", body)
            .Replace("{BUTTONTEXT}", buttonText)
            .Replace("%7BBUTTONURL%7D", buttonUrl);

        /// <summary>
        /// Gets the mail template and enters the given title and body
        /// </summary>
        /// <returns></returns>
        public static string CreateMailHtml(string title, string body)
            => File.ReadAllText(ConfigManager.GetGenericMailTemplatePath()).Replace("{TITLE}", title).Replace("{BODY}", body);

        /// <summary>
        /// Sends a mail from the default appsettings configurations with the given properties.
        /// </summary>
        /// <returns></returns>
        public static void SendMail(string subject,
            string body,
            List<string> recipients,
            List<Attachment> attachments = null)
        {
            try
            {
                using (var smtpClient = GetSmtpClient())
                {
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                    using (var mailMessage = new MailMessage
                    {
                        From = new MailAddress(ConfigManager.GetSmtpUsername()),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = ConfigManager.GetSmtpIsBodyHtml(),
                    })
                    {
                        foreach (var recipient in recipients)
                        {
                            mailMessage.To.Add(recipient);
                        }

                        if (attachments != null)
                            foreach (var attachment in attachments)
                            {
                                mailMessage.Attachments.Add(attachment);
                            }

                        smtpClient.Send(mailMessage);
                    }
                }
            }
            catch(Exception ex)
            {
                // TODO: Log this here some day :-)
            }
        }
    }
}
