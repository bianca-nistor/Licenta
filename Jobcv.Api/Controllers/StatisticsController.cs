using JobCv.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}/monthly")]
        public async Task<IActionResult> GetMonthlyStatistics(int userId, int? year, int? month)
        {
            var selectedYear = year ?? DateTime.UtcNow.Year;
            var selectedMonth = month ?? DateTime.UtcNow.Month;

            var startDate = new DateTime(selectedYear, selectedMonth, 1);
            var endDate = startDate.AddMonths(1);

            var applications = await _context.Applications
                .Where(x => x.UserId == userId &&
                            x.CreatedAt >= startDate &&
                            x.CreatedAt < endDate)
                .ToListAsync();

            var applicationsCount = applications.Count(x =>
                x.Status == "Applied" ||
                x.Status == "InterviewScheduled" ||
                x.Status == "InterviewCompleted" ||
                x.Status == "Rejected" ||
                x.Status == "Accepted" ||
                x.Status == "OfferReceived");

            var savedCount = applications.Count(x => x.Status == "Saved");

            var interviewsCount = applications.Count(x =>
                x.Status == "InterviewScheduled" ||
                x.Status == "InterviewCompleted");

            var rejectionsCount = applications.Count(x => x.Status == "Rejected");

            var offersCount = applications.Count(x =>
                x.Status == "OfferReceived" ||
                x.Status == "Accepted");

            var interviewRate = applicationsCount == 0
                ? 0
                : Math.Round((double)interviewsCount / applicationsCount * 100, 2);

            var offerRate = applicationsCount == 0
                ? 0
                : Math.Round((double)offersCount / applicationsCount * 100, 2);

            return Ok(new
            {
                Year = selectedYear,
                Month = selectedMonth,
                SavedCount = savedCount,
                ApplicationsCount = applicationsCount,
                InterviewsCount = interviewsCount,
                RejectionsCount = rejectionsCount,
                OffersCount = offersCount,
                InterviewRate = interviewRate,
                OfferRate = offerRate
            });
        }

        [HttpGet("user/{userId}/by-cv")]
        public async Task<IActionResult> GetStatisticsByCv(int userId)
        {
            var result = await _context.Cvs
                .Where(cv => cv.UserId == userId)
                .Select(cv => new
                {
                    CvId = cv.Id,
                    CvTitle = cv.Title,

                    ApplicationsCount = _context.Applications.Count(a =>
                        a.UserId == userId &&
                        a.CvId == cv.Id &&
                        a.Status != "Saved"),

                    InterviewsCount = _context.Applications.Count(a =>
                        a.UserId == userId &&
                        a.CvId == cv.Id &&
                        (a.Status == "InterviewScheduled" ||
                         a.Status == "InterviewCompleted")),

                    OffersCount = _context.Applications.Count(a =>
                        a.UserId == userId &&
                        a.CvId == cv.Id &&
                        (a.Status == "OfferReceived" ||
                         a.Status == "Accepted"))
                })
                .ToListAsync();

            var finalResult = result.Select(x => new
            {
                x.CvId,
                x.CvTitle,
                x.ApplicationsCount,
                x.InterviewsCount,
                x.OffersCount,
                InterviewRate = x.ApplicationsCount == 0
                    ? 0
                    : Math.Round((double)x.InterviewsCount / x.ApplicationsCount * 100, 2),
                OfferRate = x.ApplicationsCount == 0
                    ? 0
                    : Math.Round((double)x.OffersCount / x.ApplicationsCount * 100, 2)
            });

            return Ok(finalResult);
        }

        [HttpGet("user/{userId}/last-six-months")]
        public async Task<IActionResult> GetLastSixMonthsStatistics(int userId)
        {
            var today = DateTime.UtcNow;
            var startMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-5);

            var applications = await _context.Applications
                .Where(x => x.UserId == userId && x.CreatedAt >= startMonth)
                .ToListAsync();

            var result = Enumerable.Range(0, 6)
                .Select(i =>
                {
                    var date = startMonth.AddMonths(i);

                    var monthApplications = applications
                        .Where(x => x.CreatedAt.Year == date.Year &&
                                    x.CreatedAt.Month == date.Month)
                        .ToList();

                    var applicationsCount = monthApplications.Count(x => x.Status != "Saved");

                    var interviewsCount = monthApplications.Count(x =>
                        x.Status == "InterviewScheduled" ||
                        x.Status == "InterviewCompleted");

                    var offersCount = monthApplications.Count(x =>
                        x.Status == "OfferReceived" ||
                        x.Status == "Accepted");

                    return new
                    {
                        Year = date.Year,
                        Month = date.Month,
                        ApplicationsCount = applicationsCount,
                        InterviewsCount = interviewsCount,
                        OffersCount = offersCount
                    };
                });

            return Ok(result);
        }
    }
}