using System.DirectoryServices;

namespace Aardwolf
{
    public partial class Form1 : Form
    {
        dataHandler dh = new dataHandler();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();

            dh = new dataHandler();

            if (radioButton1.Checked)
            {
                dh.loadAllData(false);
            }
            else
            {
                dh.loadAllData(true);
            }

            dh.parseLevelData();

            byte[] leveldata = dh.getLevelData(0);

            Bitmap bitmap = new Bitmap(64, 64);

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    int offset = (y * 64 + x) * 2;

                    int r = leveldata[offset];
                    int g = leveldata[offset];
                    int b = leveldata[offset];
                    int a = 255;

                    bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            pictureBox1.Image = bitmap;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Size = new Size(640, 640);
            pictureBox1.Refresh();

            int levels = dh.getLevels();

            for (int i = 0; i < levels; i++)
            {
                comboBox1.Items.Add(dh.getLevelName(i));
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte[] leveldata = dh.getLevelData(comboBox1.SelectedIndex);

            Bitmap bitmap = new Bitmap(64, 64);

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    int offset = (y * 64 + x) * 2;

                    int r = leveldata[offset];
                    int g = leveldata[offset];
                    int b = leveldata[offset];
                    int a = 255;

                    bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            pictureBox1.Image = bitmap;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Size = new Size(640, 640);
            pictureBox1.Refresh();
        }
    }
}