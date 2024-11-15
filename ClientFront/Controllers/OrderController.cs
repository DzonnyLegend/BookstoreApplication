using ClientFront.Models;
using CommonLibrary;
using Microsoft.AspNetCore.Mvc;

namespace ClientFront.Controllers
{
    public class OrderController : Controller
    {
        private List<Order> orders;
        private BookDatabase bookDatabase;
        private CustomerDatabase customerDatabase;

        public OrderController(BookDatabase bookDatabase, CustomerDatabase customerDatabase)
        {
            orders = new List<Order>();
            this.bookDatabase = bookDatabase;
            this.customerDatabase = customerDatabase;
        }

        public bool AddOrder(Order order)
        {
            // Proveravamo da li klijent postoji
            if (!customerDatabase.Customers.ContainsKey(order.ClientId))
            {
                Console.WriteLine($"Klijent sa ID-jem {order.ClientId} ne postoji.");
                return false;
            }

            // Proveravamo da li knjiga postoji
            if (!bookDatabase.Books.ContainsKey(order.BookId))
            {
                Console.WriteLine($"Knjiga sa ID-jem {order.BookId} ne postoji.");
                return false;
            }

            // Proveravamo da li ima dovoljno knjiga na stanju
            if (bookDatabase.Books[order.BookId].Quantity < order.Quantity)
            {
                Console.WriteLine($"Nema dovoljno knjiga na stanju za ID {order.BookId}.");
                return false;
            }

            // Smanjujemo količinu knjiga u bazi i dodajemo narudžbinu
            bookDatabase.Books[order.BookId].Quantity -= (uint)order.Quantity;
            order.TotalPrice = (double)(bookDatabase.Books[order.BookId].Price * order.Quantity);
            orders.Add(order);

            // Ažuriramo balans korisnika
            var customer = customerDatabase.Customers[order.ClientId];
            if (customer.AccountBalance < order.TotalPrice)
            {
                Console.WriteLine($"Korisnik sa ID-jem {order.ClientId} nema dovoljno sredstava.");
                return false;
            }
            customer.AccountBalance -= order.TotalPrice;

            Console.WriteLine($"Narudžbina uspešno dodata za klijenta {customer.FullName}.");
            return true;
        }

        public Order? GetOrderById(long orderId)
        {
            return orders.FirstOrDefault(o => o.OrderId == orderId);
        }

        public List<Order> GetAllOrders()
        {
            return orders;
        }

        public bool UpdateOrderQuantity(long orderId, uint newQuantity)
        {
            var order = GetOrderById(orderId);
            if (order == null)
            {
                Console.WriteLine($"Narudžbina sa ID-jem {orderId} ne postoji.");
                return false;
            }

            // Proveravamo da li ima dovoljno knjiga na stanju za novu količinu
            var book = bookDatabase.Books[order.BookId];
            int availableQuantity = (int)(book.Quantity + (int)order.Quantity); // Vraćamo prethodnu količinu
            if (availableQuantity < newQuantity)
            {
                Console.WriteLine($"Nema dovoljno knjiga na stanju za ID {order.BookId} za novu količinu.");
                return false;
            }

            // Ažuriramo količinu
            book.Quantity = (uint)(availableQuantity - (int)newQuantity);
            order.Quantity = newQuantity;
            order.TotalPrice = (double)(book.Price * order.Quantity);

            Console.WriteLine($"Narudžbina {orderId} uspešno ažurirana.");
            return true;
        }
    }
}
