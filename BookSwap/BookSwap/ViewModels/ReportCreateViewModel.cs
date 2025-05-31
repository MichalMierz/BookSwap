using BookSwap.Models;
using System.ComponentModel.DataAnnotations;

namespace BookSwap.ViewModels
{
    public class ReportCreateViewModel
    {
        // Jeżeli raport dotyczy książki
        public int? BookId { get; set; }

        // Jeżeli raport dotyczy użytkownika
        public string ReportedUserId { get; set; }

        [Required]
        [Display(Name = "Rodzaj zgłoszenia")]
        public ReportType Type { get; set; }

        [Required, StringLength(1000)]
        [Display(Name = "Opis szczegółowy")]
        public string Description { get; set; }
    }
}
