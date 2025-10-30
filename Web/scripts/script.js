// Variables globales
let data = null;
let gameId = ''; // ID de la partie
const display = document.getElementById("current-action");
const gauche = document.getElementById("fleche-gauche");
const droite = document.getElementById("fleche-droite");

let indexActu = 0; // Index actuel pour naviguer

// Fonction pour récupérer les paramètres d'une partie
function fetchParameters() {
  return fetch(`http://yams.iutrs.unistra.fr:3000/api/games/${gameId}/parameters`)
    .then(response => response.json())
    .catch(error => {
      console.error('Erreur lors de la récupération des paramètres:', error);
    });
}

// Fonction pour récupérer les joueurs d'une partie
function fetchPlayers() {
  return fetch(`http://yams.iutrs.unistra.fr:3000/api/games/${gameId}/players`)
    .then(response => response.json())
    .catch(error => {
      console.error('Erreur lors de la récupération des joueurs:', error);
    });
}

// Fonction pour récupérer les résultats d'un tour
function fetchRound(roundIndex) {
  return fetch(`http://yams.iutrs.unistra.fr:3000/api/games/${gameId}/rounds/${roundIndex}`)
    .then(response => response.json())
    .catch(error => {
      console.error(`Erreur lors de la récupération du tour ${roundIndex}:`, error);
    });
}

// Fonction pour récupérer le résultat final
function fetchFinalResult() {
  return fetch(`http://yams.iutrs.unistra.fr:3000/api/games/${gameId}/final-result`)
    .then(response => response.json())
    .catch(error => {
      console.error('Erreur lors de la récupération du résultat final:', error);
    });
}

// Fonction pour afficher l'action actuelle
function displayAction(index) {
  if (!data) {
    console.error("Les données n'ont pas encore été chargées.");
    return;
  }

  const roundIndex = Math.floor(index / 2); //Determiner les tours
  const playerIndex = index % 2; // Détermine le joueur (0 pour le premier, 1 pour le second)

  const round = data.rounds[roundIndex];
  const player = data.players[playerIndex];
  const result = round.results.find(r => r.id_player === player.id);

  display.innerHTML = `
    <h2>Round ${round.id} - joueur: ${player.pseudo}</h2>
    <p>Dés: ${result.dice.join(", ")}</p>
    <p>Challenge: ${result.challenge}</p>
    <p>Score: ${result.score}</p>
  `; //Affichage dans la page web
}

// Fonction pour charger toutes les données
function loadGameData() {
  if (!gameId) {
    alert("Veuillez entrer un identifiant de partie valide.");
    return;
  }

  // Appel de toutes les fonctions pour récupérer les différentes données
  Promise.all([
    fetchParameters(),
    fetchPlayers(),
    fetchFinalResult(),
  ])
    .then(results => {
      // Les résultats des fetchs seront stockés dans un tableau
      const [parameters, players, finalResult] = results;

      // Stocke toutes les données dans une structure
      data = {
        parameters,
        players,
        rounds: Array.from({ length: 13 }, (_, index) => fetchRound(index + 1)), // Charger les 13 rounds
        finalResult
      };

      // Attendre que tous les rounds soient chargés
      Promise.all(data.rounds)
        .then(rounds => {
          data.rounds = rounds;
          displayAction(indexActu);  // Affiche l'action initiale après chargement des données
        })
        .catch(error => {
          console.error("Erreur lors du chargement des tours:", error);
        });
    })
    .catch(error => {
      console.error("Erreur lors du chargement des données du jeu:", error);
    });
}

// Événements pour naviguer avec les flèches
gauche.addEventListener("click", () => {
  if (indexActu > 0) {
    indexActu--;
    displayAction(indexActu);
  }
});

droite.addEventListener("click", () => {
  if (indexActu < data.rounds.length * 2 - 1) {
    indexActu++;
    displayAction(indexActu);
  }
});

// Événement pour récupérer l'ID de la partie
document.getElementById("submit-id").addEventListener("click", function () {
  const inputId = document.getElementById("game-id").value.trim();
  if (inputId) {
    gameId = inputId;  // Mise à jour de l'ID de la partie
    loadGameData();  // Charger les données de la partie
  } else {
    alert("Veuillez entrer un identifiant de partie valide.");
  }
});
