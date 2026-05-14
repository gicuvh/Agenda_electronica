namespace AgendaAvalonia.Models;

public class User
{
    public int Id { get; set; }
    public string NumeComplet { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "Elev";
    public DateTime DataInregistrare { get; set; } = DateTime.Now;
    public int? ClasaId { get; set; }
    public Clasa? Clasa { get; set; }

    public ICollection<Nota> Note { get; set; } = new List<Nota>();
    public ICollection<Tema> Teme { get; set; } = new List<Tema>();
    public ICollection<OrarEntry> OrarEntries { get; set; } = new List<OrarEntry>();
    public ICollection<Activitate> Activitati { get; set; } = new List<Activitate>();
}
