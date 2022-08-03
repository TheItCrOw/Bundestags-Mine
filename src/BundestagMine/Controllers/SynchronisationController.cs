using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BundestagMine.MongoDB;
using BundestagMine.SqlDatabase;
using BundestagMine.Synchronisation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BundestagMine.Controllers
{
    [Route("api/SynchronisationController")]
    [ApiController]
    public class SynchronisationController : Controller
    {
        private readonly BundestagMineDbContext _db;

        public SynchronisationController(BundestagMineDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns the amount of entities
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/SynchronisationController/GetEntityCount/{type}")]
        public async Task<IActionResult> GetEntityCount(string type)
        {
            try
            {
                var count = 0;
                switch (type)
                {
                    case "protocols":
                        count = _db.Protocols.Count();
                        break;
                    case "deputies":
                        count = _db.Deputies.Count();
                        break;
                    case "networkdatas":
                        count = _db.NetworkDatas.Count();
                        break;
                    case "speeches":
                        count = _db.Speeches.Count();
                        break;
                    default:
                        break;
                }
                return Json(new
                {
                    status = "200",
                    count
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "400",
                    message = $"Couldn't poll {type}, error {ex}. Connectionstring {_db.Database.GetDbConnection().ConnectionString}",
                });
            }
        }

        /// <summary>
        /// Starts the import of a database entity from mongodb to sql
        /// </summary>
        /// <returns></returns>
        [HttpPost("/api/SynchronisationController/StartImport/{type}")]
        public async Task<IActionResult> StartImport(string type)
        {
            try
            {
                if (MongoDBConnection.Connect(out var mongoDb))
                {
                    var exporter = new MongoDBExporter(mongoDb);
                    switch (type)
                    {
                        case "protocols":
                            exporter.ExportProtocols();
                            break;
                        case "deputies":
                            exporter.ExportDeputies();
                            break;
                        case "networkdatas":
                            exporter.ExportNetworkData();
                            break;
                        case "speeches":
                            exporter.ExportSpeeches();
                            break;
                        default:
                            break;
                    }

                    return Json(new
                    {
                        status = "200",
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = "400",
                        message = "Couldn't connet to mongodb."
                    });
                }

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "400",
                    message = $"Couldn't import {type}, error {ex}.",
                });
            }
        }
    }
}
