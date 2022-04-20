using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Improvements
{
    public interface ITransaction
    {
        Task Begin();
        Task Commit();
        Task RollBack();
        bool IsOpen();
        Task<IResultCursor> RunAsync(string query);
        Task<IResultCursor> RunAsync(string query, object parameters);
    }

    public class Transaction : ITransaction
    {
        public Transaction(AccessMode mode)
        {

        }

        public async Task Begin()
        {
            await Task.CompletedTask;
        }

        public async Task Commit()
        {
            await Task.CompletedTask;
        }

        public async Task RollBack()
        {
            await Task.CompletedTask;
        }

        public async Task<IResultCursor> RunAsync(string query)
        {
            await Task.CompletedTask;
            return new ResultCursor();
        }

        public async Task<IResultCursor> RunAsync(string query, object parameters)
        {
            await Task.CompletedTask;
            return new ResultCursor();
        }

        public bool IsOpen()
        {
            return true;
        }
    }
}
