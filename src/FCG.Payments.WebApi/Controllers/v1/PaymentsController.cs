using FCG.Payments.Application.Abstractions.Pagination;
using FCG.Payments.Application.UseCases.Payments;
using FCG.Payments.Application.UseCases.Payments.ExportPaymentReport;
using FCG.Payments.Application.UseCases.Payments.GetPaymentHistory;
using FCG.Payments.Application.UseCases.Payments.GetPaymentReport;
using FCG.Payments.Domain.Payments;
using FCG.Payments.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Payments.WebApi.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class PaymentsController : FcgPaymentBaseController
    {
        public PaymentsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet("reports")]
        [ProducesResponseType(typeof(ApiResponse<GetPaymentReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaymentReportsAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] PaymentReportStatusFilter? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] Guid? gameId = null,
            CancellationToken cancellationToken = default)
        {
            var request = new GetPaymentReportRequest(pageNumber, pageSize, status, dateFrom, dateTo, userId, gameId);
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(ApiResponse<GetPaymentReportResponse>.SuccesResponse(response));
        }

        [HttpGet("reports/export/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportPaymentReportsPdfAsync(
            [FromQuery] PaymentReportStatusFilter? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] Guid? gameId = null,
            CancellationToken cancellationToken = default)
        {
            var request = new ExportPaymentReportRequest(status, dateFrom, dateTo, userId, gameId);
            var response = await _mediator.Send(request, cancellationToken);
            return File(response.Content, response.ContentType, response.FileName);
        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(ApiResponse<PagedListResponse<GetPaymentHistoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaymentHistoryAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] PaymentStatus? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            CancellationToken cancellationToken = default)
        {
            var request = new GetPaymentHistoryRequest(pageNumber, pageSize, status, dateFrom, dateTo);

            var response = await _mediator.Send(request, cancellationToken);

            return Ok(ApiResponse<PagedListResponse<GetPaymentHistoryResponse>>.SuccesResponse(response));
        }
    }
}
