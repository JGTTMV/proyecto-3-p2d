﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P2DEngine
{
    public class Agario : myGame
    {
        Player player;
        Enemy enemy;

        myObjective Food;

        Random rand = new Random();

        float playerSpeed = 0.6f;
        float baseStep = 30;
        float enemyStep = 30;
        float playerCenterX;
        float playerCenterY;

        float sizeFactor;
        float targetZoom;

        //Agrega un interruptor para el modo de depuracion
        private bool debugMode = true;
        private float debugTimer = 0.0f; // Temporizador para rastrear el tiempo transcurrido para mensajes de depuracion
        private const float debugInterval = 0.5f; // Intervalo en segundos para mensajes de depuracion

        private float playerDistance = 0.0f;

        int pistaX = 100;
        int pistaY = 100;
        int pistaAncho = 900;
        int pistaAlto = 900;
        int grosorBorde = 20;
        int randomX;
        int randomY;
        int foodCount;

        //Agrega variables para las condiciones de fin del juego
        private const int maxScore = 25; //Puntuacion maxima para terminar el juego
        private bool isGameOver = false; //Indicador para verificar si el juego ha terminado

        public Agario(int width, int height, int FPS, myCamera c) : base(width, height, FPS, c)
        {
            //Configura la cámara para que inicialmente siga al jugador 1
            currentCamera = new myCamera(0, 0, 800, 600, 1.0f);
            currentCamera.SetTargetPlayer(player); //La cámara sigue al jugador 1 por defecto

            //Crea bordes y pista (sin cambios)
            Color colorBorde = Color.DarkGray;

            player = new Player(pistaX + pistaAncho / 2, pistaY + pistaAlto / 2, 20, 20, Color.Red); // Jugador

            int enemyX = rand.Next(pistaX + grosorBorde, pistaX + pistaAncho - grosorBorde);
            int enemyY = rand.Next(pistaX + grosorBorde, pistaX + pistaAncho - grosorBorde);

            enemy = new Enemy(enemyX, enemyY, 20, 20, Color.Blue);

            //Bordes exteriores
            Instantiate(new myBlock(pistaX, pistaY, pistaAncho, grosorBorde, colorBorde));
            Instantiate(new myBlock(pistaX, pistaY + pistaAlto - grosorBorde, pistaAncho, grosorBorde, colorBorde));
            Instantiate(new myBlock(pistaX, pistaY, grosorBorde, pistaAlto, colorBorde));
            Instantiate(new myBlock(pistaX + pistaAncho - grosorBorde, pistaY, grosorBorde, pistaAlto, colorBorde));

            foodCount = 50;

            //La comida
            for (int i = 0; i < foodCount; i++)
            {
                //Cada comida se genera dentro de los limites de la pistaww
                randomX = rand.Next(pistaX + grosorBorde, pistaX + pistaAncho - grosorBorde);
                randomY = rand.Next(pistaY + grosorBorde, pistaY + pistaAlto - grosorBorde);

                Food = new myObjective(randomX, randomY, 10, 10, Color.Yellow, true);
                Instantiate(Food);
            }

            Instantiate(player);
            Instantiate(enemy);
        }
        private bool IsColliding(myGameObject a, myGameObject b)
        {
            //Rehace la deteccion de colisiones para garantizar calculos precisos de las cajas de impacto
            float aLeft = a.x;
            float aRight = a.x + a.sizeX;
            float aTop = a.y;
            float aBottom = a.y + a.sizeY;

            float bLeft = b.x;
            float bRight = b.x + b.sizeX;
            float bTop = b.y;
            float bBottom = b.y + b.sizeY;

            //Verifica superposicion entre los dos rectangulos
            return (aRight > bLeft && aLeft < bRight && aBottom > bTop && aTop < bBottom);
        }
        private bool WillCollide(Player player, int deltaX, int deltaY)
        {
            //Predice la nueva posicion del jugador
            float newX = player.x + deltaX;
            float newY = player.y + deltaY;

            //Crea un rectangulo temporal para la nueva posicion del jugador
            Rectangle playerRect = new Rectangle((int)newX, (int)newY, (int)player.sizeX, (int)player.sizeY);

            //Verifica colisiones con todos los objetos myBlock
            foreach (var obj in gameObjects)
            {
                if (obj is myBlock block && !(block is Player) && !(block is myObstacle) && !(block is myObjective))
                {
                    Rectangle blockRect = new Rectangle((int)block.x, (int)block.y, (int)block.sizeX, (int)block.sizeY);
                    if (playerRect.IntersectsWith(blockRect))
                    {
                        return true; //Colision detectada
                    }
                }
            }

            return false; //Sin colision
        }

        protected override void ProcessInput()
        {
            if (isGameOver)
                return; //Deshabilita los inputs al terminar el juego

            //Movimiento de player
            //Revisa las colisiones antes de actualizar al jugador
            if (myInputManager.IsKeyPressed(Keys.W) && !WillCollide(player, 0, -(int)(baseStep * player.speed)))
            {
                player.y -= (int)(baseStep * player.speed);
            }
            if (myInputManager.IsKeyPressed(Keys.A) && !WillCollide(player, -(int)(baseStep * player.speed), 0))
            {
                player.x -= (int)(baseStep * player.speed);
            }
            if (myInputManager.IsKeyPressed(Keys.S) && !WillCollide(player, 0, (int)(baseStep * player.speed)))
            {
                player.y += (int)(baseStep * player.speed);
            }
            if (myInputManager.IsKeyPressed(Keys.D) && !WillCollide(player, (int)(baseStep * player.speed), 0))
            {
                player.x += (int)(baseStep * player.speed);
            }

            //Controles alternativos
            if (myInputManager.IsKeyPressed(Keys.Up) && !WillCollide(player, 0, -(int)(baseStep * player.speed)))
            {
                player.y -= (int)(baseStep * player.speed);
            }
            if (myInputManager.IsKeyPressed(Keys.Left) && !WillCollide(player, -(int)(baseStep * player.speed), 0))
            {
                player.x -= (int)(baseStep * player.speed);
            }
            if (myInputManager.IsKeyPressed(Keys.Down) && !WillCollide(player, 0, (int)(baseStep * player.speed)))
            {
                player.y += (int)(baseStep * player.speed);
            }
            if (myInputManager.IsKeyPressed(Keys.Right) && !WillCollide(player, (int)(baseStep * player.speed), 0))
            {
                player.x += (int)(baseStep * player.speed);
            }
        }

        void PlayerGrowth()
        {
            // Incrementa el tamaño del jugador
            player.sizeX += 2;
            player.sizeY += 2;
            baseStep -= 0.1f; // Disminuye su velocidad mientras más crece

            if (baseStep < 1) baseStep = 1;

            // No ajustes el zoom directamente aquí, déjalo para el Update
        }

        protected override void RenderGame(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, windowWidth, windowHeight);
            //Añade las variables de mainCamera a la logica de dibujado. Estamos
            //Dibuja con respecto a la camara.

            foreach (var gameObject in gameObjects)
            {
                gameObject.Draw(g,
                    currentCamera.GetViewPosition(gameObject.x, gameObject.y),
                    currentCamera.GetViewSize(gameObject.sizeX, gameObject.sizeY));
            }
        }

        //Funcion de la clase MathHelper, aunque instanciada como funcion para ahorrar tiempo
        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        protected override void Update()
        {
            if (isGameOver)
                return;

            debugTimer += deltaTime; //incrementa el timer

            // Calcular zoom basado en el tamaño del jugador (mínimo 0.5, máximo 1.5)
            sizeFactor = (player.sizeX + player.sizeY) / 40.0f; // 40 = 20+20 (tamaño inicial)
            targetZoom = Math.Max(0.5f, Math.Min(1.5f, 1.0f / sizeFactor));

            // Suavizar el cambio de zoom
            currentCamera.zoom = Lerp(currentCamera.zoom, targetZoom, 0.05f);

            // Centrar la cámara considerando el centro del jugador (no su esquina)
            playerCenterX = player.x + player.sizeX / 2;
            playerCenterY = player.y + player.sizeY / 2;

            currentCamera.x = playerCenterX - (windowWidth / (2 * currentCamera.zoom));
            currentCamera.y = playerCenterY - (windowHeight / (2 * currentCamera.zoom));

            // Lista temporal para almacenar las comidas a eliminar
            List<myObjective> foodToRemove = new List<myObjective>();

            // Buscar todas las instancias de myObjective en gameObjects
            foreach (var gameObject in gameObjects)
            {
                if (gameObject is myObjective food)
                {
                    if (IsColliding(player, food))
                    {
                        // Aume5ntar tamaño del jugador
                        PlayerGrowth();

                        // Agregar a la lista de comidas a eliminar
                        foodToRemove.Add(food);
                    }
                }
            }

            // Eliminar las comidas colisionadas y generar nuevas
            foreach (var food in foodToRemove)
            {
                Destroy(food);

                // Generar nueva comida en posición aleatoria
                do
                {
                    randomX = rand.Next(pistaX + grosorBorde, pistaX + pistaAncho - grosorBorde);
                    randomY = rand.Next(pistaY + grosorBorde, pistaY + pistaAlto - grosorBorde);

                }while(randomX != (player.x + player.sizeX) && randomY != (player.y + player.sizeY));

                Food = new myObjective(randomX, randomY, 10, 10, Color.Yellow, true);
                Instantiate(Food);
            }

            if (debugTimer >= debugInterval && debugMode)
            {
                Console.Clear(); //Limpia la consola

                Console.WriteLine($"Tamaño del jugador: {player.sizeX}");
                Console.WriteLine($"Velocidad del jugador: {baseStep}");

                debugTimer = 0.0f; //Resetea el timer del debug
            }
        }
    }
}
