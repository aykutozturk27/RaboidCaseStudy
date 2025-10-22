await Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHttpClient("api", c =>
        {
            c.BaseAddress = new Uri(ctx.Configuration["ApiBaseUrl"] ?? "http://localhost:5080/");
        });
        services.AddHostedService<SchedulerWorker>();
    })
    .Build()
    .RunAsync();
