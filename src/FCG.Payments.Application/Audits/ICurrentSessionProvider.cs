namespace FCG.Payments.Application.Audits
{
    public interface ICurrentSessionProvider
    {
        Guid? GetUserId();
    }
}
