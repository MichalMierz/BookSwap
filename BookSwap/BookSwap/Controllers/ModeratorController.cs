using BookSwap.Data;
using BookSwap.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookSwap.Controllers
{
    [Authorize(Roles = "Moderator")]
    public class ModeratorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModeratorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Moderator/Reports
        public async Task<IActionResult> Reports()
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

        // GET: /Moderator/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: /Moderator/Books
        public async Task<IActionResult> Books()
        {
            var books = await _context.Books
                .Include(b => b.Owner)
                .Include(b => b.Genre)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(books);
        }

        // POST: /Moderator/BlockUser
        [HttpPost]
        public async Task<IActionResult> BlockUser(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();
            user.LockoutEnd = System.DateTimeOffset.UtcNow.AddYears(100);
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Users));
        }

        // POST: /Moderator/UnblockUser
        [HttpPost]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();
            user.LockoutEnd = System.DateTimeOffset.UtcNow;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Users));
        }

        // POST: /Moderator/HideBook
        [HttpPost]
        public async Task<IActionResult> HideBook(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return NotFound();
            book.Status = BookStatus.Dostępna; // lub oznaczenie jako ukryta
            _context.Update(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Books));
        }

        // POST: /Moderator/UnhideBook
        [HttpPost]
        public async Task<IActionResult> UnhideBook(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return NotFound();
            book.Status = BookStatus.Dostępna;
            _context.Update(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Books));
        }
    }
}
