using BookSwap.Data;
using BookSwap.Models;
using BookSwap.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookSwap.Controllers
{
    [Authorize(Roles = "User,Moderator")]
    public class ExchangesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExchangesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Exchanges/Create?offeredBookId=1&requestedBookId=2
        public async Task<IActionResult> Create(int offeredBookId, int requestedBookId)
        {
            var offeredBook = await _context.Books
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.Id == offeredBookId && b.Status == BookStatus.Dostępna);
            var requestedBook = await _context.Books
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.Id == requestedBookId && b.Status == BookStatus.Dostępna);

            if (offeredBook == null || requestedBook == null)
                return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (offeredBook.OwnerId != currentUserId)
                return Forbid();

            if (requestedBook.OwnerId == currentUserId)
            {
                ModelState.AddModelError("", "Nie możesz wymienić książki z samym sobą.");
                return RedirectToAction("Index", "Books");
            }

            var vm = new ExchangeCreateViewModel
            {
                OfferedBook = offeredBook,
                RequestedBook = requestedBook
            };
            return View(vm);
        }

        // POST: /Exchanges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExchangeCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var offeredBook = await _context.Books.FindAsync(model.OfferedBookId);
            var requestedBook = await _context.Books.FindAsync(model.RequestedBookId);

            if (offeredBook == null || requestedBook == null ||
                offeredBook.OwnerId != currentUserId ||
                offeredBook.Status != BookStatus.Dostępna ||
                requestedBook.Status != BookStatus.Dostępna)
            {
                ModelState.AddModelError("", "Wystąpił błąd. Sprawdź dostępność książek.");
                return View(model);
            }

            var exchange = new Exchange
            {
                InitiatorId = currentUserId,
                RecipientId = requestedBook.OwnerId,
                OfferedBookId = offeredBook.Id,
                RequestedBookId = requestedBook.Id,
                Status = ExchangeStatus.Oczekująca
            };

            offeredBook.Status = BookStatus.WOczekiwaniu;
            requestedBook.Status = BookStatus.WOczekiwaniu;

            _context.Exchanges.Add(exchange);
            _context.Books.Update(offeredBook);
            _context.Books.Update(requestedBook);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyExchanges));
        }

        // GET: /Exchanges/MyExchanges
        public async Task<IActionResult> MyExchanges()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var exchanges = await _context.Exchanges
                .Include(e => e.OfferedBook).ThenInclude(b => b.Owner)
                .Include(e => e.RequestedBook).ThenInclude(b => b.Owner)
                .Where(e => e.InitiatorId == currentUserId || e.RecipientId == currentUserId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(exchanges);
        }

        // GET: /Exchanges/Respond/5
        public async Task<IActionResult> Respond(int id)
        {
            var exchange = await _context.Exchanges
                .Include(e => e.OfferedBook).ThenInclude(b => b.Owner)
                .Include(e => e.RequestedBook).ThenInclude(b => b.Owner)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (exchange == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (exchange.RecipientId != currentUserId && !User.IsInRole("Moderator"))
                return Forbid();

            return View(exchange);
        }

        // POST: /Exchanges/Respond/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(int id, string action)
        {
            var exchange = await _context.Exchanges
                .Include(e => e.OfferedBook)
                .Include(e => e.RequestedBook)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (exchange == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (exchange.RecipientId != currentUserId && !User.IsInRole("Moderator"))
                return Forbid();

            if (action == "accept")
            {
                exchange.Status = ExchangeStatus.Zaakceptowana;
                exchange.UpdatedAt = DateTime.UtcNow;

                exchange.OfferedBook.Status = BookStatus.WWymianie;
                exchange.RequestedBook.Status = BookStatus.WWymianie;
            }
            else if (action == "reject")
            {
                exchange.Status = ExchangeStatus.Odrzucona;
                exchange.UpdatedAt = DateTime.UtcNow;

                exchange.OfferedBook.Status = BookStatus.Dostępna;
                exchange.RequestedBook.Status = BookStatus.Dostępna;
            }
            else
            {
                return BadRequest();
            }

            _context.Exchanges.Update(exchange);
            _context.Books.Update(exchange.OfferedBook);
            _context.Books.Update(exchange.RequestedBook);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyExchanges));
        }

        // GET: /Exchanges/Complete/5
        public async Task<IActionResult> Complete(int id)
        {
            var exchange = await _context.Exchanges
                .Include(e => e.OfferedBook)
                .Include(e => e.RequestedBook)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (exchange == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (exchange.InitiatorId != currentUserId && exchange.RecipientId != currentUserId && !User.IsInRole("Moderator"))
                return Forbid();

            exchange.Status = ExchangeStatus.Zakończona;
            exchange.UpdatedAt = DateTime.UtcNow;

            // Zamiana właścicieli
            var tempOwnerId = exchange.OfferedBook.OwnerId;
            exchange.OfferedBook.OwnerId = exchange.RequestedBook.OwnerId;
            exchange.RequestedBook.OwnerId = tempOwnerId;

            // Przywrócenie statusu
            exchange.OfferedBook.Status = BookStatus.Dostępna;
            exchange.RequestedBook.Status = BookStatus.Dostępna;

            _context.Exchanges.Update(exchange);
            _context.Books.Update(exchange.OfferedBook);
            _context.Books.Update(exchange.RequestedBook);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyExchanges));
        }
    }
}
