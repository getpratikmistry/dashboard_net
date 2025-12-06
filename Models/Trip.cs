namespace DashboardNet.Models;

public enum TripStatus
{
    OnTime,
    Delayed,
    OffRoute
}

public class TripStop
{
    public required string Name { get; set; }
    public required DateTime ArrivesAt { get; set; }
    public bool IsComplete { get; set; }
}

public class Probill
{
    public required string Number { get; set; }
    public required string PickupLocation { get; set; }
    public required string DeliveryLocation { get; set; }
    public DateTime? StopArrival { get; set; }
}

public class Trailer
{
    public required string UnitNumber { get; set; }
}

public class Customer
{
    public required string Name { get; set; }
}

public class Trip
{
    public required string Id { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required string TruckUnit { get; set; }
    public required string DriverName { get; set; }
    public required List<Trailer> Trailers { get; set; }
    public required List<Customer> Customers { get; set; }
    public required List<Probill> Probills { get; set; }
    public required List<TripStop> Timeline { get; set; }
    public TripStatus Status { get; set; }
    public DateTime UpdatedAt { get; set; }
}
