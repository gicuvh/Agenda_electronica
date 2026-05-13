namespace AgendaWinForms.Models;

public class OrarEntry
{
    public int Id { get; set; }
    public string ZiSaptamana { get; set; } = string.Empty;
    public TimeSpan OraInceput { get; set; }
    public TimeSpan OraSfarsit { get; set; }
    public string Materie { get; set; } = string.Empty;
    public string? Profesor { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}
