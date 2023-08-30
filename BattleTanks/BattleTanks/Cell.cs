namespace BattleTanks
{
    public class Cell
    {
        public int Type { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public Cell(int x, int y, int type = 0)
        {
            Type = type;
            X = x;
            Y = y;
        }
        public Cell(Cell cell)
        {
            Type = cell.Type;
            X = cell.X;
            Y = cell.Y;
        }
    }
}
