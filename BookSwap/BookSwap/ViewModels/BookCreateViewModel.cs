using BookSwap.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookSwap.ViewModels
{
    public class BookCreateViewModel
    {
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

        [Display(Name = "Okładka (jpg, png)")]
        public IFormFile CoverImage { get; set; }
    }
}
