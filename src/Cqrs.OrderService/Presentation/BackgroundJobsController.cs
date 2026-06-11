using Cqrs.OrderService.Infrastructure.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cqrs.OrderService.Presentation;

[ApiController]
[Authorize(Roles = "ROLE_ADMIN")]
[Route("api/v1/jobs")]
public sealed class BackgroundJobsController(IBackgroundJobClient backgroundJobClient) : ControllerBase
{
    [HttpPost("outbox-dispatch")]
    public ActionResult<EnqueueJobResponse> EnqueueOutboxDispatch()
    {
        var jobId = backgroundJobClient.Enqueue<OutboxDispatchHangfireJob>(job => job.RunAsync(CancellationToken.None));
        return Accepted(new EnqueueJobResponse(jobId));
    }
}

public sealed record EnqueueJobResponse(string JobId);
