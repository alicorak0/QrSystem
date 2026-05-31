using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Results
{                 // birden fazla Tip alabilecek entities
   public interface IDataResult<T>:IResult
    {

        T Data { get; }  // DÖNECEK TİP GENERİC OLUR TEKRAR TEKRAR KULLANILIR
    }
}
