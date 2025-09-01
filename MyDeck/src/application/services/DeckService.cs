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

    public void AddCard(Guid deckId, Card card)
    {
        var deck = _repository.GetDeck(deckId);
        if (deck == null) throw new Exception("Deck non trovato!");

        deck.Cards.Add(card);
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

    public void RemoveCard(Guid deckId, string cardName)
    {
        var deck = _repository.GetDeck(deckId);
        if (deck == null) throw new Exception("Deck non trovato!");

        deck.Cards.RemoveAll(c => c.Name == cardName);
        _repository.SaveDeck(deck);
    }

    public List<Deck> GetAllDecks() => _repository.GetAllDecks();

    public void DeleteDeck(Guid id) => _repository.DeleteDeck(id);
}
