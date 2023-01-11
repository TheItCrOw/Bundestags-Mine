using BundestagMine.Logic.Services;
using BundestagMine.Models.Database;
using BundestagMine.Services;
using BundestagMine.SqlDatabase;
using BundestagMine.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BundestagMine.Controllers
{
    [Route("api/DailyPaperController")]
    [ApiController]
    public class DailyPaperController : Controller
    {
        private readonly ILogger<DailyPaperController> _logger;
        private readonly BundestagMineDbContext _db;
        private readonly ViewRenderService _viewRenderService;
        private readonly DailyPaperService _dailyPaperService;

        public DailyPaperController(ILogger<DailyPaperController> logger,
            DailyPaperService dailyPaperService,
            ViewRenderService viewRenderService,
            BundestagMineDbContext db)
        {
            _db = db;
            _viewRenderService = viewRenderService;
            _dailyPaperService = dailyPaperService;
            _logger = logger;
        }

        [HttpGet("/api/DailyPaperController/SendNewDailyPaperMails/")]
        public async Task<IActionResult> SendNewDailyPaperMails()
        {
            dynamic response = new ExpandoObject();

            try
            {
                // Only send mails from admin acc
                if (!User.Identity.IsAuthenticated)
                {
                    response.status = "400";
                    response.message = "Du hast keine Rechte hierfür.";
                    return Json(response);
                }

                var baseUrl = Request.Scheme + "://" + Request.Host;
                var sendList = _dailyPaperService.GetNotUpToDateSubscriptions();
                response.status = "200";

                // Send a notification to all subs
                var emailCounter = 0;
                foreach (var sub in sendList)
                {
                    var unsubscribeUrl = baseUrl + "/api/DailyPaperController/DeleteSubscription/" + sub.Email;
                    try
                    {
                        MailManager.SendMail("Neues vom Schürfer!",
                            MailManager.CreateMailWithButtonHtml("Es gibt Neues vom Schürfer",
                            "Sie erhalten diese Mail, da Sie den Schürfer abonniert haben." +
                            "<br/>Sollten Sie Ihr Abonnement beenden wollen, klicken Sie " +
                            $"<a href='{unsubscribeUrl}'>hier</a>.",
                            "Jetzt lesen",
                            baseUrl),
                            new List<string>() { sub.Email });
                        // Dont store user emails in log...
                        _logger.LogInformation("Send mail to subcription with id: " + sub.Id);
                        emailCounter++;
                        // Store the information
                        sub.LastSentDailyPaperId = _dailyPaperService.GetNewestDailyPaper().Id;
                        sub.LastSendTime = DateTime.Now;
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "couldnt send daily paper mail to sub with id: " + sub.Id);
                        response.status = "300";
                    }
                }
                response.message = emailCounter;
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't send the daily paper news mails";
                _logger.LogError(ex, "Error sending the daily paper news:");
            }

            await _db.SaveChangesAsync();
            return Json(response);
        }

        [HttpGet("/api/DailyPaperController/DeleteSubscription/{mail}")]
        public async Task<string> DeleteSubscription(string mail)
        {
            dynamic response = new ExpandoObject();

            try
            {
                if (!MailManager.IsValidEmail(mail))
                {
                    response.status = "400";
                    response.message = "Die E-Mail Adresse ist nicht gültig.";
                    return response;
                }

                // Check if we already have a subscription. In that case, activate the sub
                var subscription = _db.DailyPaperSubscriptions.FirstOrDefault(d => d.Email.ToLower() == mail.ToLower());

                if (subscription == default)
                    response.message = $"Kein Abonnement unter der Adresse {mail} gefunden.";
                else
                {
                    subscription.Active = false;
                    await _db.SaveChangesAsync();
                    response.message = $"Abonnement erfolgreich deaktiviert!";
                }
            }
            catch (Exception ex)
            {
                response.message = "Unbekannter Fehler, Abonnement konnte nicht deaktiviert werden.";
                _logger.LogError(ex, "Error unsubbing the daily paper:");
            }

            return response.message;
        }

        [HttpPost("/api/DailyPaperController/PostSubscription/{mail}")]
        public async Task<IActionResult> PostSubscription(string mail)
        {
            dynamic response = new ExpandoObject();

            try
            {
                if (!MailManager.IsValidEmail(mail))
                {
                    response.status = "400";
                    response.message = "Die E-Mail Adresse ist nicht gültig.";
                    return response;
                }

                // Check if we already have a subscription. In that case, activate the sub
                var subscription = _db.DailyPaperSubscriptions.FirstOrDefault(d => d.Email.ToLower() == mail.ToLower());

                var baseUrl = Request.Scheme + "://" + Request.Host;
                var unsubscribeUrl = baseUrl + "/api/DailyPaperController/DeleteSubscription/" + mail;

                if (subscription != default)
                {
                    if (subscription.Active) response.result = "Sie sind bereits aktiver Abonnent.";
                    else
                    {
                        subscription.Active = true;
                        response.result = "Ihr Abonnement wurde reaktiviert!";

                        await _db.SaveChangesAsync();

                        // Send a mail to verifiy it.
                        MailManager.SendMail("Ihr Neues vom Schürfer Abonnement!",
                            MailManager.CreateMailWithButtonHtml("Neues vom Schürfer",
                            "Willkommen zurück! Ihr Abonnement wurde reaktiviert. Sie erhalten eine Nachricht sobald " +
                            "es Neues vom Schürfer gibt.<br/>Sollten Sie Ihr Abonnement beenden wollen, klicken Sie " +
                            $"<a href='{unsubscribeUrl}'>hier</a>.",
                            "Jetzt lesen", baseUrl),
                            new List<string>() { mail });
                    }
                }
                else
                {
                    _db.DailyPaperSubscriptions.Add(new DailyPaperSubscription()
                    {
                        Active = true,
                        Email = mail,
                        InitialSubscriptionDate = DateTime.Now
                    });

                    await _db.SaveChangesAsync();

                    // Send a mail to verifiy it.
                    MailManager.SendMail("Ihr Neues vom Schürfer Abonnement!",
                        MailManager.CreateMailWithButtonHtml("Neues vom Schürfer",
                        "Willkommen! Ihr Abonnement wurde soeben aktiviert. Sie erhalten eine Nachricht sobald " +
                        "es Neues vom Schürfer gibt.<br/>Sollten Sie Ihr Abonnement beenden wollen, klicken Sie " +
                        $"<a href='{unsubscribeUrl}'>hier</a>.",
                        "Jetzt lesen", baseUrl),
                        new List<string>() { mail });

                    response.result = "Erfolgreich abonniert!";
                }

                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Unbekannter Fehler, Abonnement konnte nicht angelegt werden.";
                _logger.LogError(ex, "Error subbing the daily paper:");
            }

            return Json(response);
        }

        [HttpGet("/api/DailyPaperController/GetDailyPaperOfProtocol/{meetingAndPeriodNumber}")]
        public async Task<IActionResult> GetDailyPaperOfProtocol(string meetingAndPeriodNumber)
        {
            dynamic response = new ExpandoObject();

            try
            {
                var splited = meetingAndPeriodNumber.Split(",");
                var meetingNumber = int.Parse(splited[0]);
                var legislaturePeriod = int.Parse(splited[1]);

                var dailyPaperViewModel = _dailyPaperService.GetDailyPaperAsViewModel(meetingNumber, legislaturePeriod);

                response.result = await _viewRenderService.RenderToStringAsync("DailyPaper/_DailyPaperView", dailyPaperViewModel);
                response.status = "200";
            }
            catch (Exception ex)
            {
                response.status = "400";
                response.message = "Couldn't build the daily paper view model";
                _logger.LogError(ex, "Error fetching the daily paper:");
            }

            return Json(response);
        }
    }
}
