namespace AgendaAvalonia.Models;

public class Tema
{
    public int Id { get; set; }
    public string Titlu { get; set; } = string.Empty;
    public string Materie { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public bool Finalizata { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}
