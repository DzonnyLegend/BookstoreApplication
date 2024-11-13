using System;
using System.Collections.Generic;
using System.Fabric.Query;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CommonLibrary.Interface
{
    [ServiceContract]
    public interface ITransactionService : IService
    {
        [OperationContract]
        Task<bool> Prepare();

        [OperationContract]
        Task<bool> Commit();

        [OperationContract]
        Task<bool> Rollback();

    }
}
