using FCG.Payments.Application.Abstractions.Audit;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FCG.Payments.Infrastructure.SqlServer.Providers
{
    public class CurrentSessionProvider : ICurrentSessionProvider
    {
        private readonly Guid? _currentUserId;

        public CurrentSessionProvider(IHttpContextAccessor accessor)
        {
            var userIdClaim = accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim?.Value;

            if (Guid.TryParse(userId, out var id))
                _currentUserId = id;
        }

        public Guid? GetUserId() => _currentUserId;
    }
}
