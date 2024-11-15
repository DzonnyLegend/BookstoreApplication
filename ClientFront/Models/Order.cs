namespace ClientFront.Models
{
    public class Order
    {
        public long OrderId { get; set; }
        public long BookId { get; set; }
        public uint Quantity { get; set; }
        public double TotalPrice { get; set; }
        public long ClientId { get; set; }

        public Order(long orderId, long bookId, uint quantity, double totalPrice, long clientId)
        {
            OrderId = orderId;
            BookId = bookId;
            Quantity = quantity;
            TotalPrice = totalPrice;
            ClientId = clientId;
        }
    }
}
