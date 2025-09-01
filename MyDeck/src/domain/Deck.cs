namespace MyDeck.Domain;

public class Deck
{
    public Guid Id { get; set; } = Guid.NewGuid(); // identificatore unico
    public string Name { get; set; } = "";
    public List<DeckCard> Cards { get; set; } = new();

    // gestisce più copie della stessa carta
    public void AddCard(Card card, int quantity = 1)
    {
        // cerca se la carta è già presente
        var existing = Cards.FirstOrDefault(dc => dc.Card.Name.Equals(card.Name, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            Cards.Add(new DeckCard { Card = card, Quantity = quantity });
        }
    }

    // Qui sommiamo tutte le quantità per stampare il numero totale di carte.
    // Poi stampiamo ogni carta con la quantità (xN)
    public void ShowDeck()
    {
        int totalCards = Cards.Sum(dc => dc.Quantity);
        Console.WriteLine($"Deck: {Name} ({totalCards} carte)");
        foreach (var dc in Cards)
        {
            Console.WriteLine($"- {dc.Card.Name} x{dc.Quantity} [{dc.Card.ManaCost}] {dc.Card.Type}");
        }
    }

}
