using DashboardNet.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DashboardNet.Services;

public class TripService
{
    private readonly string _connectionString;
    private readonly object _sync = new();
    private DateTime _lastBroadcastCursorUtc = DateTime.MinValue;

    public TripService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Dashboard")
            ?? throw new InvalidOperationException("Missing connection string 'Dashboard'.");
    }

    public async Task<IReadOnlyCollection<Trip>> GetTripsAsync(string order = "asc", string? criticalFilter = null)
    {
        return await LoadTripsAsync(order, criticalFilter);
    }

    public async Task<IEnumerable<object>> GetTripSnapshotsAsync(string order = "asc", string? criticalFilter = null)
    {
        var trips = await GetTripsAsync(order, criticalFilter);
        return trips.Select(ToSnapshot).ToList();
    }

    /// <summary>
    /// Returns only trips that have been updated in SQL Server since the last broadcast.
    /// </summary>
    public async Task<IEnumerable<object>> GetRecentTripSnapshotsAsync()
    {
        DateTime since;
        lock (_sync)
        {
            since = _lastBroadcastCursorUtc;
        }

        var updates = await LoadTripsAsync("asc", null, since);
        if (updates.Any())
        {
            var latest = updates.Max(t => t.UpdatedAt);
            lock (_sync)
            {
                if (latest > _lastBroadcastCursorUtc)
                {
                    _lastBroadcastCursorUtc = latest;
                }
            }
        }

        return updates.Select(ToSnapshot).ToList();
    }

    public string SerializeTrips(IEnumerable<Trip> trips)
    {
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        return System.Text.Json.JsonSerializer.Serialize(trips.Select(ToSnapshot), options);
    }

    private async Task<List<Trip>> LoadTripsAsync(string order, string? criticalFilter, DateTime? updatedSinceUtc = null)
    {
        var orderClause = order?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true ? "DESC" : "ASC";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var whereParts = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(criticalFilter) && criticalFilter != "all")
        {
            whereParts.Add("t.Status IN (@Delayed, @OffRoute)");
            parameters.Add("Delayed", TripStatus.Delayed.ToString());
            parameters.Add("OffRoute", TripStatus.OffRoute.ToString());
        }

        if (updatedSinceUtc.HasValue)
        {
            whereParts.Add("t.UpdatedAt > @UpdatedSince");
            parameters.Add("UpdatedSince", updatedSinceUtc.Value);
        }

        var whereClause = whereParts.Count > 0 ? $"WHERE {string.Join(" AND ", whereParts)}" : string.Empty;

        var tripsSql = $@"SELECT t.Id, t.StartDate, t.EndDate, t.TruckUnit, t.DriverName, t.Status, t.UpdatedAt
FROM Trips t
{whereClause}
ORDER BY t.StartDate {orderClause};";

        var tripRows = (await connection.QueryAsync<TripRow>(tripsSql, parameters)).ToList();
        if (!tripRows.Any())
        {
            return new List<Trip>();
        }

        var ids = tripRows.Select(t => t.Id).ToArray();
        var idParams = new DynamicParameters(new { ids });

        var trailerLookup = (await connection.QueryAsync<TrailerRow>("SELECT TripId, UnitNumber FROM Trailers WHERE TripId IN @ids;", idParams))
            .GroupBy(t => t.TripId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var customerLookup = (await connection.QueryAsync<CustomerRow>("SELECT TripId, Name FROM Customers WHERE TripId IN @ids;", idParams))
            .GroupBy(c => c.TripId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var probillLookup = (await connection.QueryAsync<ProbillRow>("SELECT TripId, Number, PickupLocation, DeliveryLocation, StopArrival FROM Probills WHERE TripId IN @ids;", idParams))
            .GroupBy(p => p.TripId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var stopLookup = (await connection.QueryAsync<TripStopRow>("SELECT TripId, Name, ArrivesAt, IsComplete, Sequence FROM TripStops WHERE TripId IN @ids;", idParams))
            .GroupBy(s => s.TripId)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Sequence).ToList());

        var trips = tripRows.Select(row => new Trip
        {
            Id = row.Id,
            StartDate = row.StartDate,
            EndDate = row.EndDate,
            TruckUnit = row.TruckUnit,
            DriverName = row.DriverName,
            Status = ParseStatus(row.Status),
            UpdatedAt = row.UpdatedAt,
            Trailers = trailerLookup.TryGetValue(row.Id, out var trailers)
                ? trailers.Select(t => new Trailer { UnitNumber = t.UnitNumber }).ToList()
                : new List<Trailer>(),
            Customers = customerLookup.TryGetValue(row.Id, out var customers)
                ? customers.Select(c => new Customer { Name = c.Name }).ToList()
                : new List<Customer>(),
            Probills = probillLookup.TryGetValue(row.Id, out var probills)
                ? probills.Select(p => new Probill
                {
                    Number = p.Number,
                    PickupLocation = p.PickupLocation,
                    DeliveryLocation = p.DeliveryLocation,
                    StopArrival = p.StopArrival
                }).ToList()
                : new List<Probill>(),
            Timeline = stopLookup.TryGetValue(row.Id, out var stops)
                ? stops.Select(s => new TripStop
                {
                    Name = s.Name,
                    ArrivesAt = s.ArrivesAt,
                    IsComplete = s.IsComplete
                }).ToList()
                : new List<TripStop>()
        }).ToList();

        return trips;
    }

    private static TripStatus ParseStatus(string raw)
    {
        return Enum.TryParse(raw, ignoreCase: true, out TripStatus status) ? status : TripStatus.OnTime;
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

    private record TripRow(string Id, DateTime StartDate, DateTime EndDate, string TruckUnit, string DriverName, string Status, DateTime UpdatedAt);

    private record TrailerRow(string TripId, string UnitNumber);

    private record CustomerRow(string TripId, string Name);

    private record ProbillRow(string TripId, string Number, string PickupLocation, string DeliveryLocation, DateTime? StopArrival);

    private record TripStopRow(string TripId, string Name, DateTime ArrivesAt, bool IsComplete, int Sequence);
}
