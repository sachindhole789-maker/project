using App.Application.Features.V1.Users;
using App.Application.Messaging;
using Rest.API.Contracts;
using Microsoft.AspNetCore.Mvc;
using Rest.API.Middleware;
namespace Rest.API.Controllers;
[ApiController]
[Route("api/v1/students")]
public sealed class StudentsController(IDispatcher dispatcher):ControllerBase
{
 [HttpGet("{customerId:long}")]
 [ProducesResponseType(typeof(ApiResponse<GetStudentResponse>),StatusCodes.Status200OK)]
 public async Task<IActionResult> Get(long customerId,CancellationToken ct){var result=await dispatcher.ExecuteQuery(new GetStudentQuery(customerId),ct);return this.ToActionResult(result);}
}
