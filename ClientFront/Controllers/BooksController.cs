using Microsoft.AspNetCore.Mvc;
using CommonLibrary.Model;
using CommonLibrary;

namespace ClientFront.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookDatabase _bookDatabase;

        public BooksController(BookDatabase bookDatabase)
        {
            _bookDatabase = bookDatabase;
        }

        public IActionResult Index()
        {
            // Prikazuje sve dostupne knjige
            var books = _bookDatabase.Books.Values.ToList();
            return View(books);
        }

        public IActionResult Details(long id)
        {
            // Prikazuje detalje o odabranoj knjizi
            if (_bookDatabase.Books.TryGetValue(id, out var book))
            {
                return View(book);
            }

            return NotFound();
        }

        public IActionResult Payment()
        {
            // Prikazuje stranicu za plaćanje
            return View();
        }
    }
}
