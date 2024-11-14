using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.Model;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CommonLibrary.Interface
{
    [ServiceContract]
    public interface IValidationService : IService
    {
        [OperationContract]
        Task<bool> Validation(Book book);

        [OperationContract]
        Task<List<string>> GetAllBooks();

        [OperationContract]
        Task<string?> GetBook(long bookID);

        [OperationContract]
        Task<List<string>> GetAllClients();

        [OperationContract]
        Task<string> GetValidClient(long customerId);

    }
}
