using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookSwap.Models
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        // Nawigacja do książek
        public ICollection<Book> Books { get; set; }
    }
}
