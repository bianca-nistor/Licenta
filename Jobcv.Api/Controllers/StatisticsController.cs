using JobCv.Api.Data;
using JobCv.Api.Dtos;
using JobCv.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        private static readonly string[] AppliedOrFurtherStatuses =
        {
            "Applied",
            "Interview Scheduled",
            "Interview Done",
            "Rejected",
            "Offer",
            "Accepted",
            "Withdrawn",

           
            "InterviewScheduled",
            "InterviewCompleted",
            "OfferReceived"
        };

        private static readonly string[] InterviewStatuses =
        {
            "Interview Scheduled",
            "Interview Done",
            "InterviewScheduled",
            "InterviewCompleted"
        };

        private static readonly string[] OfferStatuses =
        {
            "Offer",
            "Accepted",
            "OfferReceived"
        };

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId:int}/dashboard-activity")]
        public async Task<ActionResult<DashboardActivityStatsDto>> GetDashboardActivity(
            int userId,
            [FromQuery] string? period)
        {
            var (startDate, selectedPeriodText) = ResolvePeriod(period);

            var createdCvsCountQuery = _context.Cvs.Where(x => x.UserId == userId);
            var uploadedCvsCountQuery = _context.UploadedCvFiles.Where(x => x.UserId == userId);
            var applicationsAddedCountQuery = _context.JobApplications.Where(x => x.UserId == userId);
            var interviewsCountQuery = _context.JobApplications.Where(x =>
                x.UserId == userId &&
                (x.InterviewAt.HasValue || InterviewStatuses.Contains(x.Status)));

            if (startDate.HasValue)
            {
                var start = startDate.Value;

                createdCvsCountQuery = createdCvsCountQuery.Where(x => x.CreatedAt >= start);
                uploadedCvsCountQuery = uploadedCvsCountQuery.Where(x => x.UploadedAt >= start);
                applicationsAddedCountQuery = applicationsAddedCountQuery.Where(x => x.CreatedAt >= start);
                interviewsCountQuery = interviewsCountQuery.Where(x =>
                    (x.InterviewAt.HasValue && x.InterviewAt.Value >= start) ||
                    (!x.InterviewAt.HasValue && x.CreatedAt >= start));
            }

            return Ok(new DashboardActivityStatsDto
            {
                Period = selectedPeriodText,
                Summary = selectedPeriodText == "all time"
                    ? "Showing activity for all time."
                    : $"Showing activity for {selectedPeriodText}.",
                CreatedCvsCount = await createdCvsCountQuery.CountAsync(),
                UploadedCvsCount = await uploadedCvsCountQuery.CountAsync(),

                
                ApplicationsCount = await applicationsAddedCountQuery.CountAsync(),
                InterviewsCount = await interviewsCountQuery.CountAsync()
            });
        }

        [HttpGet("user/{userId:int}/application-flow")]
        public async Task<IActionResult> GetApplicationFlow(
            int userId,
            [FromQuery] string? period)
        {
            var (startDate, selectedPeriodText) = ResolvePeriod(period);

            var query = _context.JobApplications.Where(x => x.UserId == userId);

            if (startDate.HasValue)
            {
                var start = startDate.Value;
                query = query.Where(x => x.CreatedAt >= start);
            }

            var applications = await query.ToListAsync();

            var savedCount = applications.Count(x => x.Status == "Saved");
            var interviewsCount = applications.Count(x =>
                x.InterviewAt.HasValue || InterviewStatuses.Contains(x.Status));
            var offersCount = applications.Count(x => OfferStatuses.Contains(x.Status));
            var appliedCount = applications.Count(x => x.Status == "Applied");
            var totalAddedCount = applications.Count;
            var appliedOrFurtherCount = applications.Count(x => AppliedOrFurtherStatuses.Contains(x.Status));

            return Ok(new
            {
                Period = selectedPeriodText,
                TotalAddedCount = totalAddedCount,
                SavedCount = savedCount,
                AppliedCount = appliedCount,
                AppliedOrFurtherCount = appliedOrFurtherCount,
                InterviewsCount = interviewsCount,
                OffersCount = offersCount
            });
        }

        [HttpGet("user/{userId:int}/monthly")]
        public async Task<IActionResult> GetMonthlyStatistics(int userId, int? year, int? month)
        {
            var selectedYear = year ?? DateTime.UtcNow.Year;
            var selectedMonth = month ?? DateTime.UtcNow.Month;

            var startDate = new DateTime(selectedYear, selectedMonth, 1);
            var endDate = startDate.AddMonths(1);

            var applications = await _context.JobApplications
                .Where(x => x.UserId == userId &&
                            x.CreatedAt >= startDate &&
                            x.CreatedAt < endDate)
                .ToListAsync();

            var applicationsCount = applications.Count(x => AppliedOrFurtherStatuses.Contains(x.Status));
            var savedCount = applications.Count(x => x.Status == "Saved");
            var interviewsCount = applications.Count(x =>
                x.InterviewAt.HasValue || InterviewStatuses.Contains(x.Status));
            var rejectionsCount = applications.Count(x => x.Status == "Rejected");
            var offersCount = applications.Count(x => OfferStatuses.Contains(x.Status));

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

        [HttpGet("user/{userId:int}/by-cv")]
        public async Task<IActionResult> GetStatisticsByCv(int userId)
        {
            var result = await _context.Cvs
                .Where(cv => cv.UserId == userId)
                .Select(cv => new
                {
                    CvId = cv.Id,
                    CvTitle = cv.Title,

                    ApplicationsCount = _context.JobApplications.Count(a =>
                        a.UserId == userId &&
                        a.CvId == cv.Id &&
                        AppliedOrFurtherStatuses.Contains(a.Status)),

                    InterviewsCount = _context.JobApplications.Count(a =>
                        a.UserId == userId &&
                        a.CvId == cv.Id &&
                        (a.InterviewAt.HasValue || InterviewStatuses.Contains(a.Status))),

                    OffersCount = _context.JobApplications.Count(a =>
                        a.UserId == userId &&
                        a.CvId == cv.Id &&
                        OfferStatuses.Contains(a.Status))
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

        [HttpGet("user/{userId:int}/last-six-months")]
        public async Task<IActionResult> GetLastSixMonthsStatistics(int userId)
        {
            var today = DateTime.UtcNow;
            var startMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-5);

            var applications = await _context.JobApplications
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

                    var applicationsCount = monthApplications.Count(x => AppliedOrFurtherStatuses.Contains(x.Status));
                    var interviewsCount = monthApplications.Count(x =>
                        x.InterviewAt.HasValue || InterviewStatuses.Contains(x.Status));
                    var offersCount = monthApplications.Count(x => OfferStatuses.Contains(x.Status));

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

        private static (DateTime? StartDate, string PeriodText) ResolvePeriod(string? period)
        {
            var normalizedPeriod = (period ?? "last30").Trim().ToLowerInvariant();
            var todayUtc = DateTime.UtcNow.Date;

            return normalizedPeriod switch
            {
                "last7" or "7" or "last-7-days" => (todayUtc.AddDays(-7), "last 7 days"),
                "last90" or "90" or "last-90-days" => (todayUtc.AddDays(-90), "last 90 days"),
                "all" or "alltime" or "all-time" => (null, "all time"),
                _ => (todayUtc.AddDays(-30), "last 30 days")
            };
        }
    }
}
