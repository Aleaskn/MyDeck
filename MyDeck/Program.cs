using MyDeck.Domain;
using MyDeck.Repositories;
using MyDeck.Services;

var repository = new LocalDeckRepository();
var service = new DeckService(repository);

while (true)
{
    Console.WriteLine("\n=== MyDeck ===");
    Console.WriteLine("1) Crea deck");
    Console.WriteLine("2) Lista deck");
    Console.WriteLine("3) Rinomina deck");
    Console.WriteLine("4) Elimina deck");
    Console.WriteLine("5) Aggiungi carta a un deck");
    Console.WriteLine("6) Rimuovi carta da un deck");
    Console.WriteLine("7) Dettagli deck");
    Console.WriteLine("0) Esci");
    Console.Write("Scelta: ");
    var choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                Console.Write("Nome nuovo deck: ");
                var name = Console.ReadLine() ?? "";
                var created = service.CreateDeck(name);
                Console.WriteLine($"Creato deck '{created.Name}' con Id {created.Id}");
                break;

            case "2":
                ShowDecks(service);
                break;

            case "3":
                {
                    var id = SelectDeck(service);
                    if (id == Guid.Empty) break;
                    Console.Write("Nuovo nome: ");
                    var newName = Console.ReadLine() ?? "";
                    service.RenameDeck(id, newName);
                    Console.WriteLine("Deck rinominato.");
                    break;
                }

            case "4":
                {
                    var id = SelectDeck(service);
                    if (id == Guid.Empty) break;
                    service.DeleteDeck(id);
                    Console.WriteLine("Deck eliminato.");
                    break;
                }

            case "5":
                {
                    var id = SelectDeck(service);
                    if (id == Guid.Empty) break;

                    Console.Write("Nome carta: ");
                    var cardName = Console.ReadLine() ?? "";
                    Console.Write("Costo mana (es. R, 1U, 2GG): ");
                    var mana = Console.ReadLine() ?? "";
                    Console.Write("Tipo (es. Creature, Instant): ");
                    var type = Console.ReadLine() ?? "";

                    service.AddCard(id, new Card { Name = cardName, ManaCost = mana, Type = type });
                    Console.WriteLine("Carta aggiunta.");
                    break;
                }

            case "6":
                {
                    var id = SelectDeck(service);
                    if (id == Guid.Empty) break;

                    Console.Write("Nome carta da rimuovere: ");
                    var removeName = Console.ReadLine() ?? "";
                    service.RemoveCard(id, removeName);
                    Console.WriteLine("Carte rimosse (tutte le occorrenze con quel nome).");
                    break;
                }

            case "7":
                {
                    var id = SelectDeck(service);
                    if (id == Guid.Empty) break;

                    var deck = service.GetDeck(id)!;
                    Console.WriteLine($"\nDeck: {deck.Name}  (Id: {deck.Id})");
                    if (deck.Cards.Count == 0) Console.WriteLine(" - Nessuna carta");
                    foreach (var c in deck.Cards)
                        Console.WriteLine($" - {c.Name} [{c.ManaCost}] {c.Type}");
                    break;
                }

            case "0":
                return;

            default:
                Console.WriteLine("Scelta non valida.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Errore: {ex.Message}");
    }
}

static void ShowDecks(DeckService service)
{
    var decks = service.GetAllDecks();
    if (decks.Count == 0)
    {
        Console.WriteLine("Nessun deck.");
        return;
    }

    Console.WriteLine("\n-- Deck esistenti --");
    for (int i = 0; i < decks.Count; i++)
        Console.WriteLine($"{i + 1}) {decks[i].Name}  (Carte: {decks[i].Cards.Count}, Id: {decks[i].Id})");
}

static Guid SelectDeck(DeckService service)
{
    var decks = service.GetAllDecks();
    if (decks.Count == 0)
    {
        Console.WriteLine("Nessun deck da selezionare.");
        return Guid.Empty;
    }

    ShowDecks(service);
    Console.Write("Seleziona numero deck: ");
    if (int.TryParse(Console.ReadLine(), out var n) && n >= 1 && n <= decks.Count)
        return decks[n - 1].Id;

    Console.WriteLine("Selezione non valida.");
    return Guid.Empty;
}
