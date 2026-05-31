using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Results
{
   public class SuccessDataResult<T> : DataResult<T>
    {
        public SuccessDataResult(T data, string message) : base(data, true, message )
        {

        }

        public SuccessDataResult(T data):base(data,true)
        {

        }


        public SuccessDataResult(string message) : base(default, true, message)
        {
                      //veri yok mesaj ve kontrol var
        }

        public SuccessDataResult() : base(default, true) 
        {     //veri yok mesaj da yok
            

        }

    }
}
