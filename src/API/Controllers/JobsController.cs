using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RaboidCaseStudy.Application.Abstractions.Repositories;
using RaboidCaseStudy.Application.Jobs;
using RaboidCaseStudy.Domain.Jobs;
using RaboidCaseStudy.Infrastructure.Persistence;

namespace RaboidCaseStudy.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IRepository<Job> _jobs;
    private readonly MongoContext _ctx;
    public JobsController(IRepository<Job> jobs, MongoContext ctx) { _jobs = jobs; _ctx = ctx; }

    [HttpPost("enqueue")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Job>> Enqueue(EnqueueJobRequest req, CancellationToken ct)
    {
        var job = await _jobs.InsertAsync(new Job {
            StoreId = req.StoreId,
            ProductCode = req.ProductCode,
            ProductName = req.ProductName,
            Price = req.Price
        }, ct);
        return job;
    }

    [HttpPost("lease")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<ActionResult<Job?>> Lease(LeaseJobRequest req, CancellationToken ct)
    {
        var col = _ctx.GetCollection<Job>();
        // Atomically lease the oldest queued job
        var update = Builders<Job>.Update
            .Set(j => j.Status, JobStatus.Leased)
            .Set(j => j.LeasedByClientId, req.ClientId)
            .Set(j => j.LeasedAtUtc, DateTime.UtcNow)
            .Inc(j => j.Attempt, 1);
        var job = await col.FindOneAndUpdateAsync(
            filter: Builders<Job>.Filter.Eq(j => j.Status, JobStatus.Queued),
            update: update,
            options: new FindOneAndUpdateOptions<Job> { ReturnDocument = ReturnDocument.After, Sort = Builders<Job>.Sort.Ascending(j => j.CreatedAtUtc) },
            cancellationToken: ct
        );
        return job is null ? NoContent() : Ok(job);
    }

    [HttpPost("complete/{id}")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<IActionResult> Complete(string id, CancellationToken ct)
    {
        var col = _ctx.GetCollection<Job>();
        var res = await col.UpdateOneAsync(j => j.Id == id, Builders<Job>.Update.Set(j => j.Status, JobStatus.Completed), cancellationToken: ct);
        return res.ModifiedCount == 0 ? NotFound() : Ok();
    }

    [HttpPost("fail/{id}")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<IActionResult> Fail(string id, CancellationToken ct)
    {
        var col = _ctx.GetCollection<Job>();
        var res = await col.UpdateOneAsync(j => j.Id == id, Builders<Job>.Update.Set(j => j.Status, JobStatus.Failed), cancellationToken: ct);
        return res.ModifiedCount == 0 ? NotFound() : Ok();
    }
}
