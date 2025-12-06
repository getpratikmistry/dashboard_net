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

    public IActionResult Index()
    {
        var trips = _tripService.GetTrips();
        ViewData["InitialTripsJson"] = _tripService.SerializeTrips(trips);
        return View(trips);
    }

    [HttpGet("api/trips")]
    public IActionResult Trips([FromQuery] string order = "asc", [FromQuery] string critical = "all")
    {
        var trips = _tripService.GetTripSnapshots(order, critical);
        return Json(trips);
    }
}
