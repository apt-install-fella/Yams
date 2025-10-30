using Microsoft.VisualBasic;

namespace C_Sharp;

public class Yams
{
    private static readonly Dictionary<string, string> Raccourcis = new()
    {
        { "Un", "un" }, { "Deux", "de" }, { "Trois", "tr" }, { "Quatre", "qt" }, { "Cinq", "cq" }, { "Six", "si" },
        { "Brelan", "br" }, { "Carré", "cr" }, { "Full", "fl" }, { "Petite Suite", "ps" }, { "Grande Suite", "gs" },
        { "Yams", "ym" },
        { "Chance", "ch" },
    };

    public static void Main()
    {
        const string group = "groupe1-001";

        Json.GenererJson(DateTime.Now.ToString("yyyy-mm-dd"), group);
        var joueurs = new Joueur[2];
        for (var i = 0; i < 2; i++)
        {
            Console.Write($"\nEntrez le nom du joueur {i+1} : ");
            var c = Console.ReadLine();
            joueurs[i] = new Joueur(c is null or "" ? $"Joueur {i+1}" : c, i+1);
        }
        Json.EcritureJoueurs(1, joueurs[0].Nom!, 2, joueurs[1].Nom!);

        for (var i = 0; i < 13; i++)
        {
            Console.WriteLine($"Tour #{i+1}");
            var tours = new KeyValuePair<string, Dé[]>[2];
            for (var index = 0; index < joueurs.Length; index++)
            {
                var tour = Tour(ref joueurs[index]);
                if (tour is null) return;
                tours[index] = tour.Value;
                Console.Clear();
            }
            Json.EcritureTour(i, joueurs[0].Indice,tours[0].Value.Select(k => k.Val).ToArray(), tours[0].Key,
                Challenge.Challenges[joueurs[0].Challenges.Keys.ToList().IndexOf(tours[0].Key)](tours[0].Value),
                joueurs[1].Indice, tours[1].Value.Select(k => k.Val).ToArray(), tours[1].Key,
                Challenge.Challenges[joueurs[1].Challenges.Keys.ToList().IndexOf(tours[1].Key)](tours[1].Value));
        }

        Console.WriteLine($"+-----------------YAMS------------------+");
        foreach (var joueur in joueurs) Console.WriteLine($"|\t{joueur.Indice}] {joueur.Nom}: {joueur.Challenges.Sum(k => k.Value)}\t\t\t|");
        var meilleur = joueurs.OrderBy(k => k.Challenges.Sum(i => i.Value)).Last();
        Console.WriteLine($"| ------------------------------------- |");
        var extraTab = meilleur.Nom!.Length > 5 ? "" : "\t";
        Console.WriteLine($"|\tMeilleur: {meilleur.Nom} avec {meilleur.Challenges.Sum(i => i.Value)}\t{extraTab}|");
        Console.WriteLine($"+---------------------------------------+");
        Json.FinalResult(joueurs[0].Indice, joueurs[0].Nom ?? "Joueur 0", joueurs[0].Challenges.Sum(k => k.Value) ?? 0,
            joueurs[1].Indice, joueurs[1].Nom ?? "Joueur 1", joueurs[1].Challenges.Sum(k => k.Value) ?? 0);
    }

    private static KeyValuePair<string, Dé[]>? Tour(ref Joueur joueur)
    {
        Console.WriteLine($"Tour de {joueur.Nom}");
        var des = new Dé[5];
        var peutLancer = true;
        for (var i = 0; i < 3; i++)
        {
            Console.Write((des[0].Garder ? "-" : 1) + "\t" + (des[1].Garder ? "-" : 2) + "\t" + (des[2].Garder ? "-" : 3) + "\t" + (des[3].Garder ? "-" : 4) + "\t" + (des[4].Garder ? "-" : 5) + "\n" +
                          "INDICE POUR CONSERVER UN DÉ\n");
            if (peutLancer) LancerDes(ref des);
            else peutLancer = true;
            AfficherDes(des);
            Console.WriteLine((i != 2 ? "rl" : "--") + ") Relance les dés");
            AfficherChallenges(joueur.Challenges, des);
            Console.Write("\nVotre choix : ");
            var raccourcis = RaccourcisValides(joueur.Challenges, des);
            var res = "1";
            var truc = new string[5]; // Liste permet de comparer une chaîne de character potentiellement nombre à des nombre entre 1 et 5.
            for (var j = 0; j < 5; j++) truc[j] = j + 1 + "";
            while (int.TryParse(res, out _) && truc[int.Parse(res) - 1] == res) { // Tant que le dernier nombre est entre 1 et 5 (permet de garder plusieurs dés).
                res = Console.ReadLine();
                if (!int.TryParse(res, out _)) continue;
                if (truc.Length < int.Parse(res))
                {
                    res = "a";
                    continue;
                }
                if (truc[int.Parse(res) - 1] != res) continue;
                des[int.Parse(res) - 1].Garder = !des[int.Parse(res) - 1].Garder;
                Console.Write("\n" + (des[0].Garder ? "-" : 1) + "\t" + (des[1].Garder ? "-" : 2) + "\t" + (des[2].Garder ? "-" : 3) + "\t" + (des[3].Garder ? "-" : 4) + "\t" + (des[4].Garder ? "-" : 5) + "\n" + "INDICE POUR CONSERVER UN DÉ\n");
                Console.WriteLine(res);
                AfficherDes(des);
                Console.WriteLine("rl) Relance les dés");
                Console.Write("\nVotre choix : ");
            }
            if (res == "rl")
            {
                if (i == 2)
                {
                    i--;
                    peutLancer = false;
                    Console.WriteLine("Vous devez jouer un challenge.");
                }
                continue;
            }
            if (raccourcis.ContainsKey(res!))
            {
                var challengeTotal =
                    Challenge.Challenges[joueur.Challenges.Keys.ToList().IndexOf(raccourcis[res!])](des);
                joueur.Challenges[raccourcis[res!]] = challengeTotal;
                return new KeyValuePair<string, Dé[]>(raccourcis[res!], des);
            }
            // else
            Console.WriteLine("Ce challenge n'est pas disponible");
            peutLancer = false;
            i--;
        }

        return null;
    }

    private static Dictionary<string, string> RaccourcisValides(Dictionary<string, int?> challenges, Dé[] des)
    {
        var correctChallenge = new Dictionary<string, string>();
        var i = 0;
        foreach (var challenge in challenges)
        {
            var challengeResult = Challenge.Challenges[i](des);
            var challengesRacc = challenge.Value is not null;
            if (!challengesRacc) correctChallenge[Raccourcis[challenge.Key]] = challenge.Key;
            i++;
        }
        return correctChallenge;
    }

    private static void LancerDes(ref Dé[] des)
    {
        for (var index = 0; index < des.Length; index++)
            if (!des[index].Garder) des[index].Val = new Random().Next(1, 7);
    }

    private static void AfficherDes(Dé[] des)
    {
        foreach (var de in des) Console.Write(de.Val + "\t");
        Console.Write("\n");
    }

    private static void AfficherChallenges(Dictionary<string, int?> challenges, Dé[]? des)
    {
        var i = 0; // Permet de gérer l'affichage des challenges
        var bonus = challenges.ToList().Where((_, j) => j < 6).Sum(j => j.Value);
        Console.WriteLine($"{bonus}/63");
        foreach (var challenge in challenges)
        {
            if (i % 2 == 0) Console.Write("\n"); // Affiche les challenges 2 par lignes
            if (i is 6 or 12) Console.Write("\n"); // Sépare les types de challenges
            var challengeResult = Challenge.Challenges[i](des!);
            var challengesRacc = challenge.Value is not null ? "--" : Raccourcis[challenge.Key];
            var str = $"{challengesRacc}) {challenge.Key}: " + (challenge.Value ?? challengeResult);  // Affichage
            Console.Write(str + "\t");
            if (str.Length / 19 == 0) Console.Write("\t"); // Aligne moins si le premier challenge est trop gros
            // if (str.Length / 4 == 1) Console.Write("\t"); // Aligne plus si le premier challenge est trop petit
            i++;
        }
        Console.Write("\n");
    }

    public struct Joueur(string nom, int indice = 0)
    {
        public string? Nom { get; set; } = nom == "" ? "Nouveau joueur" : nom;
        public int Indice { get; set; } = indice;
        public Dictionary<string, int?> Challenges { get; set; } = new()
        {
            { "Un", null }, { "Deux", null }, { "Trois", null }, { "Quatre", null }, { "Cinq", null }, { "Six", null },
            { "Brelan", null }, { "Carré", null }, { "Full", null }, { "Petite Suite", null }, { "Grande Suite", null },
            { "Yams", null },
            { "Chance", null },
        };
    }

    public struct Dé(int val = 0) : IComparable<Dé>, IEqualityComparer<Dé>
    {
        // Structure d'un dé.
        private int _val = val;
        private bool _garder = false;

        public int CompareTo(Dé other) => _val.CompareTo(other._val); // Permet d'utiliser les fonctions built-in des Collections
        public bool Equals(Dé x, Dé y) => x.Val == y.Val;
        public int GetHashCode(Dé obj) => obj.Val.GetHashCode();  // Pour IEqualityComparer

        /*
         * Val:
         * get: Récupère la valeure du dé.
         * set: La valeure du dé doit être compri entre 1 et 6.
         */
        public int Val
        {
            get => _val;
            set => _val = value;
        }

        /*
         * Garder:
         * get: Récupère si le dé est gardé par l'utilisateur.
         * set: Modifie la valeur du dé.
         */
        public bool Garder
        {
            get => _garder;
            set => _garder = value;
        }
    }
}

public static class Challenge // Contient le test des challenges
{
    public static readonly List<Func<Yams.Dé[], int>> Challenges = [
        Un, Deux, Trois, Quatre, Cinq, Six,
        Brelan, Carré, Full, Petite, Grande, Yams,
        Chance
    ];

    private static int Un(Yams.Dé[] dés) => dés.Where(dé => dé.Val == 1).Sum(dé => dé.Val);
    private static int Deux(Yams.Dé[] dés) => dés.Where(dé => dé.Val == 2).Sum(dé => dé.Val);
    private static int Trois(Yams.Dé[] dés) => dés.Where(dé => dé.Val == 3).Sum(dé => dé.Val);
    private static int Quatre(Yams.Dé[] dés) => dés.Where(dé => dé.Val == 4).Sum(dé => dé.Val);
    private static int Cinq(Yams.Dé[] dés) => dés.Where(dé => dé.Val == 5).Sum(dé => dé.Val);
    private static int Six(Yams.Dé[] dés) => dés.Where(dé => dé.Val == 6).Sum(dé => dé.Val);

    private static int Brelan(Yams.Dé[] des)
    {
        var valeurs = new Dictionary<int, int> { {1, 0}, {2, 0}, {3, 0}, {4, 0}, {5, 0}, {6, 0} };
        foreach (var dé in des)
        {
            valeurs[dé.Val] += 1;
            if (valeurs[dé.Val] >= 3) return Chance(des);
        }
        return 0;
    }
    private static int Carré(Yams.Dé[] des)
    {
        var valeurs = new Dictionary<int, int> { {1, 0}, {2, 0}, {3, 0}, {4, 0}, {5, 0}, {6, 0} };
        foreach (var dé in des)
        {
            valeurs[dé.Val] += 1;
            if (valeurs[dé.Val] >= 4) return Chance(des);
        }
        return 0;
    }
    private static int Full(Yams.Dé[] des)
    {
        var valeurs = new Dictionary<int, int> { {1, 0}, {2, 0}, {3, 0}, {4, 0}, {5, 0}, {6, 0} };
        foreach (var dé in des) valeurs[dé.Val] += 1;
        return valeurs.Any(valeur => valeur.Value == 2 && Brelan(des) != 0) ? 25 : 0;
    }
    private static int Petite(Yams.Dé[] dés)
    {
        var nDes = dés.Select(d => d.Val).Distinct().OrderBy(val => val).ToArray();
        var suiteLength = 1;
        for (var i = 1; i < nDes.Length; i++)
        {
            if (nDes[i] == nDes[i - 1] + 1)
            {
                suiteLength++;
                if (suiteLength >= 4)
                    return 30;
            }
            else
                suiteLength = 1;
        }
        return 0;
    }
    private static int Grande(Yams.Dé[] dés)
    {
        var nDes = dés.Select(d => d.Val).Distinct().OrderBy(val => val).ToArray();
        var suiteLength = 1;
        for (var i = 1; i < nDes.Length; i++)
        {
            if (nDes[i] == nDes[i - 1] + 1)
            {
                suiteLength++;
                if (suiteLength >= 5)
                    return 30;
            }
            else
                suiteLength = 1;
        }
        return 0;
    }
    private static int Yams(Yams.Dé[] dés) => dés.Any(dé => dé.Val != dés[0].Val) ? 0 : 50;

    private static int Chance(Yams.Dé[] dés) => dés.Sum(dé => dé.Val);
}

public static class Json
{
    public static void GenererJson(string datePartie, string codePartie)
    {
        const string filePath = "yams_results.json";

        var jsonContent = $$"""
                            {
                                "parameters": {
                                    "code": "{{codePartie}}",
                                    "date": "{{datePartie}}"
                                },
                                "players": [],
                                "rounds": [],
                                "final_result": []
                            }
                            """;
        File.WriteAllText(filePath, jsonContent);
    }

    // Ajoute les joueurs au fichier JSON
    public static void EcritureJoueurs(int idJoueur1, string pseudo1, int idJoueur2, string pseudo2)
    {
        const string filePath = "yams_results.json";

        if (File.Exists(filePath))
        {
            var existingContent = File.ReadAllText(filePath);

            var playersContent = $$"""
                                   
                                       "players": [
                                           {
                                               "id": {{idJoueur1}},
                                               "pseudo": "{{pseudo1}}"
                                           },
                                           {
                                               "id": {{idJoueur2}},
                                               "pseudo": "{{pseudo2}}"
                                           }
                                       ]
                                   """;

            existingContent = existingContent.Replace("""
                                                      "players": []
                                                      """, playersContent);
            File.WriteAllText(filePath, existingContent);
        }
        else
        {
            Console.WriteLine($"Le fichier {filePath} n'existe pas.");
        }
    }

    // Ajoute les résultats des deux joueurs pour un round
    public static void EcritureTour(int roundId, int idJoueur1, int[] des1, string challenge1, int score1,
        int idJoueur2, int[] des2, string challenge2, int score2)
    {
        const string filePath = "yams_results.json";

        if (File.Exists(filePath))
        {
            var existingContent = File.ReadAllText(filePath);

            var roundsIndex = existingContent.LastIndexOf("\"rounds\": [", StringComparison.Ordinal);
            if (roundsIndex == -1) return;

            var closingBracketIndex = existingContent.LastIndexOf("],", StringComparison.Ordinal);
            if (closingBracketIndex == -1) return;
            if (roundId > 0) closingBracketIndex-=2;

            var newRound = $$"""
                             {
                                 "id": {{roundId}},
                                 "results": [
                                     {
                                         "id_player": {{idJoueur1}},
                                         "dice": [{{string.Join(", ", des1)}}],
                                         "challenge": "{{challenge1}}",
                                         "score": {{score1}}
                                     },
                                     {
                                         "id_player": {{idJoueur2}},
                                         "dice": [{{string.Join(", ", des2)}}],
                                         "challenge": "{{challenge2}}",
                                         "score": {{score2}}
                                     }
                                 ]
                             }
                             """;

            var updatedContent = existingContent.Substring(roundsIndex, closingBracketIndex - roundsIndex + 1).Trim() == "\"rounds\": []"
                ? existingContent.Replace("\"rounds\": []", $"\"rounds\": [{newRound}]")
                : existingContent.Insert(closingBracketIndex + 2, "," + newRound);

            File.WriteAllText(filePath, updatedContent);
        }
        else Console.WriteLine($"Le fichier {filePath} n'existe pas.");
    }

    public static void FinalResult(int idJoueur1, string nomJoueur1, int scoreJoueur1, int idJoueur2, string nomJoueur2, int scoreJoueur2) {
        const string filePath = "yams_results.json";

        if (File.Exists(filePath))
        {
            var existingContent = File.ReadAllText(filePath);

            var finalResultContent = $$"""
            "final_result": [
                {
                    "id_player": {{idJoueur1}},
                    "bonus": {{(scoreJoueur1 >= 63 ? 35 : 0)}},
                    "score": {{scoreJoueur1 + (scoreJoueur1 >= 63 ? 35 : 0)}}
                },
                {
                    "id_player": {{idJoueur2}},
                    "bonus": {{(scoreJoueur2 >= 63 ? 35 : 0)}},
                    "score": {{scoreJoueur2 + (scoreJoueur2 >= 63 ? 35 : 0)}}
                }
            ]
            """;

            // Remplace "final_result" vide 
            existingContent = existingContent.Replace("\"final_result\": []", finalResultContent);

            File.WriteAllText(filePath, existingContent);
        }
        else
        {
            Console.WriteLine($"Le fichier {filePath} n'existe pas.");
        }
    }
}
