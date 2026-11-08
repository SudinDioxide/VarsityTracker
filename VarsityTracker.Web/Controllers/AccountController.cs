using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VarsityTracker.Core.Models;
using VarsityTracker.Web.Data;

namespace VarsityTracker.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly VarsityDbContext _context;

        public AccountController(VarsityDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string identifier, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Matric / Staff ID and Password are required.");
                return View();
            }

            var user = await _context.Users
                .Include(u => u.Faculty)
                .FirstOrDefaultAsync(u => (u.MatricNo == identifier || u.Email == identifier) && u.PasswordHash == password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid Matric/Staff ID or password.");
                return View();
            }

            // ENFORCE BUSINESS RULE: Only Admin and Scorekeeper can log in to the Web App
            if (user.Role != "Admin" && user.Role != "Scorekeeper")
            {
                ModelState.AddModelError(string.Empty, 
                    "Access Restricted: Student accounts cannot log in via the Web Portal. Please use the Varsity Tracker Mobile App.");
                return View();
            }

            await SignInUser(user);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // Fast Demo Login for project demonstration & evaluation (Admin and Scorekeeper only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickLogin(string role)
        {
            if (role != "Admin" && role != "Scorekeeper")
            {
                TempData["ErrorMessage"] = "Access Restricted: Student accounts must access the system via the Varsity Tracker Mobile App.";
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .Include(u => u.Faculty)
                .FirstOrDefaultAsync(u => u.Role == role);

            if (user != null)
            {
                await SignInUser(user);
                TempData["SuccessMessage"] = $"Logged in successfully as {user.FullName} ({user.Role})";
            }

            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("MatricNo", user.MatricNo),
                new Claim("FacultyId", user.FacultyId?.ToString() ?? ""),
                new Claim("FacultyName", user.Faculty?.FacultyCode ?? "General")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = System.DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        // GET: /Account/Register
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Faculties = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyName");
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("MatricNo,FullName,Email,PasswordHash,FacultyId")] User user)
        {
            if (await _context.Users.AnyAsync(u => u.MatricNo == user.MatricNo))
            {
                ModelState.AddModelError("MatricNo", "A user with this Matric Number already exists.");
            }

            if (ModelState.IsValid)
            {
                user.Role = "Student"; // Self-registered accounts are Students
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await SignInUser(user);
                TempData["SuccessMessage"] = "Student account created successfully! Welcome to Varsity Tracker.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Faculties = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyName", user.FacultyId);
            return View(user);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

