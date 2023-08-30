using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace BattleTanks
{
    public partial class Game : Form
    {
        Menu menu;
        string path;
        Timer timer;

        Stopwatch watchEquip;
        Stopwatch watchMapCut;
        TimeSpan elapsedTime;

        Dictionary<string, int> NamesCell;
        Dictionary<string, int> NamesEquip;
        List<Bitmap> bitmapTanks;
        List<Bitmap> bitmapEquips;

        int S, Ws, We, Hs, He, W, H;
        List<List<Cell>> map;

        List<Cell> freeCells;
        Random rand;

        List<Player> players;
        List<Bullet> bullets;

        public Game(Menu myMenu, string Path)
        {
            InitializeComponent();

            menu = myMenu;
            path = Path;

            NamesCell = new Dictionary<string, int>();
            map = new List<List<Cell>>();
            timer = new Timer();
            players = new List<Player>();
            bullets = new List<Bullet>();
            bitmapTanks = new List<Bitmap>();
            watchEquip = new Stopwatch();
            freeCells = new List<Cell>();
            rand = new Random();
            NamesEquip = new Dictionary<string, int>();
            watchMapCut = new Stopwatch();
            bitmapEquips = new List<Bitmap>();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            timer.Interval = 20;
            timer.Tick += Timer_Tick;
            timer.Start();
            watchEquip.Start();
            watchMapCut.Start();

            NamesCell.Add("dead", -1);
            NamesCell.Add("space", 0);
            NamesCell.Add("wall", 1);
            NamesCell.Add("equipment", 2);

            NamesEquip.Add("health", 0);
            bitmapEquips.Add(new Bitmap("Sprites/Health.png"));
            NamesEquip.Add("shot", 1);
            bitmapEquips.Add(new Bitmap("Sprites/Shot.png"));
            NamesEquip.Add("shield", 2);
            bitmapEquips.Add(new Bitmap("Sprites/Shield.png"));
            NamesEquip.Add("blocks", 3);
            bitmapEquips.Add(new Bitmap("Sprites/Blocks.png"));

            bitmapTanks.Add(new Bitmap("Sprites/Tank1.png"));
            bitmapTanks.Add(new Bitmap("Sprites/Tank2.png"));
            bitmapTanks.Add(new Bitmap("Sprites/Bullet-v.png"));
            bitmapTanks.Add(new Bitmap("Sprites/Bullet-h.png"));

            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;

            string[] lines = File.ReadAllLines(path);
            List<string> str = new List<string>();

            List<List<string>> data = new List<List<string>>();
            List<Cell> line;

            for (int i = 0; i < lines.Length; i++)
            {
                str.AddRange(lines[i].Split(' '));
                data.Add(new List<string>(str));
                str.Clear();
            }

            Hs = 0;
            He = data.Count;
            We = data[0].Count;
            Ws = 0;

            W = We;
            H = He;

            S = (ClientSize.Height - 20) / (He);
            WindowState = FormWindowState.Normal;
            Size = new Size(S * We + 16 + S * 4, S * He + 39);
            Location = new Point((Screen.PrimaryScreen.Bounds.Width - Size.Width) / 2, 0);

            for (int i = Hs; i < He; i++)
            {
                line = new List<Cell>();
                for (int j = Ws; j < We; j++)
                {
                    if (data[i][j] == "-") { line.Add(new Cell(j * S, i * S)); }
                    else if (data[i][j] == "w") { line.Add(new Block(j * S, i * S)); }
                }
                map.Add(line);
            }

            players.Add(new Player(0, Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.ControlKey,
                (int)(S * 0.6), (int)(S * 0.6), S / 10));
            players.Add(new Player(1, Keys.W, Keys.S, Keys.A, Keys.D, Keys.Space,
                (int)(S * 0.6), (int)(S * 0.6), S / 10));

            for(int i = 0; i < players.Count; i++)
            {
                PlayerRespawn(players[i]);
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            menu.Show();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            for(int i = Hs; i < He; i++)
            {
                for (int j = Ws; j < We; j++)
                {
                    e.Graphics.FillRectangle(Brushes.Gray, new Rectangle(j * S, i * S, S, S));
                    if (map[i][j].Type == NamesCell["wall"])
                    {
                        e.Graphics.FillRectangle(Brushes.Black, new Rectangle(j * S, i * S, S, S));
                    }
                    else if (map[i][j].Type == NamesCell["equipment"])
                    {
                        Equipment equip = (map[i][j] as Equipment);
                        int ind = equip.Equip;
                        e.Graphics.DrawImage(bitmapEquips[ind], equip.X, equip.Y,
                        equip.Width, equip.Height);
                    }
                    e.Graphics.DrawRectangle(Pens.Black, new Rectangle(j * S, i * S, S, S));
                }
            }

            for (int i = 0; i < bullets.Count; i++)
            {
                int rotation = 0;
                int ind = 2;
                if (bullets[i].Rotation > 2) { rotation = 1; }
                if (bullets[i].Rotation % 2 == 0) { ind = 3; }

                e.Graphics.TranslateTransform(bullets[i].X + bullets[i].Width / 2,
                    bullets[i].Y + bullets[i].Height / 2);
                e.Graphics.RotateTransform(180 * rotation);
                e.Graphics.TranslateTransform(-(bullets[i].X + bullets[i].Width / 2),
                    -(bullets[i].Y + bullets[i].Height / 2));

                e.Graphics.DrawImage(bitmapTanks[ind], bullets[i].X, bullets[i].Y,
                    bullets[i].Width, bullets[i].Height);

                e.Graphics.ResetTransform();
            }

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Ind == 0)
                {
                    e.Graphics.TranslateTransform(players[i].X + players[i].Width / 2,
                    players[i].Y + players[i].Height / 2);
                    e.Graphics.RotateTransform(90 * (players[i].Rotation - 1));
                    e.Graphics.TranslateTransform(-(players[i].X + players[i].Width / 2),
                        -(players[i].Y + players[i].Height / 2));

                    e.Graphics.DrawImage(bitmapTanks[0], players[i].X, players[i].Y,
                        players[i].Width, players[i].Height);
                    e.Graphics.ResetTransform();

                    if (!players[i].BreakShield)
                    {
                        Pen pen = new Pen(Brushes.Orange, 3);
                        e.Graphics.DrawRectangle(pen, players[i].X, players[i].Y,
                       players[i].Width, players[i].Height);
                    }

                    e.Graphics.DrawString("Первый игрок", new Font("Times New Roman", 20, FontStyle.Bold),
                    Brushes.Black, new PointF(W * S + S / 2, S / 2));
                    for (int k = 0; k < players[i].curHealth; k++)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, new Rectangle(W * S + S / 2 + S * k, S, S, S));
                        e.Graphics.DrawRectangle(Pens.Black, new Rectangle(W * S + S / 2 + S * k, S, S, S));
                    }
                    if (players[i].BigShot)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, new Rectangle(W * S + S / 2, S * 2 + S / 4, S * 3, S / 2));
                    }
                    if (players[i].Blocks)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, new Rectangle(W * S + S / 2 + S, S * 3, S, S));
                    }
                }
                else {
                    e.Graphics.TranslateTransform(players[i].X + players[i].Width / 2,
                    players[i].Y + players[i].Height / 2);
                    e.Graphics.RotateTransform(90 * (players[i].Rotation - 1));
                    e.Graphics.TranslateTransform(-(players[i].X + players[i].Width / 2),
                        -(players[i].Y + players[i].Height / 2));

                    e.Graphics.DrawImage(bitmapTanks[1], players[i].X, players[i].Y,
                        players[i].Width, players[i].Height);
                    e.Graphics.ResetTransform();

                    if (!players[i].BreakShield)
                    {
                        Pen pen = new Pen(Brushes.Orange, 3);
                        e.Graphics.DrawRectangle(pen, players[i].X, players[i].Y,
                       players[i].Width, players[i].Height);
                    }

                    e.Graphics.DrawString("Второй игрок", new Font("Times New Roman", 20, FontStyle.Bold),
                    Brushes.Black, new PointF(W * S + S / 2, (H - 4) * S - S / 2));
                    for (int k = 0; k < players[i].curHealth; k++)
                    {
                        e.Graphics.FillRectangle(Brushes.Red, new Rectangle(W * S + S / 2 + S * k, (H - 4) * S, S, S));
                        e.Graphics.DrawRectangle(Pens.Black, new Rectangle(W * S + S / 2 + S * k, (H - 4) * S, S, S));
                    }

                    if (players[i].BigShot)
                    {
                        e.Graphics.FillRectangle(Brushes.Red, new Rectangle(W * S + S / 2, (H - 2) * S - (S - S / 4), S * 3, S / 2));
                    }
                    if (players[i].Blocks)
                    {
                        e.Graphics.FillRectangle(Brushes.Red, new Rectangle(W * S + S / 2 + S, (H - 2) * S, S, S));
                    }
                }
            }
        }

        private void Game_KeyUp(object sender, KeyEventArgs e)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (e.KeyCode == players[i].Up)
                {
                    players[i].MoveUp = false;
                }
                else if (e.KeyCode == players[i].Down)
                {
                    players[i].MoveDown = false;
                }
                else if (e.KeyCode == players[i].Left)
                {
                    players[i].MoveLeft = false;
                }
                else if (e.KeyCode == players[i].Right)
                {
                    players[i].MoveRight = false;
                }
                else if (e.KeyCode == players[i].Fire)
                {
                    players[i].OpenFire = false;
                }
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (e.KeyCode == players[i].Up)
                {
                    players[i].MoveUp = true;

                }
                else if (e.KeyCode == players[i].Down)
                {
                    players[i].MoveDown = true;

                }
                else if (e.KeyCode == players[i].Left)
                {
                    players[i].MoveLeft = true;

                }
                else if (e.KeyCode == players[i].Right)
                {
                    players[i].MoveRight = true;

                }
                else if (e.KeyCode == players[i].Fire)
                {
                    players[i].OpenFire = true;
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i].Move();
                if (CollisionBullet(bullets[i])) { bullets.RemoveAt(i); i--; }
            }

            for (int i = 0; i < players.Count; i++)
            {
                if (!players[i].BreakShield)
                {
                    elapsedTime = players[i].watchShield.Elapsed;
                    if (elapsedTime.Seconds >= players[i].BreakWatchShield)
                    {
                        players[i].BreakShield = true;
                        players[i].watchShield.Stop();
                    }
                }

                if (players[i].BreakFire)
                {
                    elapsedTime = players[i].watchFire.Elapsed;
                    if (elapsedTime.Seconds >= players[i].BreakWatchFire)
                    {
                        players[i].BreakFire = false;
                        players[i].watchFire.Reset();
                    }
                }

                if (players[i].OpenFire && !players[i].BreakFire)
                {
                    if (players[i].Blocks)
                    {
                        players[i].Blocks = false;

                        int rotation = players[i].Rotation;
                        int x = 0, y = 0;
                        switch (rotation)
                        {
                            case 1:
                            {
                                x = players[i].X / S - 1;
                                y = players[i].Y / S - 1;
                                break;
                            }
                            case 2:
                            {
                                x = players[i].X / S + 1;
                                y = players[i].Y / S - 1;
                                break;
                            }
                            case 3:
                            {
                                x = players[i].X / S - 1;
                                y = players[i].Y / S + 1;
                                break;
                            }
                            case 4:
                            {
                                x = players[i].X / S - 1;
                                y = players[i].Y / S - 1;
                                break;
                            }
                        }

                        int nx, ny;
                        if (rotation % 2 == 0) { nx = x + 1; ny = y + 3; }
                        else { nx = x + 3; ny = y + 1; }

                        int ind;
                        if (i == 0) { ind = 1; }
                        else { ind = 0; }

                        for (int n = y; n < ny; n++)
                        {
                            for (int m = x; m < nx; m++)
                            {
                                if (n >= 0 && n < H && m >= 0 && m < W)
                                {
                                    if (!CollisionPlayer(players[ind], map[n][m]))
                                    {
                                        map[n][m] = new Block(m * S, n * S);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        bullets.Add(new Bullet(players[i], S));
                        if (CollisionBullet(bullets[bullets.Count - 1]))
                        {
                            bullets.RemoveAt(bullets.Count - 1);
                        }
                    }

                    players[i].BreakFire = true;
                    players[i].watchFire.Start();
                }
                else
                {
                    Equipment cell;
                    if (players[i].MoveUp)
                    {
                        players[i].Rotation = 1;
                        MoveUp(players[i]);
                        cell = CollisionEquipment(players[i]);

                        if (cell != null)
                        {
                            UseEquipment(cell, players[i]);
                        }
                    }
                    else if (players[i].MoveDown)
                    {
                        players[i].Rotation = 3;
                        MoveDown(players[i]);
                        cell = CollisionEquipment(players[i]);

                        if (cell != null)
                        {
                            UseEquipment(cell, players[i]);
                        }
                    }
                    else if (players[i].MoveLeft)
                    {
                        players[i].Rotation = 4;
                        MoveLeft(players[i]);
                        cell = CollisionEquipment(players[i]);

                        if (cell != null)
                        {
                            UseEquipment(cell, players[i]);
                        }
                    }
                    else if (players[i].MoveRight)
                    {
                        players[i].Rotation = 2;
                        MoveRight(players[i]);
                        cell = CollisionEquipment(players[i]);

                        if (cell != null)
                        {
                            UseEquipment(cell, players[i]);
                        }
                    }
                }
            }

            elapsedTime = watchEquip.Elapsed;
            if (elapsedTime.Seconds >= 15)
            {
                watchEquip.Reset();
                SpawnEquipment();
                watchEquip.Start();
            }

            elapsedTime = watchMapCut.Elapsed;
            if (elapsedTime.Minutes >= 1)
            {
                watchMapCut.Reset();
                if (MapCut()) { watchMapCut.Start(); }
            }

            Invalidate();

            int res = CheckWin();
            if (res != -1)
            {
                timer.Stop();

                switch (res)
                {
                    case 0:
                    {
                        MessageBox.Show("Ничья!!!");
                        break;
                    }
                    case 1:
                    {
                        MessageBox.Show("Победил Первый!!!");
                        break;
                    }
                    case 2:
                    {
                        MessageBox.Show("Победил Второй!!!");
                        break;
                    }
                }

                Close();
            }
        }

        private void MoveUp(Player player)
        {
            int y = player.Y - player.Speed;
            if (y <= Hs * S)
            {
                y = Hs * S;
            }
            else if (map[y / S][player.X / S].Type == NamesCell["wall"] ||
                     map[y / S][(player.X + player.Width - 1) / S].Type == NamesCell["wall"])
            {
                y = y / S + 1;
                y *= S;
            }
            player.Y = y;
        }
        private void MoveDown(Player player)
        {
            int y = player.Y + player.Height + player.Speed;
            if (y >= He * S)
            {
                y = He * S;
            }
            else if (map[y / S][player.X / S].Type == NamesCell["wall"] ||
                     map[y / S][(player.X + player.Width - 1) / S].Type == NamesCell["wall"])
            {
                y = y / S;
                y *= S;
            }
            player.Y = y - player.Height;
        }
        private void MoveLeft(Player player)
        {
            int x = player.X - player.Speed;
            if (x <= Ws * S)
            {
                x = Ws * S;
            } else if (map[player.Y / S][x / S].Type == NamesCell["wall"] ||
                       map[(player.Y + player.Height - 1) / S][x / S].Type == NamesCell["wall"])
            {
                x = x / S + 1;
                x *= S;
            }
            player.X = x;
        }
        private void MoveRight(Player player)
        {
            int x = player.X + player.Width + player.Speed;
            if (x >= We * S)
            {
                x = We * S;
            }
            else if (map[player.Y / S][x / S].Type == NamesCell["wall"] ||
                     map[(player.Y + player.Height - 1) / S][x / S].Type == NamesCell["wall"])
            {
                x = x / S;
                x *= S;
            }
            player.X = x - player.Width;
        }
        private bool CollisionPlayer(Player player, Cell cell)
        {
            Rectangle cellBounds = new Rectangle(cell.X, cell.Y, S, S);
            Rectangle playerBounds = new Rectangle(player.X, player.Y, player.Width, player.Height);
            return cellBounds.IntersectsWith(playerBounds);
        }
        private void PlayerRespawn(Player player)
        {
            if (player.Ind == 0)
            {
                player.X = Ws * S;
                player.Y = Hs * S;
            }
            else
            {
                player.X = We * S - S + S - player.Width;
                player.Y = He * S - S + S - player.Height;
            }
        }

        private bool CollisionBullet(Bullet bullet)
        {
            Rectangle bulletBounds = new Rectangle(bullet.X, bullet.Y, bullet.Width, bullet.Height);

            for (int i = 0; i < players.Count; i++)
            {
                if (bulletBounds.IntersectsWith(new Rectangle(players[i].X, players[i].Y,
                    players[i].Width, players[i].Height)))
                {
                    if (players[i].BreakShield)
                    {
                        players[i].curHealth -= bullet.Damage;
                    }
                    return true;
                }
            }

            if (bullet.X <= Ws * S || bullet.X >= We * S ||
                bullet.Y <= Hs * S || bullet.Y >= He * S ||
                bullet.X + bullet.Width <= Ws * S || bullet.X + bullet.Width >= We * S ||
                bullet.Y + bullet.Height <= Hs * S || bullet.Y + bullet.Height >= He * S)
            {
                return true;
            }

            if (map[bullet.Y / S][bullet.X / S].Type == NamesCell["wall"])
            { return BlockFire(bullet.X / S, bullet.Y / S, bullet); }
            else if(map[bullet.Y / S][(bullet.X + bullet.Width) / S].Type == NamesCell["wall"])
            { return BlockFire((bullet.X + bullet.Width) / S, bullet.Y / S, bullet); }
            else if (map[(bullet.Y + bullet.Height) / S][bullet.X / S].Type == NamesCell["wall"])
            { return BlockFire(bullet.X / S, (bullet.Y + bullet.Height) / S, bullet); }
            else if (map[(bullet.Y + bullet.Height) / S][(bullet.X + bullet.Width) / S].Type == NamesCell["wall"])
            { return BlockFire((bullet.X + bullet.Width) / S, (bullet.Y + bullet.Height) / S, bullet); }

            return false;
        }
        private bool BlockFire(int x, int y, Bullet bullet)
        {
            if (bullet.Protect)
            {
                bullet.Protect = false;
                map[y][x] = new Cell(x * S, y * S);
                return false;
            }
            else
            {
                (map[y][x] as Block).Health -= bullet.Damage;

                if ((map[y][x] as Block).Health <= 0)
                {
                    map[y][x] = new Cell(x * S, y * S);
                }
                return true;
            }
        }

        private void SpawnEquipment()
        {
            FreeCells();
            if (freeCells.Count == 0) { return; }
            int ind = rand.Next(freeCells.Count);
            Cell cell = map[freeCells[ind].Y / S][freeCells[ind].X / S];

            int width = (int)(S * 0.4);
            int height = (int)(S * 0.4);
            int x = cell.X + (S - width) / 2;
            int y = cell.Y + (S - height) / 2;

            int equipm = rand.Next(NamesEquip.Count);
            map[freeCells[ind].Y / S][freeCells[ind].X / S] = new Equipment(equipm, x, y, width, height);

            freeCells.Clear();
        }
        private void FreeCells()
        {
            for (int i = Hs; i < He; i++)
            {
                for (int j = Ws; j < We; j++)
                {
                    if (map[i][j].Type == NamesCell["space"])
                    {
                        if (!CollisionPlayer(players[0], map[i][j]) &&
                            !CollisionPlayer(players[1], map[i][j]))
                        {
                            freeCells.Add(new Cell(map[i][j]));
                        }
                    }
                }
            }
        }
        private Equipment CollisionEquipment(Player player)
        {
            List<Equipment> cells = new List<Equipment>();

            if (map[player.Y / S][player.X / S].Type == NamesCell["equipment"])
            {
                cells.Add(new Equipment((map[player.Y / S][player.X / S] as Equipment)));
            }
            if (map[player.Y / S][(player.X + player.Width - 1) / S].Type == NamesCell["equipment"])
            {
                cells.Add(new Equipment((map[player.Y / S][(player.X + player.Width - 1) / S] as Equipment)));
            }
            if (map[(player.Y + player.Height - 1) / S][player.X / S].Type == NamesCell["equipment"])
            {
                cells.Add(new Equipment((map[(player.Y + player.Height - 1) / S][player.X / S] as Equipment)));
            }
            if (map[(player.Y + player.Height - 1) / S][(player.X + player.Width - 1) / S].Type == NamesCell["equipment"])
            {
                cells.Add(new Equipment((map[(player.Y + player.Height - 1) / S][(player.X + player.Width - 1) / S] as Equipment)));
            }

            Rectangle playerBounds = new Rectangle(player.X, player.Y, player.Width, player.Height);
            Rectangle cellBounds;
            for (int i = 0; i < cells.Count; i++)
            {
                cellBounds = new Rectangle(cells[i].X, cells[i].Y, cells[i].Width, cells[i].Height);
                if (cellBounds.IntersectsWith(playerBounds)) { return cells[i]; }
            }
            return null;
        }
        private void UseEquipment(Equipment cell, Player player)
        {
            if (cell.Equip == NamesEquip["health"])
            {
                if (player.curHealth < player.maxHealth) { player.curHealth++; }
            }
            else if (cell.Equip == NamesEquip["shot"])
            {
                player.BigShot = true;
            }
            else if (cell.Equip == NamesEquip["shield"])
            {
                player.watchShield.Reset();
                player.watchShield.Start();
                player.BreakShield = false;
            }
            else if (cell.Equip == NamesEquip["blocks"])
            {
                player.Blocks = true;
            }
            map[cell.Y / S][cell.X / S] = new Cell(cell.X / S, cell.Y / S);
        }

        private  bool MapCut()
        {
            bool respawnP1 = false;
            bool respawnP2 = false;
            bool cut = false;

            if (We - Ws > 5)
            {
                cut = true;
                for (int i = Hs; i < He; i++)
                {
                    map[i][Ws] = new Cell(Ws * S, i * S, NamesCell["dead"]);
                    map[i][We - 1] = new Cell(We * S - S, i * S, NamesCell["dead"]);

                    if (!respawnP1)
                    {
                        if(CollisionPlayer(players[0], map[i][Ws]) ||
                           CollisionPlayer(players[0], map[i][We - 1]))
                        {
                            respawnP1 = true;
                        }
                    }

                    if (!respawnP2)
                    {
                        if (CollisionPlayer(players[1], map[i][Ws]) ||
                           CollisionPlayer(players[1], map[i][We - 1]))
                        {
                            respawnP2 = true;
                        }
                    }
                }
                Ws++; We--;
            }
            if (He - Hs > 5)
            {
                cut = true;
                for (int j = Ws; j < We; j++)
                {
                    map[Hs][j] = new Cell(j * S, Hs * S, NamesCell["dead"]);
                    map[He - 1][j] = new Cell(j * S, He * S - S, NamesCell["dead"]);

                    if (!respawnP1)
                    {
                        if (CollisionPlayer(players[0], map[Hs][j]) ||
                            CollisionPlayer(players[0], map[He - 1][j]))
                        {
                            respawnP1 = true;
                        }
                    }

                    if (!respawnP2)
                    {
                        if (CollisionPlayer(players[1], map[Hs][j]) ||
                            CollisionPlayer(players[1], map[He - 1][j]))
                        {
                            respawnP2 = true;
                        }
                    }
                }
                Hs++; He--;
            }

            if (respawnP1)
            {
                PlayerRespawn(players[0]);
                if (players[0].BreakShield) { players[0].curHealth--; }
                if (map[players[0].Y / S][players[0].X / S].Type != NamesCell["space"])
                {
                    map[players[0].Y / S][players[0].X / S] = new Cell(players[0].X, players[0].Y);
                }
            }
            if (respawnP2)
            {
                PlayerRespawn(players[1]);
                if (players[1].BreakShield) { players[1].curHealth--; }
                if (map[players[1].Y / S][players[1].X / S].Type != NamesCell["space"])
                {
                    map[players[1].Y / S][players[1].X / S] = new Cell(players[1].X, players[1].Y);
                }
            }

            return cut;
        }

        private int CheckWin()
        {
            if (players[0].curHealth <= 0 &&
                players[1].curHealth <= 0)
            {
                return 0;
            } else if (players[1].curHealth <= 0)
            {
                return 1;
            } else if (players[0].curHealth <= 0)
            {
                return 2;
            }

            return -1;
        }
    }
}