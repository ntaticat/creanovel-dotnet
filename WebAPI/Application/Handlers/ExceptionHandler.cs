using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Application.Handlers
{
    public class ExceptionHandler : Exception
    {
        public HttpStatusCode Code { get; }
        public object Errors { get; }

        public ExceptionHandler(HttpStatusCode statusCode, object errors = null)
        {
            Code = statusCode;
            Errors = errors;
        }
    }
}