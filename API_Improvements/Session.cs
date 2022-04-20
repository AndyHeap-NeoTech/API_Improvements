using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace API_Improvements
{
    public enum AccessMode
    {
        Read,
        Write,
        Smart,  //Extra roundtrip or something to determine read or write routing 
        Simple, //Always goes to the Leader

        NumSessionQueryModes
    }

   public static class TaskExtension
    {
        //Extend the Task class so that chaining is more elegant when used. This is needed because of async/await.
        public static async Task Execute(this Task<ISessionChainMethods> inputTask)
        {
            await inputTask.Result.ExecuteAsync();
        }
    }
   

     
    public interface ISessionChainMethods
    {
        ISessionChainMethods Then(string query);
        ISessionChainMethods Then(string query, object parameters);
        ISessionChainMethods Then(string query, object parameters, Func<IResultCursor, Task> resultHandler);
        Task ExecuteAsync();
    }
    

    public interface ISession : ISessionChainMethods
    {
        ISessionChainMethods Run(string query, AccessMode mode = AccessMode.Simple);
        ISessionChainMethods Run(string query, object parameters, AccessMode mode = AccessMode.Simple);
        ISessionChainMethods Run(string query, object parameters, Func<IResultCursor, Task> resultHandler, AccessMode mode = AccessMode.Simple);

    }


    public class Session : ISession
    {
        //If we use newer versions of C# then this could be a record or something like that.
        private class OperationType
        {
            public OperationType(string query, object parameters, Func<IResultCursor, Task> resultHandler)
            {
                Query = query;
                Parameters = parameters;
                ResultHandler = resultHandler;
            }

            public string Query { get; }
            public object Parameters { get; }
            public Func<IResultCursor, Task> ResultHandler { get; }
        }

        private ITransaction Tx { get; set; }
        private IList<OperationType> OperationsCache { get; } = new List<OperationType>(); 
        private AccessMode Mode { get; set; } = AccessMode.Simple;


        /***************************************************************************/
        //    Run Methods
        /***************************************************************************/
        public ISessionChainMethods Run(
            string query,
            AccessMode mode = AccessMode.Simple)
        {
            Run(query, null, null, mode);
            return this;
        }

        public ISessionChainMethods Run(
            string query,
            object parameters,
            AccessMode mode = AccessMode.Simple)
        {
            Run(query, parameters, null, mode);
            return this;
        }

        public ISessionChainMethods Run(
            string query, 
            object parameters, 
            Func<IResultCursor, Task> resultHandler, 
            AccessMode mode = AccessMode.Simple)
        {
            Mode = mode;

            //Add to the cache...
            OperationsCache.Add(new OperationType(query, parameters, resultHandler));

            return this;
        }



        /***************************************************************************/
        //    Method Chaining
        /***************************************************************************/
        public ISessionChainMethods Then(string query)
        {
            Then(query, null, null);
            return this;
        }

        public ISessionChainMethods Then(
            string query,
            object parameters)

        {
            Then(query, parameters, null);
            return this;
        }

        public ISessionChainMethods Then(
            string query,
            object parameters,
            Func<IResultCursor, Task> resultHandler)
        {
            OperationsCache.Add(new OperationType(query, parameters, resultHandler));
            return this;
        }

        public async Task ExecuteAsync()
        {
            //Retry logic as found in the actual driver... not going to bother doing it here...
            //return TryExecuteAsync(_logger, async () => await _retryLogic.RetryAsync(async () =>
            {
                Tx = await BeginTransaction(Mode);
                try
                {
                    await ProcessCachedOperations();
                    
                    if (Tx.IsOpen())
                        await Tx.Commit();
                }
                catch
                {
                    if(Tx.IsOpen())
                        await Tx.RollBack();

                    throw;
                }
            }
            //).ConfigureAwait(false)); //End retry logic...

            //Clear the cache...
            OperationsCache.Clear();
        }

        async Task ProcessCachedOperations()
        {
            foreach (OperationType op in OperationsCache)
            {
                //Simplified from actual driver implementation for this example...
                var resultCursor = await Tx.RunAsync(op.Query, op.Parameters).ConfigureAwait(false);

                if (op.ResultHandler != null)
                {
                    await op.ResultHandler(resultCursor);
                }

                resultCursor.Consume(); //Not sure if this should be done or not.... 
            }
        }



        /***************************************************************************/
        //    Support Methods
        /***************************************************************************/
        async Task<ITransaction> BeginTransaction(AccessMode mode)
        {
            var tx = new Transaction(mode);
            await tx.Begin().ConfigureAwait(false);
            return tx;
        }
    }
}
