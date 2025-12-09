using FCG.Payments.Application.Abstractions.Messaging;
using FCG.Payments.Application.Abstractions.Pagination;

namespace FCG.Payments.Application.UseCases.Payments.GetPaymentHistory
{
    public interface IGetPaymentHistoryUseCase : IQueryHandler<GetPaymentHistoryRequest, PagedListResponse<GetPaymentHistoryResponse>> { }
}
