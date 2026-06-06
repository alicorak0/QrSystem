using FluentValidation.Results;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public class ErrorDetails
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class ValidationErrorDetails:ErrorDetails  // zaten Error details içerisde mevcut olmuş olacak
    {
        public IEnumerable<ValidationFailure> Errors { get; set; }

        public override string ToString()
        {
            // Sadece hata mesajlarını ekle
            var errorMessages = Errors?.Select(e => e.ErrorMessage).ToList() ?? new List<string>();
            
            var result = new
            {
                StatusCode = StatusCode,
                Message = Message,
                Errors = errorMessages
            };

            return JsonConvert.SerializeObject(result);
        }
    }

}
