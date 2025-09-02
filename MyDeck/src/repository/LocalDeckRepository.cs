using System.Text.Json;
using MyDeck.Domain;

namespace MyDeck.Repositories;

public class LocalDeckRepository : IDeckRepository
{
    private readonly string _filePath = Path.Combine("Data", "decks.json");

    public LocalDeckRepository()
    {
        if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");
        if (!File.Exists(_filePath)) File.WriteAllText(_filePath, "[]");
    }

    public List<Deck> GetAllDecks()
    {
        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json)) return new List<Deck>();

        return JsonSerializer.Deserialize<List<Deck>>(json) ?? new List<Deck>();
    }

    public Deck? GetDeck(Guid id)
    {
        return GetAllDecks().FirstOrDefault(d => d.Id == id);
    }

    public void SaveDeck(Deck deck)
    {
        var decks = GetAllDecks();
        // rimuove eventuale deck con stesso Id
        decks.RemoveAll(d => d.Id == deck.Id);
        decks.Add(deck);
        File.WriteAllText(_filePath, JsonSerializer.Serialize(decks, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void DeleteDeck(Guid id)
    {
        var decks = GetAllDecks();
        decks.RemoveAll(d => d.Id == id);
        File.WriteAllText(_filePath, JsonSerializer.Serialize(decks, new JsonSerializerOptions { WriteIndented = true }));
    }
}
