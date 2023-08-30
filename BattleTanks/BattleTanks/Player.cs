using System.Diagnostics;
using System.Windows.Forms;

namespace BattleTanks
{
    public class Player
    {
        public int Ind { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int curHealth { get; set; }
        public int maxHealth { get; set; } = 3;
        public int Speed { get; set; }

        public Keys Up { get; set; }
        public Keys Down { get; set; }
        public Keys Left { get; set; }
        public Keys Right { get; set; }
        public Keys Fire { get; set; }

        public bool MoveUp { get; set; } = false;
        public bool MoveDown { get; set; } = false;
        public bool MoveLeft { get; set; } = false;
        public bool MoveRight { get; set; } = false;
        public bool OpenFire { get; set; } = false;

        public Stopwatch watchFire { get; set; } = new Stopwatch();
        public int BreakWatchFire { get; set; } = 1;
        public bool BreakFire { get; set; } = false;

        public Stopwatch watchShield { get; set; } = new Stopwatch();
        public int BreakWatchShield { get; set; } = 5;
        public bool BreakShield { get; set; } = true;

        public Stopwatch watchSpeed { get; set; } = new Stopwatch();
        public int BreakWatchSpeed { get; set; } = 5;
        public bool BreakSpeed { get; set; } = true;
        public double SpeedUp { get; set; } = 1.25;

        public bool Blocks { get; set; } = false;
        public bool BigShot { get; set; } = false;

        public int BulletSpeed { get; set; }
        public int Rotation { get; set; } = 1;

        public Player(int ind, Keys up, Keys down, Keys left, Keys right, Keys fire, int width, int height, int speed)
        {
            Ind = ind;

            curHealth = maxHealth;

            Up = up;
            Down = down;
            Right = right;
            Left = left;
            Fire = fire;

            Width = width;
            Height = height;
            Speed = speed;

            BulletSpeed = Speed * 2;
        }
    }
}
