using System.Net.Http.Json;

Console.WriteLine("Mock RPA Client starting...");
var baseUrl = Environment.GetEnvironmentVariable("API_BASE") ?? "http://localhost:5080/";
var email = Environment.GetEnvironmentVariable("EMAIL") ?? "client1@raboid.local";
var password = Environment.GetEnvironmentVariable("PASSWORD") ?? "Client123!";
var clientId = Environment.GetEnvironmentVariable("CLIENT_ID") ?? "rpa-1";

var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

// Register or login
var reg = await http.PostAsJsonAsync("api/auth/register", new { Email = email, Password = password, Roles = new []{"Client"} });
if (!reg.IsSuccessStatusCode)
{
    var login = await http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
    login.EnsureSuccessStatusCode();
    Console.WriteLine("Logged in");
}
else
{
    Console.WriteLine("Registered");
}

// Poll for jobs
while (true)
{
    var leaseResp = await http.PostAsJsonAsync("api/jobs/lease", new { ClientId = clientId });
    if (leaseResp.StatusCode == System.Net.HttpStatusCode.NoContent)
    {
        Console.WriteLine("No job. Sleeping...");
        await Task.Delay(3000);
        continue;
    }
    var job = await leaseResp.Content.ReadFromJsonAsync<JobDto>();
    if (job is null) { await Task.Delay(1000); continue; }

    Console.WriteLine($"Processing job {job.Id} -> {job.ProductName}");
    // Simulate UI login + form fill with short delay
    await Task.Delay(1500);

    // Optionally request a barcode
    var barcode = await http.PostAsync("api/barcodes/next", null);
    if (barcode.IsSuccessStatusCode)
    {
        var code = await barcode.Content.ReadAsStringAsync();
        Console.WriteLine($"Assigned barcode: {code}");
    }

    // Random success/fail
    if (Random.Shared.NextDouble() < 0.85)
        await http.PostAsync($"api/jobs/complete/{job.Id}", null);
    else
        await http.PostAsync($"api/jobs/fail/{job.Id}", null);
}

record JobDto(string Id, string StoreId, string ProductCode, string ProductName, decimal Price);
