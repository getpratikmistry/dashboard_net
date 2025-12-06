using DashboardNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardNet.Controllers;

public class HomeController : Controller
{
    private readonly TripService _tripService;

    public HomeController(TripService tripService)
    {
        _tripService = tripService;
    }

    public async Task<IActionResult> Index()
    {
        var trips = await _tripService.GetTripsAsync();
        ViewData["InitialTripsJson"] = _tripService.SerializeTrips(trips);
        return View(trips);
    }

    [HttpGet("api/trips")]
    public async Task<IActionResult> Trips([FromQuery] string order = "asc", [FromQuery] string critical = "all")
    {
        var trips = await _tripService.GetTripSnapshotsAsync(order, critical);
        return Json(trips);
    }
}
