using DashboardNet.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DashboardNet.Services;

public class TripUpdateBroadcaster : BackgroundService
{
    private readonly TripService _tripService;
    private readonly IHubContext<TripHub> _hubContext;
    private readonly ILogger<TripUpdateBroadcaster> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    public TripUpdateBroadcaster(TripService tripService, IHubContext<TripHub> hubContext, ILogger<TripUpdateBroadcaster> logger)
    {
        _tripService = tripService;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await _tripService.GetRecentTripSnapshotsAsync();
                if (updates.Any())
                {
                    await _hubContext.Clients.All.SendAsync("TripsUpdated", updates, cancellationToken: stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast trip updates.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }
    }
}
