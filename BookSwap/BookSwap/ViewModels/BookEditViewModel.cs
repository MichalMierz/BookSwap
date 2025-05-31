using BookSwap.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookSwap.ViewModels
{
    public class BookEditViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required, StringLength(150)]
        public string Author { get; set; }

        [Required]
        [Display(Name = "Gatunek")]
        public int GenreId { get; set; }

        [Required]
        [Display(Name = "Stan książki")]
        public BookCondition Condition { get; set; }

        // Istniejąca okładka (ścieżka)
        public string ExistingCoverPath { get; set; }

        [Display(Name = "Nowa okładka (opcjonalnie)")]
        public IFormFile CoverImage { get; set; }
    }
}
