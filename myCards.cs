using P2DEngine.Managers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P2DEngine.GameObjects
{
    public class myCards
    {
        public Image CardImage { get; set; } //Variable para manejar las cartas
        public int Value { get; set; } //Asigna un valor numerico a una carta (1 para As, 10 para J/Q/K)
        public string Suit { get; set; } //para debug
        public string Name { get; set; } // para debug
    }
}
