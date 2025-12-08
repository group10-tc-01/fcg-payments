using FCG.Payments.Application.UseCases.Wallets.DepositBalance;
using FCG.Payments.Application.UseCases.Wallets.GetWalletBalance;
using FCG.Payments.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Payments.WebApi.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletsController : FcgPaymentBaseController
    {
        public WalletsController(IMediator mediator) : base(mediator) { }

        [HttpGet("{id}/balance")]
        [ProducesResponseType(typeof(ApiResponse<GetWalletBalanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> GetWalletBalanceAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var request = new GetWalletBalanceRequest(id);

            var response = await _mediator.Send(request, cancellationToken);

            return Ok(ApiResponse<GetWalletBalanceResponse>.SuccesResponse(response));
        }

        [HttpPost("{id}/deposit")]
        [ProducesResponseType(typeof(ApiResponse<DepositBalanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DepositBalanceAsync([FromRoute] Guid id, [FromBody] DepositBalanceRequestBody requestBody, CancellationToken cancellationToken)
        {
            var request = new DepositBalanceRequest(id, requestBody.Amount);

            var response = await _mediator.Send(request, cancellationToken);

            return Ok(ApiResponse<DepositBalanceResponse>.SuccesResponse(response));
        }

    }
}
