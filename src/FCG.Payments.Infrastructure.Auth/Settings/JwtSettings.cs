namespace FCG.Payments.Infrastructure.Auth.Settings
{
    public sealed class JwtSettings
    {
        public string SecretKey { get; init; } = string.Empty;
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
    }
}
