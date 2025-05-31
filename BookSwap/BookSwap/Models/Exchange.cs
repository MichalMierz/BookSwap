using System;
using System.ComponentModel.DataAnnotations;

namespace BookSwap.Models
{
    public enum ExchangeStatus
    {
        Oczekująca,   // czeka na akceptację/odrzucenie
        Zaakceptowana,
        Odrzucona,
        Zakończona    // po finalizacji
    }

    public class Exchange
    {
        [Key]
        public int Id { get; set; }

        // Kto zainicjował wymianę
        [Required]
        public string InitiatorId { get; set; }
        public ApplicationUser Initiator { get; set; }

        // Do kogo – odbiorca propozycji
        [Required]
        public string RecipientId { get; set; }
        public ApplicationUser Recipient { get; set; }

        // Książka oferowana przez inicjatora
        [Required]
        public int OfferedBookId { get; set; }
        public Book OfferedBook { get; set; }

        // Książka oczekiwana od odbiorcy
        [Required]
        public int RequestedBookId { get; set; }
        public Book RequestedBook { get; set; }

        // Status wymiany
        [Required]
        public ExchangeStatus Status { get; set; } = ExchangeStatus.Oczekująca;

        // Data utworzenia propozycji
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Data aktualizacji statusu
        public DateTime? UpdatedAt { get; set; }
    }
}
