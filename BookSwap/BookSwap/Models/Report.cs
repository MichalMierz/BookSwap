using System;
using System.ComponentModel.DataAnnotations;

namespace BookSwap.Models
{
    public enum ReportType
    {
        NieodpowiedniaTreść,
        NieuczciweZachowanie,
        Inne
    }

    public class Report
    {
        [Key]
        public int Id { get; set; }

        // Kto zgłasza
        [Required]
        public string ReporterId { get; set; }
        public ApplicationUser Reporter { get; set; }

        // Jeżeli raport dotyczy książki
        public int? BookId { get; set; }
        public Book? Book { get; set; }

        // Jeżeli raport dotyczy użytkownika
        public string? ReportedUserId { get; set; }
        public ApplicationUser? ReportedUser { get; set; }

        [Required]
        public ReportType Type { get; set; }

        [Required, StringLength(1000)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;
        public bool? ActionTaken_HideBook { get; set; }
        public bool? ActionTaken_BlockUser { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedByModeratorId { get; set; }
        public ApplicationUser? ResolvedByModerator { get; set; }
    }
}
