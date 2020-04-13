using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EpidemieForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Ground.Width = pictureBox1.Width;
            Ground.Height = pictureBox1.Height;
            Ground.BulletRadius = 2;
            Ground.Speed = 1;
        }

        int NbBullets = 1;
        Ground ground;
        Stopwatch sw = new Stopwatch();
        int totalMalade = 0, totalImmunite = 0;

        private void timerFPS_Tick(object sender, EventArgs e)
        {
            if (ground == null) return;
            sw.Restart();
            ground.Update();
            pictureBox1.Refresh();

            lblFPS.Text = $"Time:{sw.ElapsedMilliseconds,4} ms";
            lblMalade.Text = $"Malades  : {totalMalade,5}";
            lblImmunite.Text = $"Immunite : {totalImmunite,5}";
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (ground == null) return;
            totalMalade = totalImmunite = 0;

            for (int i = 0; i < NbBullets; ++i)
            {
                var u = ground.AllBullets[i];
                int x = (int)(u.x - Ground.BulletRadius);
                int y = (int)(u.y - Ground.BulletRadius);

                var color = Brushes.Lime;
                if (u.state == State.Malade)
                {
                    ++totalMalade;
                    color = Brushes.DarkRed;
                }

                if (u.state == State.Immunite)
                {
                    ++totalImmunite;
                    color = Brushes.LightGray;
                }

                e.Graphics.FillEllipse(color, x, y, (int)Ground.BulletRadius * 2, (int)Ground.BulletRadius * 2);
            }
        }

        private void btnPlusUn_Click(object sender, EventArgs e)
        {
            int nb0 = (int)numericUpDown6.Value;
            for (int k = 0; k < nb0; ++k)
            {
                ground.AllBullets[k].state = State.Malade;
                ground.AllBullets[k].delaiContagion = Ground.DelaiContagion;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            NbBullets = (int)numericUpDown1.Value;
            Ground.DelaiContagion = (int)numericUpDown4.Value;
            Ground.BulletRadius = (double)numericUpDown3.Value;
            Ground.Speed = (double)numericUpDown2.Value * Ground.BulletRadius / 12;
            timerFPS.Interval = 100 / (int)numericUpDown5.Value;
            ground = new Ground(NbBullets);

            int nb0 = (int)numericUpDown6.Value;
            for(int k = 0; k < nb0; ++k)
                ground.AllBullets[k].state = State.Malade;

        }
    }
}
