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

    public async Task<int?> AddClient(ClientDTO client)
    {
        if (string.IsNullOrWhiteSpace(client.FirstName) ||
            string.IsNullOrWhiteSpace(client.LastName) ||
            string.IsNullOrWhiteSpace(client.Email) ||
            string.IsNullOrWhiteSpace(client.Telephone) ||
            string.IsNullOrWhiteSpace(client.Pesel))
            return null;

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
            OUTPUT INSERTED.IdClient
            VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)", conn);
        cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
        cmd.Parameters.AddWithValue("@LastName", client.LastName);
        cmd.Parameters.AddWithValue("@Email", client.Email);
        cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
        cmd.Parameters.AddWithValue("@Pesel", client.Pesel);

        return (int?)await cmd.ExecuteScalarAsync();
    }

    public async Task<string> RegisterClientToTrip(int clientId, int tripId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var checkClient = new SqlCommand("SELECT COUNT(*) FROM Client WHERE IdClient = @id", conn);
        checkClient.Parameters.AddWithValue("@id", clientId);
        if ((int)await checkClient.ExecuteScalarAsync() == 0) return "ClientNotFound";

        var checkTrip = new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip = @id", conn);
        checkTrip.Parameters.AddWithValue("@id", tripId);
        var maxPeople = await checkTrip.ExecuteScalarAsync();
        if (maxPeople == null) return "TripNotFound";

        var countCmd = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @id", conn);
        countCmd.Parameters.AddWithValue("@id", tripId);
        int current = (int)await countCmd.ExecuteScalarAsync();
        if (current >= (int)maxPeople) return "Full";

        var insertCmd = new SqlCommand(@"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
            VALUES (@clientId, @tripId, @regAt)", conn);
        insertCmd.Parameters.AddWithValue("@clientId", clientId);
        insertCmd.Parameters.AddWithValue("@tripId", tripId);
        insertCmd.Parameters.AddWithValue("@regAt", DateTime.Now);

        await insertCmd.ExecuteNonQueryAsync();
        return "Success";
    }

    public async Task<string> RemoveClientTrip(int clientId, int tripId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @cid AND IdTrip = @tid", conn);
        checkCmd.Parameters.AddWithValue("@cid", clientId);
        checkCmd.Parameters.AddWithValue("@tid", tripId);
        if ((int)await checkCmd.ExecuteScalarAsync() == 0) return "NotRegistered";

        var deleteCmd = new SqlCommand("DELETE FROM Client_Trip WHERE IdClient = @cid AND IdTrip = @tid", conn);
        deleteCmd.Parameters.AddWithValue("@cid", clientId);
        deleteCmd.Parameters.AddWithValue("@tid", tripId);
        await deleteCmd.ExecuteNonQueryAsync();

        return "Success";
    }
}