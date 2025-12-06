# Dispatcher Dashboard (ASP.NET Core MVC)

Live trip dashboard built with ASP.NET Core MVC, Bootstrap 5, jQuery, and SignalR. The dispatcher view shows vertically stacked cards for each trip and supports micro-updates without reloading the page.

## Getting started
1. Install the .NET 8 SDK.
2. (Optional) Configure your SQL Server connection in `appsettings.json` under `ConnectionStrings:Dashboard` if you want to point the service at a live data source. The sample runs fully in-memory without a database.
3. Restore packages and run the development server:
   ```bash
   dotnet restore
   dotnet run
   ```
4. Browse to `https://localhost:5001` (or the URL displayed in the console).

## Features
- Card view with truck, driver, trailers, customers, probills, timeline, and status badges.
- Filter header for ordering trips and surfacing critical (delayed/off-route) trips.
- SignalR push updates broadcast only the mutated card sections, paired with a light polling fallback for resiliency.
- Timeline rendered across a horizontal rail to visualize stop progress at a glance.
- Lightweight UI styling with Bootstrap 5 and custom CSS tuned for large datasets (tested with 1k+ cards in mind).
