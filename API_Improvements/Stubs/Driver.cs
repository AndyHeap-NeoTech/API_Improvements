using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Improvements
{
    interface IDriver
    {
      
    }

    class Driver : IDriver
    {
        public Driver()
        {

        }

        public ISession Session()
        {
            return new Session();
        }
    }
}
