using MyDeck.Domain;

var myDeck = new Deck { Name = "Mono Rosso Aggro" };

myDeck.AddCard(new Card { Name = "Lightning Bolt", ManaCost = "R", Type = "Instant" });
myDeck.AddCard(new Card { Name = "Goblin Guide", ManaCost = "R", Type = "Creature" });

myDeck.ShowDeck();

Console.WriteLine("\nPremi un tasto per uscire...");
Console.ReadKey();
