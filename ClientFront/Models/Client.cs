namespace ClientFront.Models
{
    public class Client
    {
        public long ClientId { get; set; }
        public string FullName { get; set; }
        public double AccountBalance { get; set; }

        public Client(long clientId, string fullName, double accountBalance)
        {
            ClientId = clientId;
            FullName = fullName;
            AccountBalance = accountBalance;
        }
    }
}
