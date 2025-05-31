using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookSwap.Models;

namespace BookSwap.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Dodajemy DbSety naszych encji:
        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Konfiguracje relacji, usuwanie kaskadowe itp.

            // 1) Book → ApplicationUser
            builder.Entity<Book>()
                .HasOne(b => b.Owner)
                .WithMany(u => u.Books)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2) Exchange → OfferedBook, RequestedBook (bez kaskadowego usuwania)
            builder.Entity<Exchange>()
                .HasOne(e => e.OfferedBook)
                .WithMany()
                .HasForeignKey(e => e.OfferedBookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Exchange>()
                .HasOne(e => e.RequestedBook)
                .WithMany()
                .HasForeignKey(e => e.RequestedBookId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3) Exchange → ApplicationUser (Initiator, Recipient)
            builder.Entity<Exchange>()
                .HasOne(e => e.Initiator)
                .WithMany(u => u.ExchangesInitiated)
                .HasForeignKey(e => e.InitiatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Exchange>()
                .HasOne(e => e.Recipient)
                .WithMany(u => u.ExchangesReceived)
                .HasForeignKey(e => e.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4) Report → Reporter (ApplicationUser)
            builder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()//u => u.ReportsMade)
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5) Report → Book
            builder.Entity<Report>()
                .HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.SetNull);

            // 6) Report → ReportedUser (ApplicationUser)
            builder.Entity<Report>()
                .HasOne(r => r.ReportedUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // 7) Seeding wstępnych danych (gatunków)
            builder.Entity<Genre>().HasData(
                new Genre { Id = 1, Name = "Fantastyka" },
                new Genre { Id = 2, Name = "Kryminał" },
                new Genre { Id = 3, Name = "Romans" },
                new Genre { Id = 4, Name = "Literatura faktu" },
                new Genre { Id = 5, Name = "Nauka i edukacja" }
            );
        }
    }
}
