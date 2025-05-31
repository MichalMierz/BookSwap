using BookSwap.Data;
using BookSwap.Models;
using BookSwap.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookSwap.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Reports/Create?bookId=5 lub ?reportedUserId=abc
        public async Task<IActionResult> Create(int? bookId, string reportedUserId = null)
        {
            var model = new ReportCreateViewModel
            {
                BookId = bookId,
                ReportedUserId = reportedUserId
            };
            return View(model);
        }

        // POST: /Reports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReportCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var report = new Report
            {
                ReporterId = currentUserId,
                BookId = model.BookId,
                ReportedUserId = model.ReportedUserId,
                Type = model.Type,
                Description = model.Description
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Reports/Index (tylko Moderator)
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Book).ThenInclude(b => b.Owner)
                .Include(r => r.ReportedUser)
                .Where(r => !r.IsResolved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reports);
        }

        // GET: /Reports/Details/5
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Book).ThenInclude(b => b.Owner)
                .Include(r => r.ReportedUser)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return NotFound();
            return View(report);
        }

        // POST: /Reports/Resolve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> Resolve(int id, bool hideBook, bool blockUser)
        {
            var report = await _context.Reports
                .Include(r => r.Book)
                .Include(r => r.ReportedUser)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();

            report.IsResolved = true;
            report.ActionTaken_HideBook = hideBook;
            report.ActionTaken_BlockUser = blockUser;
            report.ResolvedAt = DateTime.UtcNow;
            report.ResolvedByModeratorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (hideBook && report.Book != null)
            {
                // Przykładowo zmieniamy status książki na niedostępny
                report.Book.Status = BookStatus.Dostępna; // lub dodajesz pole IsHidden = true
            }

            if (blockUser && report.ReportedUser != null)
            {
                report.ReportedUser.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            }

            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
