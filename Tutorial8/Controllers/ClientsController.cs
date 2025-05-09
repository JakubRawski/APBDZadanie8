using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly ITripsService _service;
    public ClientsController(ITripsService service) => _service = service;

    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetTripsForClient(int id)
    {
        var trips = await _service.GetTripsForClient(id);
        if (trips.Count == 0)
        {
            return NotFound("Brak wycieczki lub klient nie istnieje.");
        }
        return Ok(trips);
    }

    [HttpPost]
    public async Task<IActionResult> AddClient([FromBody] ClientDTO client)
    {
        var id = await _service.AddClient(client);
        if (id == null)
        {
            return BadRequest("Nieprawidłowe dane wejściowe.");
        }
        return CreatedAtAction(nameof(GetTripsForClient), new { id = id.Value }, new { id = id.Value });
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientToTrip(int id, int tripId)
    {
        var result = await _service.RegisterClientToTrip(id, tripId);
        return result switch
        {
            "ClientNotFound" => NotFound("Klient nie istnieje."),
            "TripNotFound" => NotFound("Wycieczka nie istnieje."),
            "Full" => BadRequest("Osiągnięto limit uczestników."),
            "Success" => Ok("Zarejestrowano klienta."),
            _ => StatusCode(500, "Błąd serwera.")
        };
    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> RemoveClientTrip(int id, int tripId)
    {
        var result = await _service.RemoveClientTrip(id, tripId);
        return result switch
        {
            "NotRegistered" => NotFound("Klient nie jest zarejestrowany."),
            "Success" => Ok("Rejestracja usunięta."),
            _ => StatusCode(500, "Błąd serwera.")
        };
    }
}