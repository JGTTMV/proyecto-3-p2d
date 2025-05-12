using P2DEngine.Games;
using P2DEngine.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P2DEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Ancho y alto de la ventana.
            int windowWidth = 800;
            int windowHeight = 600;

            // Ancho y alto de la cámara.
            int camWidth = 800;
            int camHeight = 600;

            // Frames por segundo.
            int FPS = 60;

            myImageManager.Load("backcard-black.png", "backcard-black"); //cada imagen mide 55x77

            for(int i = 1; i < 14; i++)
            {
                myImageManager.Load($"corazon-{i}.png", $"corazon-{i}");
            }
            for (int i = 1; i < 14; i++)
            {
                myImageManager.Load($"diamante-{i}.png", $"diamante-{i}");
            }
            for (int i = 1; i < 14; i++)
            {
                myImageManager.Load($"picas-{i}.png", $"picas-{i}");
            }
            for (int i = 1; i < 14; i++)
            {
                myImageManager.Load($"trebol-{i}.png", $"trebol-{i}");
            }

            myImageManager.Load("00Fondo.jpg", "Fondo"); //Fondo creado por mi usando ibis paint X

            //Musica y efectos de sonido del juego Killer7
            myFontManager.Load("CourierPrime-Regular.ttf", "CourierPrime-Regular");

            myAudioManager.Load("Russian Roulette Theme.mp3", "Russian Roulette Theme");

            myAudioManager.Load("Blood Machine - Beep.wav", "Blood Machine");

            myAudioManager.Load("Bell.wav", "Bell");

            myAudioManager.Load("Click.wav", "Click");

            myAudioManager.Load("Puzzle Failed.wav", "Puzzle Failed");

            myAudioManager.Load("Puzzle Solved.wav", "Puzzle Solved");

            myAudioManager.Load("Point Score.wav", "Point Score");

            BlackJack blackJack = new BlackJack(windowWidth, windowHeight, FPS, new myCamera(0, 0, camWidth, camHeight, 
                (float)windowWidth/(float)camWidth));

            blackJack.Start();
            
            // Esto es propio de WinForms, es básicamente para que la ventana fluya.
            Application.Run();
        }
    }
}
