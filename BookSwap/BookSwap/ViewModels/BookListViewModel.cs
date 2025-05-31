using BookSwap.Models;
using System.Collections.Generic;

namespace BookSwap.ViewModels
{
    public class BookListViewModel
    {
        public IEnumerable<Book> Books { get; set; }
        public IEnumerable<Genre> Genres { get; set; }

        public string SearchTitle { get; set; }
        public string SearchAuthor { get; set; }
        public int? SelectedGenreId { get; set; }
        public BookCondition? SelectedCondition { get; set; }
    }
}
