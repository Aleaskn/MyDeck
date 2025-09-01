using MyDeck.Domain;
using MyDeck.Repositories;

namespace MyDeck.Services;

public class DeckService
{
    private readonly IDeckRepository _repository;

    public DeckService(IDeckRepository repository)
    {
        _repository = repository;
    }

    public Deck CreateDeck(string name)
    {
        var deck = new Deck { Name = name };
        _repository.SaveDeck(deck);
        return deck; // ritorniamo il deck per ottenere l'Id
    }

    public void AddCard(Guid deckId, Card card, int quantity = 1)
    {
        var deck = _repository.GetDeck(deckId) ?? throw new Exception("Deck non trovato!");

        var existing = deck.Cards.FirstOrDefault(c => 
            c.Card.Name.Equals(card.Name, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            // Gestione limite 4 copie, tranne terre base
            var isBasicLand = card.Type.Contains("Basic Land", StringComparison.OrdinalIgnoreCase);
            var newQuantity = existing.Quantity + quantity;

            if (!isBasicLand && newQuantity > 4)
                throw new Exception("Non puoi avere più di 4 copie di questa carta (tranne le terre base).");

            existing.Quantity = newQuantity;
        }
        else
        {
            if (quantity > 4 && !card.Type.Contains("Basic Land", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Non puoi aggiungere più di 4 copie di questa carta.");
            
            deck.Cards.Add(new DeckCard { Card = card, Quantity = quantity });
        }

        _repository.SaveDeck(deck);
    }

    public Deck? GetDeck(Guid id) => _repository.GetDeck(id);

    public void RenameDeck(Guid deckId, string newName)
    {
        var deck = _repository.GetDeck(deckId) ?? throw new Exception("Deck non trovato!");
        
        // Impedisce rinomina a nome già usato 
         if (_repository.GetAllDecks().Any(d => 
             d.Id != deckId && d.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
         {
             throw new Exception("Esiste già un deck con questo nome!");
         }

        deck.Name = newName.Trim();
        _repository.SaveDeck(deck);
    }

    public void RemoveCard(Guid deckId, string cardName, int quantity = int.MaxValue)
    {
        var deck = _repository.GetDeck(deckId) ?? throw new Exception("Deck non trovato!");
        var existing = deck.Cards.FirstOrDefault(dc =>
            dc.Card.Name.Equals(cardName, StringComparison.OrdinalIgnoreCase));

        if (existing == null) return;

        if (quantity >= existing.Quantity)
            deck.Cards.RemoveAll(dc => dc.Card.Name.Equals(cardName, StringComparison.OrdinalIgnoreCase));
        else
            existing.Quantity -= quantity;

        _repository.SaveDeck(deck);
    }

    // Metodo per prendere la curva di mana del deck
    public Dictionary<int, int> GetManaCurve(Guid deckId)
    {
        var deck = _repository.GetDeck(deckId) ?? throw new Exception("Deck non trovato!");

        var curve = new Dictionary<int, int>();

        foreach (var dc in deck.Cards)
        {
            int cmc = CalculateCmc(dc.Card.ManaCost);
            if (!curve.ContainsKey(cmc)) curve[cmc] = 0;
            curve[cmc] += dc.Quantity;
        }

        return curve;
    }

    // Metodo semplificato per costo mana convertito
    private int CalculateCmc(string manaCost)
    {
        // Esempi: "1U" => 2, "2GG" => 4, "R" => 1
        int total = 0;
        string num = "";

        foreach (char c in manaCost.ToUpper())
        {
            if (char.IsDigit(c))
            {
                num += c;
            }
            else
            {
                if (num != "")
                {
                    total += int.Parse(num);
                    num = "";
                }
                total += 1; // ogni simbolo di mana singolo vale 1
            }
        }

        if (num != "") total += int.Parse(num);
        return total;
    }
    
    public List<Deck> GetAllDecks() => _repository.GetAllDecks();

    public void DeleteDeck(Guid id) => _repository.DeleteDeck(id);
}
