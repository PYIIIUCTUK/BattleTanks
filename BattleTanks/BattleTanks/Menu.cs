using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BattleTanks
{
    public partial class Menu : Form
    {
        private List<string> maps;

        public Menu()
        {
            InitializeComponent();

            maps = new List<string>(System.IO.Directory.GetFiles("Maps"));

            int pos;
            int pos2;
            string data;

            for (int i = 0; i < maps.Count; i++)
            {
                data = maps[i];
                pos = data.IndexOf('\\');
                pos2 = data.IndexOf('.');

                listBox1.Items.Add(data.Substring(pos + 1, pos2 - pos - 1));
            }
            listBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Game form = new Game(this, maps[listBox1.SelectedIndex]);
            form.Show();
            Hide();
        }

        private void Menu_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
