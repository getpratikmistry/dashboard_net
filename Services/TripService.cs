using DashboardNet.Models;
using System.Text.Json;

namespace DashboardNet.Services;

public class TripService
{
    private readonly List<Trip> _trips;
    private readonly Random _random = new();

    public TripService()
    {
        _trips = BuildSeedTrips();
    }

    public IReadOnlyCollection<Trip> GetTrips(string order = "asc", string? criticalFilter = null)
    {
        ApplyLiveMutations();

        IEnumerable<Trip> query = _trips;

        if (!string.IsNullOrWhiteSpace(criticalFilter) && criticalFilter != "all")
        {
            query = query.Where(t => t.Status == TripStatus.Delayed || t.Status == TripStatus.OffRoute);
        }

        query = order?.ToLowerInvariant() == "desc"
            ? query.OrderByDescending(t => t.StartDate)
            : query.OrderBy(t => t.StartDate);

        return query.ToList();
    }

    public IEnumerable<object> GetTripSnapshots(string order = "asc", string? criticalFilter = null)
    {
        var trips = GetTrips(order, criticalFilter);
        return trips.Select(ToSnapshot).ToList();
    }

    public string SerializeTrips(IEnumerable<Trip> trips)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        return JsonSerializer.Serialize(trips.Select(ToSnapshot), options);
    }

    private static object ToSnapshot(Trip trip) => new
    {
        id = trip.Id,
        startDate = trip.StartDate.ToString("yyyy-MM-dd HH:mm"),
        endDate = trip.EndDate.ToString("yyyy-MM-dd HH:mm"),
        truckUnit = trip.TruckUnit,
        driverName = trip.DriverName,
        trailers = trip.Trailers.Select(t => new { unitNumber = t.UnitNumber }),
        customers = trip.Customers.Select(c => new { name = c.Name }),
        probills = trip.Probills.Select(p => new
        {
            number = p.Number,
            pickupLocation = p.PickupLocation,
            deliveryLocation = p.DeliveryLocation,
            stopArrival = p.StopArrival?.ToString("MMM dd, HH:mm") ?? "Pending"
        }),
        timeline = trip.Timeline.Select(stop => new
        {
            name = stop.Name,
            arrivesAt = stop.ArrivesAt.ToString("MMM dd, HH:mm"),
            isComplete = stop.IsComplete
        }),
        status = trip.Status.ToString()
    };

    private void ApplyLiveMutations()
    {
        foreach (var trip in _trips)
        {
            if (_random.NextDouble() < 0.15)
            {
                var statusRoll = _random.Next(0, 100);
                trip.Status = statusRoll switch
                {
                    < 70 => TripStatus.OnTime,
                    < 90 => TripStatus.Delayed,
                    _ => TripStatus.OffRoute
                };
            }

            if (_random.NextDouble() < 0.25)
            {
                var nextStop = trip.Timeline.FirstOrDefault(stop => !stop.IsComplete);
                if (nextStop is not null)
                {
                    nextStop.IsComplete = true;
                }
            }

            foreach (var probill in trip.Probills)
            {
                if (_random.NextDouble() < 0.2)
                {
                    probill.StopArrival = DateTime.UtcNow.AddMinutes(_random.Next(-120, 180));
                }
            }
        }
    }

    private List<Trip> BuildSeedTrips()
    {
        var now = DateTime.UtcNow.Date;
        var trips = new List<Trip>();

        for (var i = 0; i < 12; i++)
        {
            var start = now.AddHours(i * 3);
            var end = start.AddHours(_random.Next(20, 48));
            var tripId = $"TRP-{2000 + i}";

            trips.Add(new Trip
            {
                Id = tripId,
                StartDate = start,
                EndDate = end,
                TruckUnit = $"TK-{30 + i:000}",
                DriverName = $"Driver {i + 1}",
                Trailers = new List<Trailer>
                {
                    new() { UnitNumber = $"TLR-{i + 10:000}" },
                    new() { UnitNumber = $"TLR-{i + 20:000}" }
                },
                Customers = new List<Customer>
                {
                    new() { Name = $"Contoso Retail {i + 1}" },
                    new() { Name = $"Fabrikam Foods {i + 1}" }
                },
                Probills = new List<Probill>
                {
                    new() { Number = $"PB-{i + 101}", PickupLocation = "Calgary, AB", DeliveryLocation = "Winnipeg, MB", StopArrival = start.AddHours(4) },
                    new() { Number = $"PB-{i + 202}", PickupLocation = "Regina, SK", DeliveryLocation = "Edmonton, AB", StopArrival = start.AddHours(10) },
                    new() { Number = $"PB-{i + 303}", PickupLocation = "Vancouver, BC", DeliveryLocation = "Calgary, AB", StopArrival = start.AddHours(18) }
                },
                Timeline = new List<TripStop>
                {
                    new() { Name = "Pickup", ArrivesAt = start.AddHours(1), IsComplete = true },
                    new() { Name = "Linehaul", ArrivesAt = start.AddHours(12), IsComplete = i % 3 == 0 },
                    new() { Name = "Break", ArrivesAt = start.AddHours(18), IsComplete = false },
                    new() { Name = "Delivery", ArrivesAt = end, IsComplete = false }
                },
                Status = (TripStatus)_random.Next(0, 3)
            });
        }

        return trips;
    }
}
