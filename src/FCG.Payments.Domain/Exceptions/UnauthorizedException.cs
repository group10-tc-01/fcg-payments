using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace FCG.Payments.Domain.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message) : base(HttpStatusCode.Unauthorized, message) { }
        public UnauthorizedException(string message, Exception innerException) : base(HttpStatusCode.Unauthorized, message, innerException) { }
    }
}
