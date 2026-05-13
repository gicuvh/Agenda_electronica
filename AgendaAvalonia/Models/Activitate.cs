namespace AgendaAvalonia.Models;

public class Activitate
{
    public int Id { get; set; }
    public string Descriere { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Tip { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User? User { get; set; }
}
