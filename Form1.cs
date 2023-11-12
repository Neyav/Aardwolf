using System.Diagnostics;
using System.DirectoryServices;

namespace Aardwolf
{
    public partial class Form1 : Form
    {
        dataHandler dh = new dataHandler();
        palettehandler ph = new palettehandler();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();

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
            dh.prepareVSWAP();

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
            pictureBox1.Refresh();

            int levels = dh.getLevels();

            for (int i = 0; i < levels; i++)
            {
                comboBox1.Items.Add(dh.getLevelName(i));
            }

            VSWAPHeader VSWAPH = dh.getVSWAPHeader;

            for (int i = 0; i < VSWAPH.spiteStart; i++)
            {
                comboBox2.Items.Add("Texture - " + i.ToString());
            }

            bitmap = new Bitmap(64, 64);
            byte[] texturedata = dh.getTexture(0);

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    int offset = (y * 64 + x);

                    int r = texturedata[offset];
                    int g = texturedata[offset];
                    int b = texturedata[offset];
                    int a = 255;

                    bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            pictureBox2.Image = bitmap;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.Refresh();
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

                    if (g > 100)
                        r = b = 0;
                    if (r > 40)
                        g = b = 0;
                    if (b > 10)
                        r = g = 0;

                    r *= 2;
                    b *= 6;

                    int a = 255;
                    Debug.WriteLine("Leveldata for x: {0}, y: {1} is {2}", x, y, r);

                    bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            pictureBox1.Image = bitmap;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Refresh();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte[] texturedata = dh.getTexture(comboBox2.SelectedIndex);

            Bitmap bitmap = new Bitmap(64, 64);

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    RGBA RGBa = ph.getPaletteColor(texturedata[x * 64 + y]);

                    bitmap.SetPixel(x, y, Color.FromArgb(RGBa.r, RGBa.g, RGBa.b));
                }
            }

            pictureBox2.Image = bitmap;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.Refresh();
        }
    }
}