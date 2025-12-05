using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace FCG.Payments.Domain.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class BaseException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        protected BaseException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        protected BaseException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
