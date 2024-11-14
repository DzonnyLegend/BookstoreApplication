using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonLibrary;
using CommonLibrary.Interface;
using CommonLibrary.Model;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using static System.Reflection.Metadata.BlobBuilder;

namespace BookstoreService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class BookstoreService : StatefulService, IBookstoreService
    {
        private BookDatabase bookDatabase = new BookDatabase();
        private CustomerDatabase customerDatabase = new CustomerDatabase();

        public BookstoreService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public async Task<double?> GetItemPrice(long bookID)
        {
            if (bookDatabase.Books.TryGetValue(bookID, out Book? book))
            {
                return await Task.FromResult(book.Price);
            }
            return null;
        }

        public async Task<string?> GetBook(long bookID)
        {
            if (bookDatabase.Books.TryGetValue(bookID, out Book? book))
            {
                return await Task.FromResult($"{book.Title} by {book.Author}");
            }
            return null;
        }

        public async Task<List<string>> GetAllBooks()
        {
            return await Task.FromResult(bookDatabase.Books.Values.Select(book => $"{book.Title} by {book.Author}").ToList());
        }

        public async Task<bool> EnlistPurchase(long bookID, uint count, long customerId)
        {
            if (bookDatabase.Books.TryGetValue(bookID, out Book? book) && book.Quantity >= count)
            {
                double totalPrice = (double)(book.Price ?? 0.0) * count;

                if (customerDatabase.Customers.TryGetValue(customerId, out Customer? customer) && customer.AccountBalance >= totalPrice)
                {
                    customer.AccountBalance -= totalPrice;
                    book.Quantity -= (uint)count;

                    ServiceEventSource.Current.ServiceMessage(this.Context, $"Korisnik {customerId} je kupio knjigu {bookID} u količini {count} za ukupno {totalPrice}.");

                    return await Task.FromResult(true);
                }
            }

            return await Task.FromResult(false);
        }


        public async Task<Dictionary<long, Book>?> GetAvailableBooks()
        {
            return await Task.FromResult<Dictionary<long, Book>?>(bookDatabase.Books);
        }

        public async Task<double?> GetCustomerBalance(long customerId)
        {
            if (customerDatabase.Customers.TryGetValue(customerId, out Customer? customer))
            {
                return await Task.FromResult(customer.AccountBalance);
            }
            return null;
        }

        public async Task<bool> UpdateCustomerBalance(long customerId, double amount)
        {
            if (customerDatabase.Customers.TryGetValue(customerId, out Customer? customer))
            {
                if (customer.AccountBalance + amount >= 0)
                {
                    customer.AccountBalance += amount;
                    return await Task.FromResult(true);
                }
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> Prepare()
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, "Priprema transakcije u BookstoreService.");
            return await Task.FromResult(true);
        }

        public async Task<bool> Commit()
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, "Commit transakcije u BookstoreService.");
            return await Task.FromResult(true);
        }

        public async Task<bool> Rollback()
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, "Rollback transakcije u BookstoreService.");
            return await Task.FromResult(true);
        }
    }
}
