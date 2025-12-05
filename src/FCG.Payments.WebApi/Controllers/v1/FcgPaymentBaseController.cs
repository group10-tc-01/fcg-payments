using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.WebApi.Controllers.v1
{
    [ExcludeFromCodeCoverage]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class FcgPaymentBaseController(IMediator mediator) : ControllerBase
    {
        protected IMediator _mediator = mediator;
    }
}
