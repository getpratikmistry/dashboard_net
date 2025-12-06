-- Pull trips with nested data for the dashboard (ordered by start date)
SELECT t.Id, t.StartDate, t.EndDate, t.TruckUnit, t.DriverName, t.Status, t.UpdatedAt,
       tr.UnitNumber AS TrailerUnit,
       c.Name AS CustomerName,
       p.Number AS ProbillNumber, p.PickupLocation, p.DeliveryLocation, p.StopArrival,
       s.Name AS StopName, s.ArrivesAt, s.IsComplete, s.Sequence
FROM Trips t
LEFT JOIN Trailers tr ON tr.TripId = t.Id
LEFT JOIN Customers c ON c.TripId = t.Id
LEFT JOIN Probills p ON p.TripId = t.Id
LEFT JOIN TripStops s ON s.TripId = t.Id
WHERE (@critical = 'all' OR t.Status IN ('Delayed', 'OffRoute'))
ORDER BY t.StartDate ASC, s.Sequence ASC;

-- Mark a trip off-route
UPDATE Trips SET Status = 'OffRoute', UpdatedAt = SYSUTCDATETIME() WHERE Id = 'TRP-2002';

-- Mark the next stop complete
UPDATE TripStops SET IsComplete = 1
WHERE TripId = 'TRP-2002' AND Sequence = 1;
UPDATE Trips SET UpdatedAt = SYSUTCDATETIME() WHERE Id = 'TRP-2002';

-- Record a probill arrival time
UPDATE Probills
SET StopArrival = SYSUTCDATETIME()
WHERE TripId = 'TRP-2001' AND Number = 'PB-102';
UPDATE Trips SET UpdatedAt = SYSUTCDATETIME() WHERE Id = 'TRP-2001';

-- Reset a trip back to on-time
UPDATE Trips SET Status = 'OnTime', UpdatedAt = SYSUTCDATETIME() WHERE Id = 'TRP-2001';
