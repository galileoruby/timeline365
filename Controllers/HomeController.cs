using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TimeLine365.Application.DTOs;
using TimeLine365.Application.Interfaces;
using TimeLine365.Models;

namespace TimeLine365.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITimelineService _timelineService;

        public HomeController(ILogger<HomeController> logger, ITimelineService timelineService)
        {
            _logger = logger;
            _timelineService = timelineService;
        }

        public async Task<IActionResult> Index([FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? day, CancellationToken cancellationToken)
        {
            var request = new TimelineFilterRequest(year, month, day);
            var timeline = await _timelineService.GetTimelineAsync(request, cancellationToken);

            var viewModel = new TimelineIndexViewModel
            {
                Timeline = timeline
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
