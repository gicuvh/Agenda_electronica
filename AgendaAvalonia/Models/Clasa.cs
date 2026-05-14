namespace AgendaAvalonia.Models;

public class Clasa
{
    public int Id { get; set; }
    public string Nume { get; set; } = string.Empty;
    public DateTime DataCreare { get; set; } = DateTime.Now;

    public ICollection<User> Elevi { get; set; } = new List<User>();

    public override string ToString() => Nume;
}
