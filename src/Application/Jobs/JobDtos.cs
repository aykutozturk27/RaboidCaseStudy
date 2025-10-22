namespace RaboidCaseStudy.Application.Jobs;
public record EnqueueJobRequest(string StoreId, string ProductCode, string ProductName, decimal Price);
public record LeaseJobRequest(string ClientId);
public record JobResultRequest(string ClientId, string JobId, bool Success, string? ErrorMessage);
