using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VarsityTracker.Core.Models;
using VarsityTracker.Web.Data;

namespace VarsityTracker.Web.Controllers
{
    public class PlayersController : Controller
    {
        private readonly VarsityDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PlayersController(VarsityDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Players
        public async Task<IActionResult> Index(int? facultyId, string? search)
        {
            var query = _context.Players
                .Include(p => p.Faculty)
                .AsQueryable();

            if (facultyId.HasValue && facultyId.Value > 0)
            {
                query = query.Where(p => p.FacultyId == facultyId.Value);
                ViewData["CurrentFacultyId"] = facultyId.Value;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.FullName.ToLower().Contains(search.ToLower()) 
                                      || p.MatricNo.ToLower().Contains(search.ToLower())
                                      || p.PositionRole.ToLower().Contains(search.ToLower()));
                ViewData["CurrentSearch"] = search;
            }

            ViewBag.Faculties = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyCode", facultyId);

            var players = await query.OrderBy(p => p.Faculty!.FacultyCode).ThenBy(p => p.FullName).ToListAsync();
            return View(players);
        }

        // GET: Players/ExportCsv
        public async Task<IActionResult> ExportCsv()
        {
            var players = await _context.Players
                .Include(p => p.Faculty)
                .OrderBy(p => p.Faculty!.FacultyCode)
                .ThenBy(p => p.FullName)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("PlayerId,FullName,MatricNo,FacultyCode,FacultyName,PositionRole");

            foreach (var p in players)
            {
                sb.AppendLine($"{p.PlayerId},\"{p.FullName}\",\"{p.MatricNo}\",\"{p.Faculty?.FacultyCode}\",\"{p.Faculty?.FacultyName}\",\"{p.PositionRole}\"");
            }

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"UTHM_Varsity_Athletes_{DateTime.Now:yyyyMMdd}.csv");
        }

        // GET: Players/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players
                .Include(p => p.Faculty)
                .FirstOrDefaultAsync(m => m.PlayerId == id);

            if (player == null) return NotFound();

            return View(player);
        }

        // GET: Players/Create
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> Create(int? facultyId)
        {
            ViewBag.FacultyId = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyName", facultyId);
            return View(new Player { FacultyId = facultyId ?? 0 });
        }

        // POST: Players/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> Create([Bind("FacultyId,FullName,MatricNo,PositionRole,PhotoUrl")] Player player, IFormFile? photoFile)
        {
            if (await _context.Players.AnyAsync(p => p.MatricNo.ToLower() == player.MatricNo.ToLower()))
            {
                ModelState.AddModelError("MatricNo", "A player with this Matric Number is already registered.");
            }

            if (photoFile != null && photoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "players");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"player_{Guid.NewGuid():N}{Path.GetExtension(photoFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }
                player.PhotoUrl = $"/uploads/players/{uniqueFileName}";
            }

            if (ModelState.IsValid)
            {
                _context.Add(player);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Player '{player.FullName}' added to roster.";
                return RedirectToAction(nameof(Index), new { facultyId = player.FacultyId });
            }

            ViewBag.FacultyId = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyName", player.FacultyId);
            return View(player);
        }

        // GET: Players/Edit/5
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players.FindAsync(id);
            if (player == null) return NotFound();

            ViewBag.FacultyId = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyName", player.FacultyId);
            return View(player);
        }

        // POST: Players/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> Edit(int id, [Bind("PlayerId,FacultyId,FullName,MatricNo,PositionRole,PhotoUrl")] Player player, IFormFile? photoFile)
        {
            if (id != player.PlayerId) return NotFound();

            if (photoFile != null && photoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "players");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"player_{Guid.NewGuid():N}{Path.GetExtension(photoFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }
                player.PhotoUrl = $"/uploads/players/{uniqueFileName}";
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(player);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Player roster details for '{player.FullName}' updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Players.Any(e => e.PlayerId == player.PlayerId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index), new { facultyId = player.FacultyId });
            }

            ViewBag.FacultyId = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyName", player.FacultyId);
            return View(player);
        }

        // GET: Players/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players
                .Include(p => p.Faculty)
                .FirstOrDefaultAsync(m => m.PlayerId == id);

            if (player == null) return NotFound();

            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                int facultyId = player.FacultyId;
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Player '{player.FullName}' removed from team roster.";
                return RedirectToAction(nameof(Index), new { facultyId });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

