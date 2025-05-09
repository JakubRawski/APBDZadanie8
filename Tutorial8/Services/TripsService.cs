using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService(IConfiguration config) : ITripsService
{
//"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Database;Integrated Security=True;";

    private readonly string? _connectionString = config.GetConnectionString("Default");
    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT t.IdTrip, t.Name, c.Name AS Country
            FROM Trip t
            JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
            JOIN Country c ON ct.IdCountry = c.IdCountry", conn);

        using var reader = await cmd.ExecuteReaderAsync();
        var tripDict = new Dictionary<int, TripDTO>();

        while (await reader.ReadAsync())
        {
            int idTrip = reader.GetInt32(0);
            string tripName = reader.GetString(1);
            string country = reader.GetString(2);

            if (!tripDict.TryGetValue(idTrip, out var trip))
            {
                trip = new TripDTO { Id = idTrip, Name = tripName, Countries = new List<CountryDTO>() };
                tripDict[idTrip] = trip;
            }
            trip.Countries.Add(new CountryDTO { Name = country });
        }

        return tripDict.Values.ToList();
    }

    public async Task<List<TripDTO>> GetTripsForClient(int clientId)
    {
        var trips = new List<TripDTO>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT t.IdTrip, t.Name, c.Name AS Country
            FROM Client_Trip ct
            JOIN Trip t ON ct.IdTrip = t.IdTrip
            JOIN Country_Trip ctr ON t.IdTrip = ctr.IdTrip
            JOIN Country c ON ctr.IdCountry = c.IdCountry
            WHERE ct.IdClient = @id", conn);
        cmd.Parameters.AddWithValue("@id", clientId);

        using var reader = await cmd.ExecuteReaderAsync();
        var tripDict = new Dictionary<int, TripDTO>();

        while (await reader.ReadAsync())
        {
            int idTrip = reader.GetInt32(0);
            string name = reader.GetString(1);
            string country = reader.GetString(2);

            if (!tripDict.TryGetValue(idTrip, out var trip))
            {
                trip = new TripDTO { Id = idTrip, Name = name, Countries = new List<CountryDTO>() };
                tripDict[idTrip] = trip;
            }
            trip.Countries.Add(new CountryDTO { Name = country });
        }

        return tripDict.Values.ToList();
    }

    
}