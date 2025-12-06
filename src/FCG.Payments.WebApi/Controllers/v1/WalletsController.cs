using FCG.Payments.Application.UseCases.Wallets.Deposit;
using FCG.Payments.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.WebApi.Controllers.v1
{
    [ExcludeFromCodeCoverage]
    [Authorize(Roles = "Admin")]
    public class WalletsController : FcgPaymentBaseController
    {
        public WalletsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost("{id}/deposit")]
        public async Task<IActionResult> Deposit(Guid id, [FromBody] DepositRequest request)
        {
            var command = new DepositCommand(id, request.Amount);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<DepositResponse>.SuccesResponse(result));
        }
    }
}
