namespace BattleTanks
{
    public class Equipment : Cell
    {
        public int Equip { get; set; }

        public new int X { get; set; }
        public new int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Equipment(int equip, int x, int y, int width, int height, int type = 2) : base(x, y, type)
        {
            Equip = equip;

            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public Equipment(Equipment equip) : base ((equip as Cell))
        {
            Equip = equip.Equip;
            X = equip.X;
            Y = equip.Y;
            Width = equip.Width;
            Height = equip.Height;
        }
    }
}
