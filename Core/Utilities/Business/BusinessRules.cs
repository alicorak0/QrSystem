using Core.Utilities.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Business
{
   public class BusinessRules
    {

        //İş kuralları Motoru

        public static IResult Run(params IResult[] logics) //params ile istediğim kadar IResult geçireblirm
        {
            foreach (var logic in logics)
            { 
               if(!logic.Success)
                {
                    return logic;
                }         

            }

            return null; // başarılı ise null döner

        }

    }
}
