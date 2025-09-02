using MyDeck.Domain;
using MyDeck.Repositories;
using System.Text;

namespace MyDeck.Services;

public class DeckService
{
    private readonly IDeckRepository _repository;

    public DeckService(IDeckRepository repository)
    {
        _repository = repository;
    }

    public Deck CreateDeck(string name, DeckFormat format)
    {
        var deck = new Deck { Name = name, Format = format };
        _repository.SaveDeck(deck);
        return deck; // ritorniamo il deck per ottenere l'Id
    }

    // Metodo per aggiungere una carta al main deck e nella sidebord se presente
    // Controllo per la quantità delle carte 
    public void AddCard(Guid deckId, Card card, int quantity = 1, bool toSideboard = false)
    {
        var deck = _repository.GetDeck(deckId) ?? throw new Exception("Deck non trovato!");

        // Controlla il limite delle 4 copie tra main deck & sideboard
        var totalQuantityInDeck = GetCardCount(deck, card.Name);
        var isBasicLand = card.Type.Contains("Basic Land", StringComparison.OrdinalIgnoreCase);

        if (!isBasicLand && totalQuantityInDeck + quantity > 4)
            throw new Exception("Non puoi avere più di 4 copie di una carta (tranne le terre base) tra mazzo e sideboard.");

        var targetList = toSideboard ? deck.Sideboard : deck.Cards;

        // Controllo limite sideboard
        if (toSideboard && deck.TotalSideboardCards + quantity > 15)
            throw new Exception("Il sideboard non può contenere più di 15 carte.");

        var existing = targetList.FirstOrDefault(c =>
            c.Card.Name.Equals(card.Name, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            targetList.Add(new DeckCard { Card = card, Quantity = quantity });
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

    public void RemoveCard(Guid deckId, string cardName, int quantity, bool fromSideboard = false)
    {
        var deck = _repository.GetDeck(deckId) ?? throw new Exception("Deck non trovato!");
        var targetList = fromSideboard ? deck.Sideboard : deck.Cards;

        var existing = targetList.FirstOrDefault(dc => dc.Card.Name.Equals(cardName, StringComparison.OrdinalIgnoreCase));
        if (existing == null) return;

        if (quantity >= existing.Quantity)
            targetList.Remove(existing);
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
            // Non includiamo le terre nella curva di mana
            if (dc.Card.Type.Contains("Land", StringComparison.OrdinalIgnoreCase)) continue;

            int cmc = GetConvertedManaCost(dc.Card.ManaCost);
            if (!curve.ContainsKey(cmc)) curve[cmc] = 0;
            curve[cmc] += dc.Quantity;
        }

        return curve;
    }

    // Metodo per calcolare il CMC
    public int GetConvertedManaCost(string manaCost)
    {
        if (string.IsNullOrWhiteSpace(manaCost)) return 0;
        
        int total = 0;
        string num = "";

        foreach (char c in manaCost.ToUpper())
        {
            if (char.IsDigit(c))
            {
                num += c;
            }
            // Ignora simboli non alfanumerici come '(', ')', '/'
            else if (char.IsLetterOrDigit(c))
            {
                if (num != "")
                {
                    total += int.Parse(num);
                    num = "";
                }
                // Simboli come 'X' contano 0 nel CMC, gli altri 1
                if (c != 'X')
                {
                    total += 1;
                }
            }
        }
        if (num != "") total += int.Parse(num);
        return total;
    }

    // Metodo per validare un deck secondo le regole per il formato scelto 
        public List<string> ValidateDeck(Deck deck)
    {
        var warnings = new List<string>();
        int minDeckSize = 0;

        switch (deck.Format)
        {
            case DeckFormat.Standard:
                minDeckSize = 60;
                break;
            case DeckFormat.Commander:
                minDeckSize = 100; // 99 + 1 commander, ma per ora controlliamo 100 totali
                break;
        }

        if (deck.TotalMainDeckCards < minDeckSize)
            warnings.Add($"Il mazzo principale ha {deck.TotalMainDeckCards}/{minDeckSize} carte (minimo richiesto).");

        if (deck.TotalSideboardCards > 15)
            warnings.Add($"Il sideboard ha {deck.TotalSideboardCards}/15 carte (massimo consentito).");

        return warnings;
    }

    // Logica per l'export del deck in formato testo
        public string ExportDeckToString(Guid deckId)
    {
        var deck = GetDeck(deckId) ?? throw new Exception("Deck non trovato!");
        var builder = new StringBuilder();
        
        builder.AppendLine($"// Deck: {deck.Name}");
        builder.AppendLine($"// Format: {deck.Format}");
        builder.AppendLine();

        foreach (var dc in deck.Cards.OrderBy(c => c.Card.Name))
        {
            builder.AppendLine($"{dc.Quantity} {dc.Card.Name}");
        }

        if (deck.Sideboard.Any())
        {
            builder.AppendLine();
            builder.AppendLine("Sideboard");
            foreach (var dc in deck.Sideboard.OrderBy(c => c.Card.Name))
            {
                builder.AppendLine($"{dc.Quantity} {dc.Card.Name}");
            }
        }
        return builder.ToString();
    }

    // Metodo helper per contare le copie di una carta tra mazzo e sideboard
    private int GetCardCount(Deck deck, string cardName)
    {
        var mainCount = deck.Cards.FirstOrDefault(c => c.Card.Name.Equals(cardName, StringComparison.OrdinalIgnoreCase))?.Quantity ?? 0;
        var sideCount = deck.Sideboard.FirstOrDefault(c => c.Card.Name.Equals(cardName, StringComparison.OrdinalIgnoreCase))?.Quantity ?? 0;
        return mainCount + sideCount;
    }

    public List<Deck> GetAllDecks() => _repository.GetAllDecks();

    public void DeleteDeck(Guid id) => _repository.DeleteDeck(id);
}
