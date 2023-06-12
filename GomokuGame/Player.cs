using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GomokuGame
{
    public class Player
    {
        private string name;    

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private Image mark;

        public Image Mark
        {
            get { return mark; }
            set { mark = value; }
        }

        public Player(string name, Image mark)
        {
            this.Name = name;
            this.Mark = mark;
        }
    }
}
