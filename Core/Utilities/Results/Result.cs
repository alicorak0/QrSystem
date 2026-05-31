using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Results
{
    public class Result : IResult
    {
        public bool Success{get; }
        public string Message{get; }


        public Result(bool success,string message):this(success)
        { // Farklı constructoru da çalıştıralım

            Message = message;
        }
            
        public Result(bool success)
        {
            //Success i buraya alacam
            Success=success;

        }

        public Result()
        {

        }


    }
}
