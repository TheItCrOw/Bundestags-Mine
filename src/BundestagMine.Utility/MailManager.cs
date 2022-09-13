using BundestagMine.SqlDatabase;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace BundestagMine.Utility
{
    public static class MailManager
    {
        private static SmtpClient _smtpClient = new SmtpClient(ConfigManager.GetSmtpHost())
        {
            Port = ConfigManager.GetSmtpPort(),
            Credentials = new NetworkCredential(ConfigManager.GetSmtpUsername(), ConfigManager.GetSmtpPassword()),
            EnableSsl = ConfigManager.GetSmtpEnableSSL()
        };

        /// <summary>
        /// Sends a mail from the default appsettings configurations with the given properties.
        /// </summary>
        /// <returns></returns>
        public static void SendMail(string subject,
            string body,
            List<string> recipients,
            List<Attachment> attachments = null)
        {
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

                _smtpClient.Send(mailMessage);
            }
        }
    }
}
