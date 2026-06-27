using Microsoft.AspNetCore.Mvc;
using TimeLine365.Application.DTOs;
using TimeLine365.Application.Interfaces;
using TimeLine365.Domain.Entities;
using TimeLine365.Models;

namespace TimeLine365.Controllers
{
    public class TimelineEventsController : Controller
    {
        private readonly ITimelineService _timelineService;

        public TimelineEventsController(ITimelineService timelineService)
        {
            _timelineService = timelineService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var events = await _timelineService.GetEventsAsync(cancellationToken);

            var viewModel = new TimelineEventsIndexViewModel
            {
                StatusMessage = TempData["StatusMessage"] as string,
                Events = events
                    .Select(MapListItem)
                    .ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
        {
            var item = await _timelineService.GetEventAsync(id, cancellationToken);

            if (item is null)
            {
                return NotFound();
            }

            return View(MapDetails(item));
        }

        public IActionResult Create()
        {
            return View(new TimelineEventFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TimelineEventFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var id = await _timelineService.CreateEventAsync(
                    new CreateTimelineEventRequest(
                        model.Year,
                        model.Month,
                        model.Day,
                        model.Title,
                        model.Description,
                        model.MediaUrl,
                        model.ReferenceUrl,
                        model.SortOrder),
                    cancellationToken);

                TempData["StatusMessage"] = "Timeline event created.";

                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
        {
            var item = await _timelineService.GetEventAsync(id, cancellationToken);

            if (item is null)
            {
                return NotFound();
            }

            return View(MapForm(item));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TimelineEventFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var updated = await _timelineService.UpdateEventAsync(
                    new UpdateTimelineEventRequest(
                        model.Id,
                        model.Year,
                        model.Month,
                        model.Day,
                        model.Title,
                        model.Description,
                        model.MediaUrl,
                        model.ReferenceUrl,
                        model.SortOrder),
                    cancellationToken);

                if (!updated)
                {
                    return NotFound();
                }

                TempData["StatusMessage"] = "Timeline event updated.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var item = await _timelineService.GetEventAsync(id, cancellationToken);

            if (item is null)
            {
                return NotFound();
            }

            return View(MapDetails(item));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(long id, CancellationToken cancellationToken)
        {
            var deleted = await _timelineService.DeleteEventAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            TempData["StatusMessage"] = "Timeline event deleted.";

            return RedirectToAction(nameof(Index));
        }

        private static TimelineEventFormViewModel MapForm(TimelineEvent item)
        {
            return new TimelineEventFormViewModel
            {
                Id = item.Id,
                Year = item.Year,
                Month = item.Month,
                Day = item.Day,
                Title = item.Title,
                Description = item.Description,
                MediaUrl = item.MediaUrl,
                ReferenceUrl = item.ReferenceUrl,
                SortOrder = item.SortOrder
            };
        }

        private static TimelineEventListItemViewModel MapListItem(TimelineEvent item)
        {
            return new TimelineEventListItemViewModel
            {
                Id = item.Id,
                DateLabel = FormatDateLabel(item),
                Title = item.Title,
                SortOrder = item.SortOrder
            };
        }

        private static TimelineEventDetailsViewModel MapDetails(TimelineEvent item)
        {
            return new TimelineEventDetailsViewModel
            {
                Id = item.Id,
                DateLabel = FormatDateLabel(item),
                Year = item.Year,
                Month = item.Month,
                Day = item.Day,
                Title = item.Title,
                Description = item.Description,
                MediaUrl = item.MediaUrl,
                ReferenceUrl = item.ReferenceUrl,
                SortOrder = item.SortOrder
            };
        }

        private static string FormatDateLabel(TimelineEvent item)
        {
            return item.Day.HasValue
                ? $"{item.Year:D4}-{item.Month:D2}-{item.Day:D2}"
                : item.Month.HasValue
                    ? $"{item.Year:D4}-{item.Month:D2}"
                    : $"{item.Year:D4}";
        }
    }
}
