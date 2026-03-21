namespace FCG.Payments.Application.Abstractions.Audit
{
    public interface ICurrentSessionProvider
    {
        Guid? GetUserId();
    }
}
