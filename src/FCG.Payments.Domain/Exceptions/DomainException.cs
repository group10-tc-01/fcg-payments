using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace FCG.Payments.Domain.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class DomainException : BaseException
    {
        public DomainException(string message) : base(HttpStatusCode.BadRequest, message) { }
        public DomainException(string message, Exception innerException) : base(HttpStatusCode.BadRequest, message, innerException) { }
    }
}
