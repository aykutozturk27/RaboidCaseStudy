using System.Net.Http.Json;

public class SchedulerWorker : BackgroundService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<SchedulerWorker> _logger;
    private readonly IConfiguration _configuration;

    public SchedulerWorker(IHttpClientFactory httpFactory, 
                           ILogger<SchedulerWorker> logger, 
                           IConfiguration cfg)
    {
        _httpFactory = httpFactory; 
        _logger = logger; 
        _configuration = cfg;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = _httpFactory.CreateClient("api");
        var leaseInterval = _configuration.GetValue("LeaseIntervalSeconds", 5);
        var clientId = _configuration["ClientId"] ?? "scheduler";
        _logger.LogInformation("Scheduler started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // For demo simplicity, call lease without auth (or attach a token if needed)
                var leaseResp = await client.PostAsJsonAsync("api/jobs/lease", new { ClientId = clientId }, stoppingToken);
                if (leaseResp.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _logger.LogInformation("No jobs available");
                }
                else if (leaseResp.IsSuccessStatusCode)
                {
                    var job = await leaseResp.Content.ReadFromJsonAsync<JobDto>(cancellationToken: stoppingToken);
                    if (job is not null)
                    {
                        _logger.LogInformation("Leased job {Id} for store {StoreId}", job.Id, job.StoreId);
                        // Simulate outcome
                        var ok = Random.Shared.NextDouble() < _configuration.GetValue("CompleteProbability", 0.9);
                        var endpoint = ok ? $"api/jobs/complete/{job.Id}" : $"api/jobs/fail/{job.Id}";
                        await client.PostAsync(endpoint, null, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in scheduler loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(leaseInterval), stoppingToken);
        }
    }

    private record JobDto(string Id, string StoreId, string ProductCode, string ProductName, decimal Price);
}
