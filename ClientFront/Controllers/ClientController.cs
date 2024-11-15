using ClientFront.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClientFront.Controllers
{
    public class ClientController : Controller
    {
        private List<Client> clients;

        public ClientController()
        {
            clients = new List<Client>();
        }

        public void AddClient(Client client)
        {
            clients.Add(client);
        }

        public Client? GetClientById(long clientId)
        {
            return clients.FirstOrDefault(c => c.ClientId == clientId);
        }

        public List<Client> GetAllClients()
        {
            return clients;
        }

        public bool UpdateClientBalance(long clientId, double newBalance)
        {
            var client = GetClientById(clientId);
            if (client != null)
            {
                client.AccountBalance = newBalance;
                return true;
            }
            return false;
        }
    }
}
