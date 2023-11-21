using FlightManagement.Models;
using FlightManagement.Models.Management_Flight;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoard : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DashBoard(ApplicationDBContext context) => _context = context;
        [HttpGet]
        public IActionResult GetDashboard()
        {
            var recentlyActivities = _context.DocumentInfo
                .Include(di => di.AddFlight)
                .Where(di => di.Status == "Sent")
                .Select(di => new RecentlyActivityDTO
                {
                    DocumentName = di.Documentname,
                    DocumentType = di.Documenttype,
                    FlightNo = di.AddFlight.Flightno,
                    DepartureDate = (DateTime)di.AddFlight.Date,
                    Creator = di.Creator,
                    UpdateDate = (DateTime)(di.Status == "Sent" ? (DateTime)(di.AddFlight.Date ?? DateTime.MinValue) : di.UpdateDate)
                })
                .ToList();

            var currentFlights = _context.Addflights
                 .Select(cf => new CurrentFlightDTO
                 {
                     FlightId = cf.FlightId,
                     DepartureTime = cf.Date ?? DateTime.MinValue, // Use DateTime.MinValue if Date is null
                     SentFiles = cf.DocumentInformation
                         .Where(di => di.Status == "Sent")
                         .Select(di => new DocumentInfoDTO
                         {
                             DocumentName = di.Documentname,
                             DocumentType = di.Documenttype,
                             FileName = di.FileName
                         })
                         .ToList(),
                     ReturnedFiles = cf.DocumentInformation
                         .Where(di => di.Status == "Returned")
                         .Select(di => new DocumentInfoDTO
                         {
                             DocumentName = di.Documentname,
                             DocumentType = di.Documenttype,
                             FileName = di.FileName
                         })
                         .ToList()
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
