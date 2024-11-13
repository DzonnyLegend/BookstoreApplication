using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using CommonLibrary.Interface;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace TransactionCoordinator
{
    internal sealed class TransactionCoordinator : StatelessService, ITransactionCoordinator
    {
        public TransactionCoordinator(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners(); // Koristi CreateServiceRemotingInstanceListeners za StatelessService
        }

        // Implementacija metoda definisanih u ITransactionCoordinator

        public async Task<List<string>> GetAllBooks()
        {
            // Primer: poziv odgovarajućeg servisa da dobije sve knjige
            IBookstoreService proxy = ServiceProxy.Create<IBookstoreService>(new Uri("fabric:/BookstoreTransaction/BookstoreService"), new ServicePartitionKey(1));
            return await proxy.GetAllBooks();
        }

        public async Task<List<string>> GetAllClients()
        {
            // Primer: poziv odgovarajućeg servisa da dobije sve klijente
            IBank proxy = ServiceProxy.Create<IBank>(new Uri("fabric:/BookstoreTransaction/Bank"), new ServicePartitionKey(1));
            return await proxy.ListClients();
        }

        public async Task<string?> GetBook(long bookID)
        {
            // Primer: poziv odgovarajućeg servisa da dobije podatke o knjizi po ID-u
            IBookstoreService proxy = ServiceProxy.Create<IBookstoreService>(new Uri("fabric:/BookstoreTransaction/BookstoreService"), new ServicePartitionKey(1));
            return await proxy.GetBook(bookID);
        }

        public async Task<string> GetValidClient(string client)
        {
            // Primer: poziv odgovarajućeg servisa da proveri validnost klijenta
            IBank proxy = ServiceProxy.Create<IBank>(new Uri("fabric:/BookstoreTransaction/Bank"), new ServicePartitionKey(1));
            return await proxy.GetValidClient(client);
        }

        public async Task<bool> Prepare(long bookID, long userID, uint count)
        {
            // Kreiramo proxy objekte za Bank i Bookstore servise
            IBank bankProxy = ServiceProxy.Create<IBank>(new Uri("fabric:/BookstoreTransaction/Bank"), new ServicePartitionKey(1));
            IBookstoreService bookProxy = ServiceProxy.Create<IBookstoreService>(new Uri("fabric:/BookstoreTransaction/BookstoreService"), new ServicePartitionKey(1));

            // Dobijamo cenu knjige
            var bookPrice = await bookProxy.GetItemPrice(bookID);

            // Rezervacija novca i knjige
            await bankProxy.EnlistMoneyTransfer(userID, (double)bookPrice * count);
            await bookProxy.EnlistPurchase(bookID, count);

            // Proveravamo da li su oba servisa uspešno pripremila transakciju
            if (await bankProxy.Prepare() && await bookProxy.Prepare())
            {
                return true;
            }

            return false;
        }

        public async Task<bool> Commit()
        {
            // Potvrđujemo (commit-ujemo) i za Bookstore i za Bank
            IBank bankProxy = ServiceProxy.Create<IBank>(new Uri("fabric:/BookstoreTransaction/Bank"), new ServicePartitionKey(1));
            IBookstoreService bookProxy = ServiceProxy.Create<IBookstoreService>(new Uri("fabric:/BookstoreTransaction/BookstoreService"), new ServicePartitionKey(1));

            if (await bankProxy.Commit() && await bookProxy.Commit())
            {
                return true;
            }

            return false;
        }

        public async Task<bool> Rollback()
        {
            // Vraćamo transakciju (rollback) i za Bookstore i za Bank
            IBank bankProxy = ServiceProxy.Create<IBank>(new Uri("fabric:/BookstoreTransaction/Bank"), new ServicePartitionKey(1));
            IBookstoreService bookProxy = ServiceProxy.Create<IBookstoreService>(new Uri("fabric:/BookstoreTransaction/BookstoreService"), new ServicePartitionKey(1));

            await bankProxy.Rollback();
            await bookProxy.Rollback();

            return true;
        }


        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Primer: osnovni kod za stalni zadatak
            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
