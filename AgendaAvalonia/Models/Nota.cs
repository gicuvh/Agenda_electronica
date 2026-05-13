namespace AgendaAvalonia.Models;

public class Nota
{
    public int Id { get; set; }
    public string Materie { get; set; } = string.Empty;
    public int Valoare { get; set; }
    public DateTime Data { get; set; } = DateTime.Now;
    public string? Descriere { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}
