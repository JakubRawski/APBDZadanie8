namespace Tutorial8.Models.DTOs;

public class Client
{
    public int IdClient { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Telephone { get; set; }
    public string Pesel { get; set; }
}

public class Trip
{
    public int IdTrip { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxSeats { get; set; }
    public List<Country> Countries { get; set; } = new List<Country>();
}
public class Country
{
    public int IdCountry { get; set; }
    public string Name { get; set; }
}
public class ClientTrip
{
    public int IdClient { get; set; }
    public int IdTrip { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; } // Zmieniono na DateTime?
}
public class CountryTrip
{
    public int IdCountry { get; set; }
    public int IdTrip { get; set; }
}
public class TripDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxSeats { get; set; }
    public List<CountryDTO> Countries { get; set; }
}

public class CountryDTO
{
    public string Name { get; set; }
}
public class ClientTripDTO
{
    public int TripId { get; set; }
    public string TripName { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; } // Zmieniono na DateTime?
    public List<CountryDTO> Countries { get; set; }
}
public class NewClientDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Telephone { get; set; }
    public string Pesel { get; set; }
}
