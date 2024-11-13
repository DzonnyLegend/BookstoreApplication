using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Model
{
    [DataContract]
    public class Book
    {
        [DataMember]
        public long BookId { get; set; }
        [DataMember]
        public string? Title { get; set; }
        [DataMember]
        public string? Author { get; set; }
        [DataMember]
        public double? Price { get; set; }
        [DataMember]
        public string? Genre { get; set; }
        [DataMember]
        public uint Quantity { get; set; }
        [DataMember]
        public string? Description { get; set; }
        [DataMember]
        public uint Count { get; set; } = 1;

        public Book(long bookId, string? bookTitle, string? description, string? author, double? price, uint quantity)
        {
            BookId = bookId;
            Title = bookTitle;
            Description = description;
            Author = author;
            Price = price;
            Quantity = quantity;
        }

        public Book() { }

        public override string? ToString()
        {
            return base.ToString();
        }

    }
}
