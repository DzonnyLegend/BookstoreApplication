using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.Model;

namespace CommonLibrary.Interface
{
    [ServiceContract]
    public interface IBookstoreService : ITransactionService
    {
        [OperationContract]
        Task<Dictionary<long, Book>?> GetAvailableBooks();

        [OperationContract]
        Task<bool> EnlistPurchase(long bookID, uint count, long customerId);

        [OperationContract]
        Task<double?> GetItemPrice(long bookID);

        [OperationContract]
        Task<string?> GetBook(long bookID);

        [OperationContract]
        Task<List<string>> GetAllBooks();
    }
}
