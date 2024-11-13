using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private Dictionary<long, Book> books = new Dictionary<long, Book>
        {
            { 1, new Book(1, "The Great Gatsby", "A novel set in the Roaring Twenties", "F. Scott Fitzgerald", 10.0, 3) },
            { 2, new Book(2, "1984", "A dystopian novel about totalitarianism", "George Orwell", 15.0, 5) },
            { 3, new Book(3, "To Kill a Mockingbird", "A story about racial inequality", "Harper Lee", 12.5, 4) },
            { 4, new Book(4, "Pride and Prejudice", "A romantic novel", "Jane Austen", 8.0, 6) },
            { 5, new Book(5, "Moby Dick", "A story about the quest for a giant whale", "Herman Melville", 18.0, 2) }
        };





        public async Task<double?> GetItemPrice(long bookID)
        {
            if (books.TryGetValue(bookID, out Book? book))
            {
                return await Task.FromResult(book.Price);
            }
            return null;
        }

        public async Task<string?> GetBook(long bookID)
        {
            if (books.TryGetValue(bookID, out Book? book))
            {
                return await Task.FromResult($"{book.Title} by {book.Author}");
            }
            return null;
        }

        public async Task<List<string>> GetAllBooks()
        {
            return await Task.FromResult(books.Values.Select(book => $"{book.Title} by {book.Author}").ToList());
        }

        public async Task<bool> EnlistPurchase(long bookID, uint count)
        {
            if (books.TryGetValue(bookID, out Book? book) && book.Quantity >= count)
            {
                book.Quantity -= (uint)count;
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Rezervacija za kupovinu knjige {bookID} u količini {count}.");
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        public async Task<Dictionary<long, Book>?> GetAvailableBooks()
        {
            return await Task.FromResult<Dictionary<long, Book>?>(books);
        }

        // Implementacija metoda definisanih u ITransactionService 

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
