using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace FCG.Payments.Domain.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ConflictException : BaseException
    {
        public ConflictException(string message) : base(HttpStatusCode.Conflict, message) { }
        public ConflictException(string message, Exception innerException) : base(HttpStatusCode.Conflict, message, innerException) { }
    }
}
