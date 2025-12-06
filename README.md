# Dispatcher Dashboard (ASP.NET Core MVC)

Live trip dashboard built with ASP.NET Core MVC, Bootstrap 5, jQuery, and SignalR. The dispatcher view shows vertically stacked cards for each trip and supports micro-updates without reloading the page.

## Getting started
1. Install the .NET 8 SDK.
2. Configure your SQL Server connection in `appsettings.json` under `ConnectionStrings:Dashboard`.
3. Create the schema and seed data using the scripts in `database/schema.sql` (works against a blank `Dispatcher` database):
   ```sql
   :r database/schema.sql
   ```
4. Restore packages and run the development server:
   ```bash
   dotnet restore
   dotnet run
   ```
5. Browse to `https://localhost:5001` (or the URL displayed in the console).

## Features
- Card view with truck, driver, trailers, customers, probills, timeline, and status badges.
- Filter header for ordering trips and surfacing critical (delayed/off-route) trips.
- SignalR push updates broadcast only the mutated card sections pulled from SQL Server, paired with a light polling fallback for resiliency.
- Timeline rendered across a horizontal rail to visualize stop progress at a glance.
- Lightweight UI styling with Bootstrap 5 and custom CSS tuned for large datasets (tested with 1k+ cards in mind).

## SQL Server scripts
- `database/schema.sql` builds the tables (Trips, Trailers, Customers, Probills, TripStops) and seeds a couple of trips for local testing.
- `database/dashboard_queries.sql` contains the exact SELECT used by the dashboard along with UPDATE statements you can run to mark trips off-route, advance stops, or log probill arrivals. Each UPDATE touches the trip's `UpdatedAt` column so SignalR picks up the change.
