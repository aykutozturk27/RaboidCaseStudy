using RaboidCaseStudy.Domain.Common;
namespace RaboidCaseStudy.Domain.Jobs;
public class Job : Entity
{
    public string StoreId { get; set; } = default!;
    public string ProductCode { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public decimal Price { get; set; }
    public string? Barcode { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public string? LeasedByClientId { get; set; }
    public DateTime? LeasedAtUtc { get; set; }
    public int Attempt { get; set; } = 0;
}
