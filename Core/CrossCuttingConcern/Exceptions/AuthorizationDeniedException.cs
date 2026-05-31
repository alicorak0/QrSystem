using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcern.Exceptions
{
   public class AuthorizationDeniedException:Exception
    {
        public AuthorizationDeniedException(string message):base(message) 
        {
        
        }
    }
}
