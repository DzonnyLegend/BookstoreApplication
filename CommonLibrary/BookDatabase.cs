using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.Model;
using CommonLibrary;

namespace CommonLibrary
{
    public class BookDatabase
    {
        public Dictionary<long, Book> Books { get; private set; } = new Dictionary<long, Book>
        {
            { 1, new Book(1, "The Great Gatsby", "A novel set in the Roaring Twenties", "F. Scott Fitzgerald", 10.0, 3) },
            { 2, new Book(2, "1984", "A dystopian novel about totalitarianism", "George Orwell", 15.0, 5) },
            { 3, new Book(3, "To Kill a Mockingbird", "A story about racial inequality", "Harper Lee", 12.5, 4) },
            { 4, new Book(4, "Pride and Prejudice", "A romantic novel", "Jane Austen", 8.0, 6) },
            { 5, new Book(5, "Moby Dick", "A story about the quest for a giant whale", "Herman Melville", 18.0, 2) }
        };
    }
}
