namespace MyDeck.Domain;

public class Deck
{
    public Guid Id { get; set; } = Guid.NewGuid(); // identificatore unico
    public string Name { get; set; } = "";
    public List<DeckCard> Cards { get; set; } = new();

    public DeckFormat Format { get; set; } = DeckFormat.Standard;
    public List<DeckCard> Sideboard { get; set; } = new();

    // Proprietà calcolate per ottenere facilmente il numero totale di carte
    public int TotalMainDeckCards => Cards.Sum(dc => dc.Quantity);
    public int TotalSideboardCards => Sideboard.Sum(dc => dc.Quantity);

}
