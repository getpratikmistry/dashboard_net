-- Schema for dispatcher dashboard
CREATE TABLE Trips (
    Id NVARCHAR(40) NOT NULL PRIMARY KEY,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    TruckUnit NVARCHAR(25) NOT NULL,
    DriverName NVARCHAR(120) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'OnTime',
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Trailers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TripId NVARCHAR(40) NOT NULL,
    UnitNumber NVARCHAR(25) NOT NULL,
    FOREIGN KEY (TripId) REFERENCES Trips(Id)
);

CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TripId NVARCHAR(40) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    FOREIGN KEY (TripId) REFERENCES Trips(Id)
);

CREATE TABLE Probills (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TripId NVARCHAR(40) NOT NULL,
    Number NVARCHAR(50) NOT NULL,
    PickupLocation NVARCHAR(200) NOT NULL,
    DeliveryLocation NVARCHAR(200) NOT NULL,
    StopArrival DATETIME2 NULL,
    FOREIGN KEY (TripId) REFERENCES Trips(Id)
);

CREATE TABLE TripStops (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TripId NVARCHAR(40) NOT NULL,
    Name NVARCHAR(120) NOT NULL,
    ArrivesAt DATETIME2 NOT NULL,
    IsComplete BIT NOT NULL DEFAULT 0,
    Sequence INT NOT NULL,
    FOREIGN KEY (TripId) REFERENCES Trips(Id)
);

CREATE INDEX IX_Trailers_TripId ON Trailers(TripId);
CREATE INDEX IX_Customers_TripId ON Customers(TripId);
CREATE INDEX IX_Probills_TripId ON Probills(TripId);
CREATE INDEX IX_TripStops_TripId ON TripStops(TripId);
CREATE INDEX IX_Trips_UpdatedAt ON Trips(UpdatedAt);

GO

-- Sample seed data for local testing
INSERT INTO Trips (Id, StartDate, EndDate, TruckUnit, DriverName, Status)
VALUES
('TRP-2001', DATEADD(HOUR, -4, SYSUTCDATETIME()), DATEADD(HOUR, 16, SYSUTCDATETIME()), 'TK-031', 'Driver 1', 'OnTime'),
('TRP-2002', DATEADD(HOUR, -1, SYSUTCDATETIME()), DATEADD(HOUR, 24, SYSUTCDATETIME()), 'TK-032', 'Driver 2', 'Delayed');

INSERT INTO Trailers (TripId, UnitNumber) VALUES
('TRP-2001', 'TLR-011'),
('TRP-2001', 'TLR-021'),
('TRP-2002', 'TLR-012'),
('TRP-2002', 'TLR-022');

INSERT INTO Customers (TripId, Name) VALUES
('TRP-2001', 'Contoso Retail'),
('TRP-2001', 'Fabrikam Foods'),
('TRP-2002', 'Adventure Works'),
('TRP-2002', 'Tailspin Toys');

INSERT INTO Probills (TripId, Number, PickupLocation, DeliveryLocation, StopArrival) VALUES
('TRP-2001', 'PB-101', 'Calgary, AB', 'Winnipeg, MB', DATEADD(HOUR, -2, SYSUTCDATETIME())),
('TRP-2001', 'PB-102', 'Regina, SK', 'Edmonton, AB', NULL),
('TRP-2001', 'PB-103', 'Vancouver, BC', 'Calgary, AB', NULL),
('TRP-2002', 'PB-201', 'Saskatoon, SK', 'Calgary, AB', DATEADD(HOUR, 2, SYSUTCDATETIME())),
('TRP-2002', 'PB-202', 'Edmonton, AB', 'Winnipeg, MB', NULL),
('TRP-2002', 'PB-203', 'Calgary, AB', 'Regina, SK', NULL);

INSERT INTO TripStops (TripId, Name, ArrivesAt, IsComplete, Sequence) VALUES
('TRP-2001', 'Pickup', DATEADD(HOUR, -3, SYSUTCDATETIME()), 1, 1),
('TRP-2001', 'Linehaul', DATEADD(HOUR, 6, SYSUTCDATETIME()), 0, 2),
('TRP-2001', 'Delivery', DATEADD(HOUR, 16, SYSUTCDATETIME()), 0, 3),
('TRP-2002', 'Pickup', DATEADD(HOUR, -1, SYSUTCDATETIME()), 1, 1),
('TRP-2002', 'Linehaul', DATEADD(HOUR, 10, SYSUTCDATETIME()), 0, 2),
('TRP-2002', 'Delivery', DATEADD(HOUR, 24, SYSUTCDATETIME()), 0, 3);
