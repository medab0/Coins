using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using ImageProcess2;

namespace Coins
{
    public partial class Form1 : Form
    {
        Bitmap loaded, image;

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (loaded == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            Bitmap processedImage = CoinCounter.ProcessCoin(loaded, 128); 

            double totalValue = CoinCounter.CountCoins(processedImage);

            pictureBox3.Image = processedImage; 
            label1.Text = $"Total Value: ₱{(totalValue):F2}";
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            loaded = new Bitmap(openFileDialog2.FileName);
            pictureBox2.Image = loaded;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(openFileDialog2.FileName);
                pictureBox2.Image = image;
            }
        }

        
    }
}
