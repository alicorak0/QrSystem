using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Results
{
    public class ErrorDataResult<T> : DataResult<T>
    {
        public ErrorDataResult(T data, string message) : base(data, false, message)
        {

        }

        public ErrorDataResult(T data) : base(data, false)
        {

        }


        public ErrorDataResult(string message) : base(default, false, message)
        {
            //veri yok mesaj ve kontrol var
        }

        public ErrorDataResult() : base(default, false)
        {     //veri yok mesaj da yok


        }

    }
}
