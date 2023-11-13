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

            Bitmap bitmap = new Bitmap(1280, 1280);

            // There must be a better way to do this.
            for (int x = 0; x < 1280; x++)
            {
                for (int y = 0; y < 1280; y++)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                }
            }

            List<int> tilecount = new List<int>();

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    int offset = (y * 64 + x) * 2;
                    if (leveldata[offset] == 0 || leveldata[offset] > 64) // 64 is maxtile in Wolf3D.
                        continue;
                    int texture = (leveldata[offset] - 1) * 2;

                    tilecount.Add(leveldata[offset]);

                    // Now load the appropriate texture.
                    byte[] texturedata = dh.getTexture(texture);

                    // Now we need to draw a 10x10 square of the texturedata onto the bitmap.
                    for (int x2 = 0; x2 < 20; x2++)
                    {
                        for (int y2 = 0; y2 < 20; y2++)
                        {
                            int offset2 = ((x2 * 3) * 64 + (y2 * 3));

                            RGBA RGBa = ph.getPaletteColor(texturedata[offset2]);

                            bitmap.SetPixel(x * 20 + x2, y * 20 + y2, Color.FromArgb(RGBa.r, RGBa.g, RGBa.b));
                        }
                    }

                    //bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            // Display, in order from most to least, the number of tiles used in the level.
            var sorted = tilecount.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).ToList();

            string tilecountstring = "";

            for (int i = 0; i < sorted.Count(); i++)
            {
                tilecountstring += sorted[i].ToString() + " ";
            }

            Debug.WriteLine(tilecountstring);

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