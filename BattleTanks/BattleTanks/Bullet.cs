namespace BattleTanks
{
    public class Bullet
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int Rotation { get; set; }

        public int MoveX { get; set; }
        public int MoveY { get; set; }

        public int Damage { get; set; } = 1;
        public bool Protect { get; set; }

        public Bullet(Player player, int S)
        {
            Rotation = player.Rotation;
            if (player.BigShot) { Protect = true; player.BigShot = false; }
            else { Protect = false; }

            switch (Rotation)
            {
                case 1:
                {
                    Width = (int)(S * 0.2);
                    Height = (int)(S * 0.4);
                    X = player.X + player.Width / 2 - Width / 2;
                    Y = player.Y - Height;

                    MoveX = 0;
                    MoveY = -player.BulletSpeed;
                    break;
                }
                case 2:
                {
                    Width = (int)(S * 0.4);
                    Height = (int)(S * 0.2);
                    X = player.X + player.Width;
                    Y = player.Y + player.Height / 2 - Height / 2;

                    MoveX = player.BulletSpeed;
                    MoveY = 0;
                    break;
                }
                case 3:
                {
                    Width = (int)(S * 0.2);
                    Height = (int)(S * 0.4);
                    X = player.X + player.Width / 2 - Width / 2;
                    Y = player.Y + player.Height;

                    MoveX = 0;
                    MoveY = player.BulletSpeed;
                    break;
                }
                case 4:
                {
                    Width = (int)(S * 0.4);
                    Height = (int)(S * 0.2);
                    X = player.X - Width;
                    Y = player.Y + player.Height / 2 - Height / 2;

                    MoveX = -player.BulletSpeed;
                    MoveY = 0;
                    break;
                }
            }
        }

        public void Move()
        {
            X += MoveX;
            Y += MoveY;
        }
    }
}
