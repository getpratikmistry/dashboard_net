# Dispatcher Dashboard (ASP.NET Core MVC)

Live trip dashboard built with ASP.NET Core MVC, Bootstrap 5, and jQuery. The dispatcher view shows vertically stacked cards for each trip and supports micro-updates without reloading the page.

## Getting started
1. Install the .NET 8 SDK.
2. Restore packages and run the development server:
   ```bash
   dotnet restore
   dotnet run
   ```
3. Browse to `https://localhost:5001` (or the URL displayed in the console).

## Features
- Card view with truck, driver, trailers, customers, probills, timeline, and status badges.
- Filter header for ordering trips and surfacing critical (delayed/off-route) trips.
- Micro-update loop (every 5s) updates card sections individually without a page refresh and pauses when hovering a card.
- Lightweight UI styling with Bootstrap 5 and custom CSS tuned for large datasets.
