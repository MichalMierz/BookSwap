using System;
using System.ComponentModel.DataAnnotations;

namespace BookSwap.Models
{
    public enum BookCondition
    {
        Nowa,
        UżywanaDobry,
        UżywanaPrzeciętny,
        Uszkodzona
    }

    public enum BookStatus
    {
        Dostępna,        // można składać propozycje wymiany
        WOczekiwaniu,    // ktoś złożył propozycję wymiany – zablokowana
        WWymianie,       // wymiana zaakceptowana, w trakcie realizacji
        Zakończona       // wymiana zakończona (po oddaniu)
    }

    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required, StringLength(150)]
        public string Author { get; set; }

        // Relacja do gatunku
        [Display(Name = "Gatunek")]
        public int GenreId { get; set; }
        public Genre Genre { get; set; }

        // Stan książki
        [Required]
        public BookCondition Condition { get; set; }

        // Ścieżka do pliku okładki (np. "/images/covers/abc.jpg")
        public string CoverImagePath { get; set; }

        // Status dostępności
        [Required]
        public BookStatus Status { get; set; } = BookStatus.Dostępna;

        // Właściciel książki (ApplicationUser.Id)
        [Required]
        public string OwnerId { get; set; }
        public ApplicationUser Owner { get; set; }

        // Data dodania wpisu
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
