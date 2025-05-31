using BookSwap.Models;
using System.ComponentModel.DataAnnotations;

namespace BookSwap.ViewModels
{
    public class ExchangeCreateViewModel
    {
        [Required]
        public int OfferedBookId { get; set; }
        public Book OfferedBook { get; set; }

        [Required]
        public int RequestedBookId { get; set; }
        public Book RequestedBook { get; set; }
    }
}
