// Sélection des éléments HTML
const vainqueurElem = document.getElementById("vainqueur");
const scoreFinalElem = document.getElementById("score-final");
const joueursElem = document.getElementById("joueurs");
const loadResultsBtn = document.getElementById("loadResultsBtn");
const gameIdInput = document.getElementById("gameIdInput");

// Fonction pour récupérer le résultat final de la partie
function fetchFinalResult(gameId) {
  return fetch(`http://yams.iutrs.unistra.fr:3000/api/games/${gameId}/final-result`)
    .then(response => response.json())
    .then(data => data)
    .catch(error => {
      console.error('Erreur lors de la récupération du résultat final:', error);
    });
}

// Fonction pour récupérer les joueurs d'une partie
function fetchPlayers(gameId) {
  return fetch(`http://yams.iutrs.unistra.fr:3000/api/games/${gameId}/players`)
    .then(response => response.json())
    .then(data => data)
    .catch(error => {
      console.error('Erreur lors de la récupération des joueurs:', error);
    });
}

// Fonction pour afficher les résultats
function displayResults(gameId) {
  // On récupère les données nécessaires
  Promise.all([fetchPlayers(gameId), fetchFinalResult(gameId)])
    .then(results => {
      const [players, finalResult] = results;

      // Trouver le joueur avec le score maximum
      const winner = finalResult.reduce((max, player) => player.score > max.score ? player : max, finalResult[0]);

      // Trouver le pseudo du vainqueur
      const winnerPlayer = players.find(player => player.id === winner.id_player);

      // Créer la liste des joueurs
      const allPlayers = players.map(player => `${player.pseudo} (ID: ${player.id})`).join(", ");

      // Afficher les données dans les éléments HTML
      vainqueurElem.textContent = winnerPlayer.pseudo;
      scoreFinalElem.textContent = winner.score;
      joueursElem.textContent = allPlayers;
    })
    .catch(error => {
      console.error('Erreur lors de l\'affichage des résultats:', error);
    });
}

// Fonction pour charger les résultats lorsque le bouton est cliqué
loadResultsBtn.addEventListener("click", () => {
  const gameId = gameIdInput.value.trim(); // Récupère l'ID entré par l'utilisateur
  if (gameId) {
    displayResults(gameId);  // Appelle la fonction pour afficher les résultats
  } else {
    alert("Veuillez entrer un ID de partie valide.");
  }
});
