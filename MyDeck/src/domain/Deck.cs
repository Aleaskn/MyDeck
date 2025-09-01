namespace MyDeck.Domain;

public class Deck
{
    public string Name { get; set; } = "";
    public List<Card> Cards { get; set; } = new();

    public void AddCard(Card card)
    {
        Cards.Add(card);
    }

    public void ShowDeck()
    {
        Console.WriteLine($"Deck: {Name} ({Cards.Count} carte)");
        foreach (var card in Cards)
        {
            Console.WriteLine($"- {card.Name} [{card.ManaCost}] {card.Type}");
        }
    }
}
