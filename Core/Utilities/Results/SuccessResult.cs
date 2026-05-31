using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Results
{
    public class SuccessResult : Result //C# derleyici burada otomatik olarak base class (Result)’ın
    {                                  //parametresiz constructor’ını çağırmaya çalışır:

        //sadece message alacaz success zaten true olmalı başarılı durumda  gösterildiği için

        public SuccessResult(string message):base(true, message) 
        { 

        }

        //mesaj vermek istenmez ise sadece true yolla
        public SuccessResult() : base(true)
        {

        }


    }
}
