using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using CommonLibrary.Interface;
using CommonLibrary.Model;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using CommonLibrary;


namespace ValidationService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ValidationService : StatelessService, IValidationService
    {
        private BookDatabase bookDatabase = new BookDatabase();
        private CustomerDatabase customerDatabase = new CustomerDatabase();

        public ValidationService(StatelessServiceContext context)
            : base(context)
        { }

        public async Task<List<string>> GetAllBooks()
        {
            return await Task.FromResult(bookDatabase.Books.Values.Select(book => $"{book.Title} by {book.Author}").ToList());
        }

        public async Task<List<string>> GetAllClients()
        {
            return await Task.FromResult(customerDatabase.Customers.Values.Select(customer => customer.FullName).ToList());
        }

        public async Task<string?> GetBook(long bookID)
        {
            if (bookDatabase.Books.TryGetValue(bookID, out Book? book))
            {
                return await Task.FromResult(book != null ? $"{book.Title} by {book.Author}" : "Book not found");

            }
            return null;
        }

        public async Task<string> GetValidClient(long customerId)
        {
            if (customerDatabase.Customers.TryGetValue(customerId, out Customer? customer) && customer != null)
            {
                if (customer.AccountBalance > 0)
                {
                    return await Task.FromResult(customer.FullName!);
                }
            }

            return await Task.FromResult(string.Empty);
        }

        public async Task<bool> Validation(Book book)
        {
            bool isBookValid = book != null && !string.IsNullOrEmpty(book.Title) && book.Price > 0 && book.Quantity > 0;
            return await Task.FromResult(isBookValid);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

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
