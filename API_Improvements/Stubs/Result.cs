using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Improvements
{
    public interface IRecord
    {
        string Contents { get; set; }
    }
    
    public class Record : IRecord
    {
        public string Contents { get; set; }

        public Record(string info)
        {
            Contents = info;
        }
    }

    public interface IResultCursor
    {
        Task<bool> FetchAsync();
        IRecord Current();
        void Consume();

    }

    public class ResultCursor : IResultCursor
    {
        private int _count = 0;

        public ResultCursor()
        {
            _count = 0;
        }

        public async Task<bool> FetchAsync()
        {
            await Task.CompletedTask;
            return (_count++ < 3);
        }

        public IRecord Current()
        {
            return new Record(_count.ToString());
        }

        public void Consume()
        {
            
        }
    }

}
