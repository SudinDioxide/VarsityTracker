using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VarsityTracker.Core.Models;
using VarsityTracker.Web.Data;

namespace VarsityTracker.Web.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly VarsityDbContext _context;

        public FeedbackController(VarsityDbContext context)
        {
            _context = context;
        }

        // GET: /Feedback
        public async Task<IActionResult> Index()
        {
            var query = _context.VenueFeedbacks
                .Include(f => f.Venue)
                .Include(f => f.User)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                int userId = GetCurrentUserId();
                query = query.Where(f => f.UserId == userId);
            }

            var tickets = await query.OrderByDescending(f => f.SubmittedAt).ToListAsync();
            return View(tickets);
        }

        // GET: Feedback/ExportCsv
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCsv()
        {
            var list = await _context.VenueFeedbacks
                .Include(f => f.Venue)
                .Include(f => f.User)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("FeedbackId,Venue,Subject,Details,Status,ReportedBy,SubmittedAt");

            foreach (var f in list)
            {
                var safeDetails = f.Details?.Replace("\"", "\"\"") ?? "";
                sb.AppendLine($"{f.FeedbackId},\"{f.Venue?.VenueName}\",\"{f.Subject}\",\"{safeDetails}\",\"{f.Status}\",\"{f.User?.FullName}\",\"{f.SubmittedAt:yyyy-MM-dd HH:mm}\"");
            }

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"UTHM_Venue_Safety_Hazard_Log_{DateTime.Now:yyyyMMdd}.csv");
        }

        // GET: /Feedback/Create
        public async Task<IActionResult> Create(int? venueId)
        {
            ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", venueId);
            return View(new VenueFeedback { VenueId = venueId ?? 0 });
        }

        // POST: /Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,Subject,Details")] VenueFeedback feedback)
        {
            feedback.UserId = GetCurrentUserId();
            feedback.SubmittedAt = DateTime.Now;
            feedback.Status = "Pending";

            if (ModelState.IsValid)
            {
                _context.Add(feedback);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue safety/hazard feedback submitted. The sports administration will inspect the facility.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", feedback.VenueId);
            return View(feedback);
        }

        // POST: /Feedback/UpdateStatus (Admin action)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int feedbackId, string newStatus, bool closeVenue = false)
        {
            var feedback = await _context.VenueFeedbacks
                .Include(f => f.Venue)
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

            if (feedback != null)
            {
                feedback.Status = newStatus;

                if (closeVenue && feedback.Venue != null)
                {
                    feedback.Venue.IsAvailable = false;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Feedback #{feedback.FeedbackId} updated to '{newStatus}'" + (closeVenue ? " and venue was closed for repairs." : ".");
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Feedback/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var feedback = await _context.VenueFeedbacks.FindAsync(id);
            if (feedback != null)
            {
                if (!User.IsInRole("Admin") && feedback.UserId != GetCurrentUserId())
                {
                    return Forbid();
                }

                _context.VenueFeedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Feedback report withdrawn.";
            }

            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentUserId()
        {
            string? idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out int id) ? id : 1;
        }
    }
}

