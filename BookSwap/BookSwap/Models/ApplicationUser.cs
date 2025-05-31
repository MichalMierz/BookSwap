using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookSwap.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Dodatkowe pola profilu (opcjonalnie)
        [PersonalData]
        [Required]
        public string FirstName { get; set; }

        [PersonalData]
        [Required]
        public string LastName { get; set; }

        // Nawigacja do książek dodanych przez użytkownika
        public ICollection<Book> Books { get; set; }

        // Nawigacja do wymian (propozycji) utworzonych przez użytkownika
        public ICollection<Exchange> ExchangesInitiated { get; set; }

        // Nawigacja do wymian, w których jest druga strona (odbiorca propozycji)
        public ICollection<Exchange> ExchangesReceived { get; set; }

        // Nawigacja do zgłoszeń dokonanych przez użytkownika
        public ICollection<Report> ReportsMade { get; set; }
    }
}
