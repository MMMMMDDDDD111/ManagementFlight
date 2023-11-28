using FlightManagement.Models;
using FlightManagement.Models.Management_Flight;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace FlightManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoard : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DashBoard(ApplicationDBContext context)
        {
            _context = context;
        }
        [HttpGet("{id1}/{id2}")]
        public IActionResult GetDashboard(int id1, int id2)
        {
            DateTime currentDateTime = DateTime.Now;

            var recentlyActivities = _context.DocumentInfo
                .Include(di => di.AddFlight)
                .Where(di => di.Status == "Sent" &&
                             di.AddFlight.Date.HasValue &&
                             currentDateTime >= di.AddFlight.Date.Value &&
                             currentDateTime <= di.AddFlight.Date.Value.AddHours(2))
                .Select(di => new RecentlyActivityDTO
                {
                    DocumentName = di.Documentname,
                    DocumentType = di.Documenttype,
                    FlightNo = di.AddFlight.Flightno,
                    DepartureDate = di.AddFlight.Date.Value,
                    Creator = di.Creator,
                    UpdateDate = (DateTime)(di.Status == "Sent" ? (DateTime)(di.AddFlight.Date ?? DateTime.MinValue) : di.UpdateDate)
                })
                .ToList();

            var currentFlights = _context.Addflights
              .Where(cf => cf.FlightId == id1 || cf.FlightId == id2)
              .AsEnumerable()
              .Select(cf =>
              {
                  var arrivalTimes = _context.Addflights
                      .Where(innerCf => (innerCf.FlightId == id1 || innerCf.FlightId == id2) && innerCf.Date < cf.Date)
                      .OrderByDescending(innerCf => innerCf.Date)
                      .Take(2)
                      .AsEnumerable()
                      .Select(d => (TimeSpan?)(cf.Date - d.Date))
                      .Reverse()
                      .Take(2)
                      .ToList();

                  var aggregatedTime = arrivalTimes.Aggregate(TimeSpan.Zero, (acc, time) => acc.Add(time ?? TimeSpan.Zero));

                  var sentFiles = _context.DocumentInfo
                      .Select(di => new DocumentInfoDTO
                      {
                          ID = di.Id,
                          DocumentName = di.Documentname,
                          DocumentType = di.Documenttype,
                          FileName = di.FileName
                          
                      })
                      .ToList();

                  var returnedFiles = _context.DocumentInfo
                      .Select(di => new DocumentInfoDTO
                      {
                          ID = di.Id,
                          DocumentName = di.Documentname,
                          DocumentType = di.Documenttype,
                          FileName = di.FileName
                      })
                      .ToList();

                  return new CurrentFlightDTO
                  {
                      FlightId = cf.FlightId,
                      DepartureTime = cf.Date ?? DateTime.MinValue,
                      ArrivalTime = aggregatedTime,
                      SentFiles = sentFiles,
                      ReturnedFiles = returnedFiles
                  };
              })
              .ToList();

            var dashboard = new DashboardDTO
            {
                RecentlyActivities = recentlyActivities,
                CurrentFlights = currentFlights
            };

            return new JsonResult(new { Data = dashboard });
        }
    }
}
