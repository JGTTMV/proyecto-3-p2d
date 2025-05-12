using P2DEngine;
using P2DEngine.GameObjects;
using P2DEngine.Managers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P2DEngine.Games
{
    public class BlackJack : myGame //Juan José Valdebenito
    {
        private int fichasPlayer = 1000;
        private int currentBet; //Variable usada para manejar la apuesta con inputs
        private int totalBet; //Almacena el total de la apuesta del jugador
        private bool turnValid = false; //Verifica si el jugador puede sacar una carta
        private bool betsPlaced = false; //Verifica si las apuestas ya se realizaron
        private bool isPlayerTurn = true; // True = turno del jugador, False = turno del crupier
        int playerHandTotal;
        int enemyHandTotal;

        private int playerX = 350;
        private int playerY = 400;

        private int enemyX = 350;
        private int enemyY = 200;

        Random rand = new Random();
        private List<Image> availableCards; //Cartas aun en el mazo
        private List<myCards> discardedCards = new List<myCards>(); //Cartas ya usadas

        private List<myCards> deck = new List<myCards>(); //Lista para almacenar el mazo
        private List<myCards> playerHand = new List<myCards>();
        private List<myCards> enemyHand = new List<myCards>();
        private bool isEnemyCardHidden = true; //Oculta la primera carta del crupier
        private float drawBuffer = 0; //Genera un buffer para no sacar varias cartas en un solo input
        private float endTurnBuffer = 0; //Genera un buffer al terminar un turno
        private bool isWaitingForNextRound = false; //Controla el estado de espera
        private bool isGameOver = false; //Verifica si el juego ha terminado
        private string whoWon; //Dice quien gano al final de una ronda

        private string fontId = "CourierPrime-Regular";
        private float volume = 0.4f;
        private Image defaultCard; //Carta dada vuelta
        private Image Background; //Fondo del juego
        private Font f;

        public BlackJack(int width, int height, int FPS, myCamera c) : base(width, height, FPS, c)
        {
            defaultCard = myImageManager.Get("backcard-black"); //Define la carta dada vuelta como la default 

            string[] suits = { "corazon", "diamante", "picas", "trebol" }; //Define los tipos de cartas
            foreach (string suit in suits) //Al añadirlos al ciclo, les asigna un valor
            {
                for (int number = 1; number <= 13; number++)
                {
                    string cardName = $"{suit}-{number}";
                    Image cardImage = myImageManager.Get(cardName);
                    int value = (number >= 10) ? 10 : number; // Algunas cartas son asignadas valores distintos (As = 1, J/Q/K = 10)
                    deck.Add(new myCards { CardImage = cardImage, Value = value });
                }
            }

            f = myFontManager.Get(fontId, 12); //Obtenie la fuente
            Background = myImageManager.Get("Fondo");
            myAudioManager.Play("Russian Roulette Theme", volume); //Ejecuta la cancion de fondo

            //Revuelve el mazo y empieza el juego
            ShuffleDeck();
            StartRound();
        }

        private void StartRound()
        {
            //Verifica variables
            isPlayerTurn = true;
            turnValid = false;
            betsPlaced = false;
            currentBet = 0;

            //Limpiar manos anteriores
            playerHand.Clear();
            enemyHand.Clear();

            //Reparte 2 cartas al jugador
            playerHand.Add(DrawRandomCard());
            playerHand.Add(DrawRandomCard());

            //Reparte 2 cartas al crupier (una oculta)
            enemyHand.Add(DrawRandomCard());
            enemyHand.Add(DrawRandomCard());
            isEnemyCardHidden = true;

            //Verifica si ocurre un Blackjack inicial
            if (CalculateHandValue(playerHand) == 21)
            {
                isPlayerTurn = false; //Termina el turno del jugador
                EndRound(); //Termina la ronda
            }
        }

        private void EndRound()
        {
            isEnemyCardHidden = false; //Mostrar la carta oculta del crupier
            playerHandTotal = CalculateHandValue(playerHand); //Calcula valores
            enemyHandTotal = CalculateHandValue(enemyHand);
            turnValid = false;

            //Descarta las cartas usadas
            discardedCards.AddRange(playerHand);
            discardedCards.AddRange(enemyHand);

            //Todas las variaciones de quien gana
            if (playerHandTotal > 21 && enemyHandTotal > 21)
            {
                whoWon = "Ningun participante";
                myAudioManager.Play("Bell", volume);
                fichasPlayer += totalBet;
            }
            else if (playerHandTotal > 21)
            {
                whoWon = "El Crupier";
                myAudioManager.Play("Puzzle Failed");
                fichasPlayer -= totalBet;

            }
            else if (enemyHandTotal > 21)
            {
                whoWon = "El Jugador";
                myAudioManager.Play("Puzzle Solved");
                fichasPlayer += (totalBet * 2);
            }
            else if (playerHandTotal > enemyHandTotal)
            {
                whoWon = "El Jugador";
                myAudioManager.Play("Puzzle Solved");
                fichasPlayer += (totalBet * 2);
            }
            else
            {
                whoWon = "El Crupier";
                myAudioManager.Play("Puzzle Failed");
                fichasPlayer -= totalBet;
            }

            //En caso de que el jugador pierda o gane demasiado (como en un casino real)
            if (fichasPlayer <= 0)
            {
                isGameOver = true;

                MessageBox.Show("El Jugador fue echado del casino por quedar sin fichas :((");

                Application.Exit();
            }
            else if (fichasPlayer > 9999)
            {
                isGameOver = true;

                MessageBox.Show("El Jugador fue echado del casino por ganar demasiado:((");

                Application.Exit();
            }

            isWaitingForNextRound = true; //Activa el buffer de espera
            endTurnBuffer = 2; //2 segundos de espera
            totalBet = 0; //Se reinicia el valor de la apuesta total
        }

        private int CalculateHandValue(List<myCards> hand)
        {
            int total = 0;

            foreach (var card in hand)
            {
                total += card.Value; //Accede a la propiedad Value de myCards
            }

            return total;
        }

        private myCards DrawRandomCard()
        {
            //Si quedan menos de la mitad de las cartas, revuelve las descartadas
            if (deck.Count < 26 && discardedCards.Count > 0)
            {
                ShuffleDiscardedCards();
            }

            if (deck.Count == 0)
            {
                throw new Exception("No hay cartas en el mazo.");
            }

            int randomIndex = rand.Next(0, deck.Count);
            myCards drawnCard = deck[randomIndex];
            deck.RemoveAt(randomIndex);
            return drawnCard;
        }
        private void ShuffleDiscardedCards()
        {
            //Agrega las cartas descartadas al mazo
            deck.AddRange(discardedCards);
            discardedCards.Clear();

            //Revuelve el mazo
            ShuffleDeck();
        }

        private void ShuffleDeck()
        {
            //Usa el algoritmo Fisher-Yates para revolver
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = rand.Next(0, i + 1);
                myCards temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }

        protected override void ProcessInput()
        {
            //Se verifica que la apuesta aun no se decide
            if (myInputManager.IsKeyPressed(Keys.Up) && !betsPlaced)
            {
                if (currentBet >= fichasPlayer) { currentBet = fichasPlayer; }
                else { currentBet++; }
            }
            if (myInputManager.IsKeyPressed(Keys.Down) && !betsPlaced)
            {
                if (currentBet <= 0) { currentBet = 0; }
                else { currentBet--; }
            }
            if (myInputManager.IsKeyPressed(Keys.Right) && !betsPlaced)
            {
                if (currentBet >= fichasPlayer) { currentBet = fichasPlayer; }
                else { currentBet += 10; }
            }
            if (myInputManager.IsKeyPressed(Keys.Left) && !betsPlaced)
            {
                if (currentBet <= 0) { currentBet = 0; }
                else { currentBet -= 10; }
            }
            if (myInputManager.IsKeyPressed(Keys.Space) && !betsPlaced)
            {
                if (currentBet <= 0) { currentBet = 0; }
                else if (currentBet >= fichasPlayer)
                {
                    fichasPlayer = 0;
                }
                else
                {
                    fichasPlayer -= currentBet;
                }

                myAudioManager.Play("Point Score", volume); //Reproduce un efecto de sonido

                totalBet += currentBet;

                //Se confirma que se realizaron las apuestas, y se activa el turno con cartas
                betsPlaced = true;
                turnValid = true;
            }

            if (myInputManager.IsKeyPressed(Keys.Enter) && drawBuffer == 0 && turnValid)
            {
                drawBuffer = 0.5f; //Se Reactiva el buffer

                myAudioManager.Play("Click", volume);

                //El crupier tambien recibe una carta
                playerHand.Add(DrawRandomCard());
                enemyHand.Add(DrawRandomCard());

                //Si el jugador se pasa de 21, termina la ronda
                if (CalculateHandValue(playerHand) >= 21)
                {
                    isPlayerTurn = false; //Fuerza turno del crupier
                    EndRound(); //Termina la ronda
                }
            }
            else if (myInputManager.IsKeyPressed(Keys.R) && isPlayerTurn && turnValid) //Utiliza el turno para plantarse
            {
                myAudioManager.Play("Blood Machine", volume);
                isPlayerTurn = false;
                EndRound();
            }
        }

        protected override void RenderGame(Graphics g)
        {
            bool enemyReveal = false;

            g.DrawImage(Background, 0, 0);

            //Dibuja las cartas del crupier
            if (enemyHand.Count > 0)
            {
                //Primera carta (oculta)
                if (isEnemyCardHidden)
                {
                    g.DrawImage(defaultCard, enemyX, enemyY);
                }
                else
                {
                    g.DrawImage(enemyHand[0].CardImage, enemyX, enemyY);
                    enemyReveal = true;
                }

                //Resto de cartas (visibles)
                for (int i = 1; i < enemyHand.Count; i++)
                {
                    g.DrawImage(enemyHand[i].CardImage, enemyX + (i * 30), enemyY + (i * 15));
                }
            }

            //Dibuja las cartas del jugador
            for (int i = 0; i < playerHand.Count; i++)
            {
                g.DrawImage(playerHand[i].CardImage, playerX + (i * 30), playerY + (i * 15)); //Offset horizontal
            }

            //Configura el formato del texto
            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near
            };

            //Color del texto
            Brush textBrush = Brushes.White;

            //Fuente para las fichas
            Font fichasFont = f;

            //Texto a mostrar
            string playerText = "Jugador";
            string enemyText = "Crupier";
            string fichasText = $"Fichas: ${fichasPlayer}";
            string totalText = $"Apuesta: ${currentBet}";
            string playerScoreText = $"Puntuación: {CalculateHandValue(playerHand)}"; //Puntaje del jugador
            string enemyScoreText = $"Puntuación: {CalculateHandValue(enemyHand)}"; //Puntaje del crupier
            string waitingText = $"PREPARANDO NUEVA RONDA... {endTurnBuffer.ToString("0.0")}s";
            string winnerText = $"{whoWon} gano"; //Indica quien gano dependiendo de la variable whoWon

            //Posiciones del texto
            PointF playerTextPos = new PointF(playerX, playerY - 20); //Encima de las cartas del jugador
            PointF puntuacionTextPos = new PointF(playerX + 190, playerY - 10); //A la izquierda del jugador
            PointF fichasTextPos = new PointF(playerX + 190, playerY + 10); //Por debajo de la puntuacion
            PointF totalTextPos = new PointF(playerX + 190, playerY + 30); //Por debajo de las fichas
            PointF enemyTextPos = new PointF(enemyX, enemyY - 20); //Encima de las cartas del enemigo
            PointF crupierTextPos = new PointF(enemyX + 190, enemyY - 10); //A la izquierda del jugador
            PointF waitingTextPos = new PointF(windowWidth / 2 - 100, windowHeight - 25); //En la parte inferior de la pantalla
            PointF winnerTextPos = new PointF(windowWidth / 4 - 100, windowHeight / 2); //A la derecha de la pantalla

            // Dibuja el texto principal
            g.DrawString(playerText, fichasFont, textBrush, playerTextPos, format);
            g.DrawString(playerScoreText, fichasFont, textBrush, puntuacionTextPos, format);
            g.DrawString(fichasText, fichasFont, textBrush, fichasTextPos, format);
            g.DrawString(totalText, fichasFont, textBrush, totalTextPos, format);
            g.DrawString(enemyText, fichasFont, textBrush, enemyTextPos, format);
            if (enemyReveal) { g.DrawString(enemyScoreText, fichasFont, textBrush, crupierTextPos, format); }
            if (isWaitingForNextRound)
            {
                g.DrawString(waitingText, fichasFont, textBrush, waitingTextPos, format);
                g.DrawString(winnerText, fichasFont, textBrush, winnerTextPos, format);
            }
        }

        protected override void Update()
        { 
            //Evita que el buffer quede en los negativos
            if(drawBuffer > 0) drawBuffer -= deltaTime;
            else if (drawBuffer < 0) drawBuffer = 0;

            //Buffer entre rondas
            if (isWaitingForNextRound)
            {
                endTurnBuffer -= deltaTime;
                if (endTurnBuffer <= 0)
                {
                    isWaitingForNextRound = false;
                    StartRound(); //Inicia nueva ronda
                }
            }
        }
    }
}
