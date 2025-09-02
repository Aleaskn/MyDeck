namespace MyDeck.Domain;

public class DeckCard
{
    public Card Card { get; set; } = new Card();
    public int Quantity { get; set; } = 1;
}
