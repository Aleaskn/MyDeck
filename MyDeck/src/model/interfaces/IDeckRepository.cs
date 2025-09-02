using MyDeck.Domain;

namespace MyDeck.Repositories;

public interface IDeckRepository
{
    List<Deck> GetAllDecks();
    Deck? GetDeck(Guid id);
    void SaveDeck(Deck deck);
    void DeleteDeck(Guid id);
}
