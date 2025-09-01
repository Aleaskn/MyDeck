using MyDeck.Domain;
using MyDeck.Repositories;
using MyDeck.Services;

var repository = new LocalDeckRepository();
var service = new DeckService(repository);

while (true)
{
    Console.Clear();
    Console.WriteLine("=== MY DECK MANAGER ===");
    Console.WriteLine("1. Visualizza tutti i deck");
    Console.WriteLine("2. Crea nuovo deck");
    Console.WriteLine("3. Seleziona deck");
    Console.WriteLine("0. Esci");
    Console.Write("Scelta: ");
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            var decks = service.GetAllDecks();
            if (!decks.Any())
            {
                Console.WriteLine("Nessun deck presente.");
            }
            else
            {
                foreach (var d in decks)
                    Console.WriteLine($"- {d.Name} (Id: {d.Id})");
            }
            Console.WriteLine("Premi un tasto per continuare...");
            Console.ReadKey();
            break;

        case "2":
            Console.Write("Nome del nuovo deck: ");
            var name = Console.ReadLine() ?? "Deck senza nome";
            var newDeck = service.CreateDeck(name);
            Console.WriteLine($"Deck '{newDeck.Name}' creato con Id {newDeck.Id}");
            Console.ReadKey();
            break;

        case "3":
            Console.Write("Inserisci Id del deck: ");
            if (Guid.TryParse(Console.ReadLine(), out var deckId))
            {
                var deck = service.GetDeck(deckId);
                if (deck == null)
                {
                    Console.WriteLine("Deck non trovato!");
                    Console.ReadKey();
                    break;
                }

                ManageDeck(deck, service);
            }
            else
            {
                Console.WriteLine("Id non valido!");
                Console.ReadKey();
            }
            break;

        case "0":
            return;
        default:
            Console.WriteLine("Scelta non valida!");
            Console.ReadKey();
            break;
    }
}

void ManageDeck(Deck deck, DeckService service)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine($"=== DECK: {deck.Name} ===");
        Console.WriteLine("Carte:");
        if (!deck.Cards.Any())
        {
            Console.WriteLine("  Nessuna carta");
        }
        else
        {
            foreach (var dc in deck.Cards)
                Console.WriteLine($"  {dc.Quantity}x {dc.Card.Name} [{dc.Card.Type}, {dc.Card.ManaCost}]");
        }

        Console.WriteLine("\nOpzioni:");
        Console.WriteLine("1. Aggiungi carta");
        Console.WriteLine("2. Rimuovi carta");
        Console.WriteLine("3. Rinomina deck");
        Console.WriteLine("4. Visualizza curva di mana");
        Console.WriteLine("5. Torna al menu principale");
        Console.Write("Scelta: ");
        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Console.Write("Nome carta: ");
                var cardName = Console.ReadLine() ?? "";
                Console.Write("Tipo carta: ");
                var cardType = Console.ReadLine() ?? "";
                Console.Write("Costo mana (es: 1R): ");
                var mana = Console.ReadLine() ?? "";
                Console.Write("Quantità: ");
                var qtyInput = Console.ReadLine();
                int qty = int.TryParse(qtyInput, out var q) ? q : 1;

                var card = new Card { Name = cardName, Type = cardType, ManaCost = mana };
                try
                {
                    service.AddCard(deck.Id, card, qty);
                    deck = service.GetDeck(deck.Id)!;
                    Console.WriteLine("Carta aggiunta!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore: {ex.Message}");
                }
                Console.ReadKey();
                break;

            case "2":
                Console.Write("Nome carta da rimuovere: ");
                var removeName = Console.ReadLine() ?? "";
                Console.Write("Quantità da rimuovere (lascia vuoto per rimuovere tutte): ");
                var qtyRemoveInput = Console.ReadLine();
                int qtyRemove = int.TryParse(qtyRemoveInput, out var qr) ? qr : int.MaxValue;

                service.RemoveCard(deck.Id, removeName, qtyRemove);
                deck = service.GetDeck(deck.Id)!;
                Console.WriteLine("Operazione completata!");
                Console.ReadKey();
                break;

            case "3":
                Console.Write("Nuovo nome deck: ");
                var newName = Console.ReadLine() ?? deck.Name;
                service.RenameDeck(deck.Id, newName);
                deck = service.GetDeck(deck.Id)!;
                Console.WriteLine("Deck rinominato!");
                Console.ReadKey();
                break;

            case "4":
                DisplayManaCurve(deck);
                Console.WriteLine("Premi un tasto per continuare...");
                Console.ReadKey();
                break;

            case "5":
                return;
            default:
                Console.WriteLine("Scelta non valida!");
                Console.ReadKey();
                break;
        }
    }
}

void DisplayManaCurve(Deck deck)
{
    Console.WriteLine("\n=== CURVA DI MANA ===");
    var curve = new Dictionary<int, int>();

    foreach (var dc in deck.Cards)
    {
        int cost = ParseManaCost(dc.Card.ManaCost);
        if (!curve.ContainsKey(cost)) curve[cost] = 0;
        curve[cost] += dc.Quantity;
    }

    foreach (var kvp in curve.OrderBy(k => k.Key))
        Console.WriteLine($"Costo {kvp.Key}: {kvp.Value} carte");
}

int ParseManaCost(string manaCost)
{
    // Conteggio semplice: somma numeri nel costo mana, ignora simboli colore
    int total = 0;
    var numStr = "";
    foreach (var c in manaCost)
    {
        if (char.IsDigit(c))
        {
            numStr += c;
        }
        else
        {
            if (!string.IsNullOrEmpty(numStr))
            {
                total += int.Parse(numStr);
                numStr = "";
            }
            total += 1; // simboli colore contano 1
        }
    }
    if (!string.IsNullOrEmpty(numStr)) total += int.Parse(numStr);
    return total;
}
