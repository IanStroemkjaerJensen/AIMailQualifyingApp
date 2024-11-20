using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Care.Web.Common.ValueObjects
{
    public class Error
    {
        public string Code { get; }
        public string Message { get; }
        public int StatusCode { get; }

        public Error(string code, string message, int statusCode = 400)
        {
            Code = code;
            Message = message;
            StatusCode = statusCode;
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(Code);
        }
    }
}
