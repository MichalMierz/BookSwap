using BookSwap.Data;
using BookSwap.Models;
using BookSwap.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookSwap.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: /Books/Index (lista dostępnych książek + filtrowanie)
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchTitle, string searchAuthor, int? genreId, BookCondition? condition)
        {
            var query = _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Owner)
                .Where(b => b.Status == BookStatus.Dostępna)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTitle))
                query = query.Where(b => b.Title.Contains(searchTitle));
            if (!string.IsNullOrEmpty(searchAuthor))
                query = query.Where(b => b.Author.Contains(searchAuthor));
            if (genreId.HasValue)
                query = query.Where(b => b.GenreId == genreId);
            if (condition.HasValue)
                query = query.Where(b => b.Condition == condition);

            var books = await query.ToListAsync();

            var genres = await _context.Genres.ToListAsync();

            var vm = new BookListViewModel
            {
                Books = books,
                Genres = genres,
                SearchTitle = searchTitle,
                SearchAuthor = searchAuthor,
                SelectedGenreId = genreId,
                SelectedCondition = condition
            };

            return View(vm);
        }
        // GET: /Books/MyBooks
        [Authorize(Roles = "User,Moderator")]
        public async Task<IActionResult> MyBooks()
        {
            // Pobierz ID zalogowanego użytkownika
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Pobierz wszystkie książki, których właścicielem jest aktualny user
            var myBooks = await _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Owner)
                .Where(b => b.OwnerId == userId)
                .ToListAsync();

            return View(myBooks);
        }

        // GET: /Books/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // GET: /Books/Create
        [Authorize(Roles = "User,Moderator")]
        public async Task<IActionResult> Create()
        {
            ViewData["Genres"] = new SelectList(await _context.Genres.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: /Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Moderator")]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.CoverImage != null && model.CoverImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/covers");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CoverImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImage.CopyToAsync(fileStream);
                    }
                }

                var book = new Book
                {
                    Title = model.Title,
                    Author = model.Author,
                    GenreId = model.GenreId,
                    Condition = model.Condition,
                    CoverImagePath = uniqueFileName is null ? null : "/images/covers/" + uniqueFileName,
                    Status = BookStatus.Dostępna,
                    OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Genres"] = new SelectList(await _context.Genres.ToListAsync(), "Id", "Name", model.GenreId);
            return View(model);
        }

        // GET: /Books/Edit/5
        [Authorize(Roles = "User,Moderator")]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            // Tylko właściciel lub moderator
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (book.OwnerId != currentUserId && !User.IsInRole("Moderator"))
                return Forbid();

            var vm = new BookEditViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                GenreId = book.GenreId,
                Condition = book.Condition,
                ExistingCoverPath = book.CoverImagePath
            };
            ViewData["Genres"] = new SelectList(await _context.Genres.ToListAsync(), "Id", "Name", book.GenreId);
            return View(vm);
        }

        // POST: /Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Moderator")]
        public async Task<IActionResult> Edit(int id, BookEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (book.OwnerId != currentUserId && !User.IsInRole("Moderator"))
                return Forbid();

            if (ModelState.IsValid)
            {
                if (model.CoverImage != null && model.CoverImage.Length > 0)
                {
                    if (!string.IsNullOrEmpty(book.CoverImagePath))
                    {
                        var oldPath = Path.Combine(_hostEnvironment.WebRootPath, book.CoverImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/covers");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CoverImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImage.CopyToAsync(fileStream);
                    }
                    book.CoverImagePath = "/images/covers/" + uniqueFileName;
                }

                book.Title = model.Title;
                book.Author = model.Author;
                book.GenreId = model.GenreId;
                book.Condition = model.Condition;

                _context.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Genres"] = new SelectList(await _context.Genres.ToListAsync(), "Id", "Name", model.GenreId);
            return View(model);
        }

        // GET: /Books/Delete/5
        [Authorize(Roles = "User,Moderator")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (book.OwnerId != currentUserId && !User.IsInRole("Moderator"))
                return Forbid();

            return View(book);
        }

        // POST: /Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Moderator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (book.OwnerId != currentUserId && !User.IsInRole("Moderator"))
                return Forbid();

            if (!string.IsNullOrEmpty(book.CoverImagePath))
            {
                var filePath = Path.Combine(_hostEnvironment.WebRootPath, book.CoverImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
