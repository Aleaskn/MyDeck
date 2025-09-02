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
    Console.WriteLine("4. Importa deck da file");
    Console.WriteLine("0. Esci");
    Console.Write("Scelta: ");
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            ViewAllDecks();
            break;
        case "2":
            CreateNewDeck();
            break;
        case "3":
            SelectDeck();
            break;
        case "4":
            ImportDeckFromFile();
            break;
        case "0":
            return;
        default:
            Console.WriteLine("Scelta non valida!");
            Console.ReadKey();
            break;
    }
}

void ViewAllDecks()
{
    var decks = service.GetAllDecks();
    if (!decks.Any())
    {
        Console.WriteLine("Nessun deck presente.");
    }
    else
    {
        foreach (var d in decks)
            Console.WriteLine($"- {d.Name} ({d.Format}) [Id: {d.Id}]");
    }
    Console.WriteLine("\nPremi un tasto per continuare...");
    Console.ReadKey();
}

void CreateNewDeck()
{
    Console.Write("Nome del nuovo deck: ");
    var name = Console.ReadLine() ?? "Deck senza nome";

    // Scelta del formato
    Console.WriteLine("Scegli un formato:");
    var formats = Enum.GetValues<DeckFormat>();
    for (int i = 0; i < formats.Length; i++)
    {
        Console.WriteLine($"{i + 1}. {formats[i]}");
    }
    Console.Write("Scelta: ");
    if (!int.TryParse(Console.ReadLine(), out var formatChoice) || formatChoice < 1 || formatChoice > formats.Length)
    {
        Console.WriteLine("Scelta non valida!");
        Console.ReadKey();
        return;
    }
    var selectedFormat = formats[formatChoice - 1];

    var newDeck = service.CreateDeck(name, selectedFormat);
    Console.WriteLine($"Deck '{newDeck.Name}' ({selectedFormat}) creato con Id {newDeck.Id}");
    Console.ReadKey();
}

void SelectDeck()
{
    Console.Write("Inserisci Id del deck: ");
    if (Guid.TryParse(Console.ReadLine(), out var deckId))
    {
        var deck = service.GetDeck(deckId);
        if (deck == null)
        {
            Console.WriteLine("Deck non trovato!");
        }
        else
        {
            ManageDeck(deck);
        }
    }
    else
    {
        Console.WriteLine("Id non valido!");
    }
    Console.ReadKey();
}

void ManageDeck(Deck deck)
{
    while (true)
    {
        deck = service.GetDeck(deck.Id)!; // Ricarica il deck per avere sempre i dati aggiornati
        Console.Clear();

        // Visualizzazione più dettagliata del deck
        Console.WriteLine($"=== GESTIONE DECK: {deck.Name} ({deck.Format}) ===");
        Console.WriteLine($"Carte nel mazzo: {deck.TotalMainDeckCards} | Carte nel sideboard: {deck.TotalSideboardCards}\n");

        // Avvisi di validazione per il formato
        var warnings = service.ValidateDeck(deck);
        if (warnings.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var warning in warnings) Console.WriteLine($"ATTENZIONE: {warning}");
            Console.ResetColor();
            Console.WriteLine();
        }

        Console.WriteLine("--- MAZZO PRINCIPALE ---");
        if (!deck.Cards.Any())
            Console.WriteLine("  Nessuna carta");
        else
            foreach (var dc in deck.Cards.OrderBy(c => c.Card.Name))
                Console.WriteLine($"  {dc.Quantity}x {dc.Card.Name} [{dc.Card.ManaCost}, {dc.Card.Type}]");

        Console.WriteLine("\n--- SIDEBOARD ---");
        if (!deck.Sideboard.Any())
            Console.WriteLine("  Nessuna carta");
        else
            foreach (var dc in deck.Sideboard.OrderBy(c => c.Card.Name))
                Console.WriteLine($"  {dc.Quantity}x {dc.Card.Name} [{dc.Card.ManaCost}, {dc.Card.Type}]");


        Console.WriteLine("\n--- OPZIONI ---");
        Console.WriteLine("1. Aggiungi carta al mazzo");
        Console.WriteLine("2. Aggiungi carta al sideboard");
        Console.WriteLine("3. Rimuovi carta");
        Console.WriteLine("4. Rinomina deck");
        Console.WriteLine("5. Visualizza curva di mana");
        Console.WriteLine("6. Esporta deck su file");
        Console.WriteLine("0. Torna al menu principale");
        Console.Write("Scelta: ");
        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                AddCardToDeck(deck.Id, false);
                break;
            case "2":
                AddCardToDeck(deck.Id, true);
                break;
            case "3":
                RemoveCardFromDeck(deck);
                break;
            case "4":
                Console.Write("Nuovo nome deck: ");
                var newName = Console.ReadLine() ?? deck.Name;
                service.RenameDeck(deck.Id, newName);
                Console.WriteLine("Deck rinominato!");
                Console.ReadKey();
                break;
            case "5":
                DisplayManaCurve(deck.Id);
                Console.ReadKey();
                break;
            case "6":
                ExportDeck(deck.Id);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Scelta non valida!");
                Console.ReadKey();
                break;
        }
    }
}

void AddCardToDeck(Guid deckId, bool toSideboard)
{
    Console.Write("Nome carta: ");
    var cardName = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(cardName)) return;

    Console.Write("Tipo carta (es: Creature, Instant, Basic Land): ");
    var cardType = Console.ReadLine() ?? "";
    Console.Write("Costo mana (es: 1R, 2GG): ");
    var mana = Console.ReadLine() ?? "";
    Console.Write("Quantità: ");
    int.TryParse(Console.ReadLine(), out var qty);
    if (qty <= 0) qty = 1;

    var card = new Card { Name = cardName, Type = cardType, ManaCost = mana };
    try
    {
        service.AddCard(deckId, card, qty, toSideboard);
        Console.WriteLine($"Carta aggiunta al {(toSideboard ? "sideboard" : "mazzo")}!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Errore: {ex.Message}");
    }
    Console.ReadKey();
}

// Mostra la lista di carte del deck prima e poi chiede da dove rimuovere
void RemoveCardFromDeck(Deck deck)
{
    Console.WriteLine("\nDa dove vuoi rimuovere la carta?");
    Console.WriteLine("1. Mazzo principale");
    Console.WriteLine("2. Sideboard");
    Console.Write("Scelta: ");
    var listChoice = Console.ReadLine();
    bool fromSideboard = listChoice == "2";
    
    var cardList = fromSideboard ? deck.Sideboard : deck.Cards;
    if (!cardList.Any())
    {
        Console.WriteLine("Questa sezione è vuota.");
        Console.ReadKey();
        return;
    }
    
    Console.Write("Nome carta da rimuovere: ");
    var removeName = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(removeName)) return;

    Console.Write("Quantità da rimuovere (lascia vuoto per rimuovere tutte): ");
    int.TryParse(Console.ReadLine(), out var qtyRemove);
    if (qtyRemove <= 0) qtyRemove = int.MaxValue;

    service.RemoveCard(deck.Id, removeName, qtyRemove, fromSideboard);
    Console.WriteLine("Operazione completata!");
    Console.ReadKey();
}

// Usiamo il DeckService per la curva di mana
void DisplayManaCurve(Guid deckId)
{
    Console.WriteLine("\n=== CURVA DI MANA (escluse le terre) ===");
    var curve = service.GetManaCurve(deckId);

    if (!curve.Any())
    {
        Console.WriteLine("Nessuna carta con costo di mana presente.");
        return;
    }

    var maxCount = curve.Values.Max();
    foreach (var kvp in curve.OrderBy(k => k.Key))
    {
        var bar = new string('█', (int)Math.Round((double)kvp.Value * 20 / maxCount));
        Console.WriteLine($"Costo {kvp.Key} ({kvp.Value,2} carte): {bar}");
    }
    Console.WriteLine("\nPremi un tasto per continuare...");
}

// Logica per l'export
void ExportDeck(Guid deckId)
{
    try
    {
        var deck = service.GetDeck(deckId)!;
        var deckString = service.ExportDeckToString(deckId);
        string fileName = $"{deck.Name.Replace(" ", "_")}.txt";
        File.WriteAllText(fileName, deckString);
        Console.WriteLine($"\nDeck esportato con successo nel file: {Path.GetFullPath(fileName)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Errore durante l'export: {ex.Message}");
    }
    Console.ReadKey();
}

// Logica per l'import
void ImportDeckFromFile()
{
    Console.Write("Inserisci il percorso completo del file .txt da importare: ");
    var filePath = Console.ReadLine();

    if (!File.Exists(filePath))
    {
        Console.WriteLine("File non trovato!");
        Console.ReadKey();
        return;
    }

    try
    {
        Console.Write("Nome da dare al nuovo deck: ");
        var deckName = Console.ReadLine() ?? "Deck Importato";
        
        // Per ora, l'import crea un deck Standard di default. Si può migliorare chiedendo all'utente.
        var deck = service.CreateDeck(deckName, DeckFormat.Standard); 
        
        var lines = File.ReadAllLines(filePath);
        bool isSideboard = false;
        var knownCards = new Dictionary<string, Card>(); // Cache per non chiedere più volte la stessa carta

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;
            if (line.Trim().Equals("Sideboard", StringComparison.OrdinalIgnoreCase))
            {
                isSideboard = true;
                continue;
            }

            var parts = line.Trim().Split(new[] { ' ' }, 2);
            if (parts.Length != 2 || !int.TryParse(parts[0], out var qty)) continue;

            var cardName = parts[1];
            Card card;

            if (knownCards.ContainsKey(cardName.ToLower()))
            {
                card = knownCards[cardName.ToLower()];
            }
            else
            {
                Console.WriteLine($"\nDettagli richiesti per la carta: '{cardName}'");
                Console.Write("Tipo: ");
                var type = Console.ReadLine() ?? "";
                Console.Write("Costo Mana: ");
                var mana = Console.ReadLine() ?? "";
                card = new Card { Name = cardName, Type = type, ManaCost = mana };
                knownCards[cardName.ToLower()] = card;
            }

            service.AddCard(deck.Id, card, qty, isSideboard);
        }
        Console.WriteLine($"\nDeck '{deckName}' importato con successo!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Errore durante l'import: {ex.Message}");
    }
    Console.ReadKey();
}
