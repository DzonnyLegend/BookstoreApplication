using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using CommonLibrary.Interface;
using CommonLibrary.Model;
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
            return this.CreateServiceRemotingInstanceListeners(); 
        }

        public async Task<List<string>> GetAllBooks()
        {
            IBookstoreService proxy = ServiceProxy.Create<IBookstoreService>(new Uri("fabric:/BookstoreTransaction/BookstoreService"), new ServicePartitionKey(1));
            return await proxy.GetAllBooks();
        }

        public async Task<List<string>> GetAllClients()
        {
            IBank proxy = ServiceProxy.Create<IBank>(new Uri("fabric:/BookstoreTransaction/Bank"), new ServicePartitionKey(1));
            return await proxy.ListClients();
        }

        public async Task<string?> GetBook(long bookID)
        {
            IBookstoreService proxy = ServiceProxy.Create<IBookstoreService>(new Uri("fabric:/BookstoreTransaction/BookstoreService"), new ServicePartitionKey(1));
            return await proxy.GetBook(bookID);
        }

        public async Task<string> GetValidClient(string client)
        {
            IBank proxy = ServiceProxy.Create<IBank>(new Uri("fabric:/BookstoreTransaction/Bank"), new ServicePartitionKey(1));
            return await proxy.GetValidClient(client);
        }

        public async Task<bool> CompleteTransaction(long bookID, uint count, long customerId)
        {
            if (await Prepare(bookID, customerId, count, customerId))
            {
                if (await Commit())
                {
                    return true;
                }
                else
                {
                    await Rollback();
                }
            }
            else
            {
                await Rollback();
            }

            return false;
        }

        public async Task<bool> Prepare(long bookID, long userID, uint count, long customerId)
        {
            IBank bankProxy = ServiceProxy.Create<IBank>(new Uri("fabric:/BookstoreTransaction/Bank"), new ServicePartitionKey(1));
            IBookstoreService bookProxy = ServiceProxy.Create<IBookstoreService>(new Uri("fabric:/BookstoreTransaction/BookstoreService"), new ServicePartitionKey(1));

            var bookPrice = await bookProxy.GetItemPrice(bookID);

            await bankProxy.EnlistMoneyTransfer(userID, (double)bookPrice * count);
            await bookProxy.EnlistPurchase(bookID, count, customerId);

            if (await bankProxy.Prepare() && await bookProxy.Prepare())
            {
                return true;
            }

            return false;
        }

        public async Task<bool> Commit()
        {
            try
            {
                var bookService = ServiceProxy.Create<IBookstoreService>(new Uri("fabric:/BookstoreTransaction/BookstoreService"), new ServicePartitionKey(1));
                var bankService = ServiceProxy.Create<IBank>(new Uri("fabric:/BookstoreTransaction/Bank"), new ServicePartitionKey(1));

                if (await bankService.Commit() && await bookService.Commit())
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, "Transakcija uspešno potvrđena.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Greška pri potvrđivanju transakcije: {ex.Message}");
            }

            return false;
        }

        public async Task<bool> Rollback()
        {
            try
            {
                var bookService = ServiceProxy.Create<IBookstoreService>(new Uri("fabric:/BookstoreTransaction/BookstoreService"), new ServicePartitionKey(1));
                var bankService = ServiceProxy.Create<IBank>(new Uri("fabric:/BookstoreTransaction/Bank"), new ServicePartitionKey(1));

                if (await bankService.Rollback() && await bookService.Rollback())
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, "Transakcija uspešno poništena.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Greška pri poništavanju transakcije: {ex.Message}");
            }

            return false;
        }


        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
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
