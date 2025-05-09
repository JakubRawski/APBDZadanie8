using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<List<TripDTO>> GetTripsForClient(int clientId);
    Task<int?> AddClient(ClientDTO client);
    Task<string> RegisterClientToTrip(int clientId, int tripId);
    Task<string> RemoveClientTrip(int clientId, int tripId);
}