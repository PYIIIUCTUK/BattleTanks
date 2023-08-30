namespace BattleTanks
{
    public class Block : Cell
    {
        public int Health { get; set; }
        public Block(int x, int y, int type = 1, int health = 3) : base(x, y, type)
        {
            Health = health;
        }
    }
}
