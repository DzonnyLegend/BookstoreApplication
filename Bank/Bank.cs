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

namespace Bank
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Bank : StatefulService, IBank
    {
        public Bank(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public async Task<List<string>> ListClients()
        {
            var usersDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<long, Customer>>("usersDictionary");
            var clients = new List<string>();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await usersDictionary.CreateEnumerableAsync(tx);
                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default))
                    {
                        clients.Add(enumerator.Current.Value.FullName);
                    }
                }
            }

            return clients;
        }

        public async Task EnlistMoneyTransfer(long userID, double amount)
        {
            var usersDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<long, Customer>>("usersDictionary");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await usersDictionary.TryGetValueAsync(tx, userID);
                if (result.HasValue && result.Value.AccountBalance >= amount)
                {
                    var updatedCustomer = result.Value;
                    updatedCustomer.AccountBalance -= amount;

                    await usersDictionary.SetAsync(tx, userID, updatedCustomer);
                    await tx.CommitAsync();
                }
                else
                {
                    throw new System.Exception("Nedovoljno sredstava ili korisnik ne postoji");
                }
            }
        }

        public async Task<string> GetValidClient(string user)
        {
            var usersDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<long, Customer>>("usersDictionary");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await usersDictionary.CreateEnumerableAsync(tx);
                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default))
                    {
                        if (enumerator.Current.Value.FullName == user)
                        {
                            return "Valid User";
                        }
                    }
                }
            }
            return "Invalid User";
        }

        public async Task<bool> Prepare()
        {
            await Task.CompletedTask; 
            return true;
        }

        public async Task<bool> Commit()
        {
            await Task.CompletedTask;
            return true;
        }

        public async Task<bool> Rollback()
        {
            await Task.CompletedTask; 
            return true;
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
    }
}
