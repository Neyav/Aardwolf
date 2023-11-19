using System.Diagnostics;
using System.DirectoryServices;

namespace Aardwolf
{
    public partial class Form1 : Form
    {
        dataHandler dh = new dataHandler();
        palettehandler ph = new palettehandler(false);

        int previewZoom = 0;
        int previewCenterX = 0;
        int previewCenterY = 0;

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
                ph = new palettehandler(false);
            }
            else
            {
                dh.loadAllData(true);
                ph = new palettehandler(true);
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

        private void rendercurrentLevel()
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
                    if (leveldata[offset] == 0 || (leveldata[offset] > 64 && leveldata[offset] < 90) || leveldata[offset] > 101) // 64 is maxtile in Wolf3D.
                        continue;

                    int texture = 0;

                    if (leveldata[offset] >= 90)
                    {  // This is a door.
                        if (leveldata[offset] == 90 || leveldata[offset] == 91)
                            texture = 98;
                        if (leveldata[offset] > 91 && leveldata[offset] < 100)
                            texture = 104;
                        if (leveldata[offset] == 100 || leveldata[offset] == 101)
                            texture = 102;
                    }
                    else
                        texture = (leveldata[offset] - 1) * 2;

                    tilecount.Add(leveldata[offset]);

                    // Now load the appropriate texture.
                    byte[] texturedata = dh.getTexture(texture);

                    // Determine where the image is to be drawn based on previewZoom.
                    int drawX = 0;
                    int drawY = 0;
                    int tileWidth = 0;
                    int tileHeight = 0;

                    // previewZoom 1 = 2x zoom, 2 = 4x zoom, 3 = 8x zoom.
                    // previewCenterX and previewCenterY are the center of the preview.
                    // We need to calculate where each tile is to be drawn based on the zoom and center.
                    if (previewZoom == 1)
                    {
                        drawX = (x - previewCenterX) * 40 + 640;
                        drawY = (y - previewCenterY) * 40 + 640;
                        tileWidth = 40;
                        tileHeight = 40;
                    }
                    else
                    {
                        tileWidth = 20;
                        tileHeight = 20;
                        drawX = x * 20;
                        drawY = y * 20;
                    }

                    // Now we need to draw a 10x10 square of the texturedata onto the bitmap.
                    for (int x2 = 0; x2 < tileHeight; x2++)
                    {
                        for (int y2 = 0; y2 < tileWidth; y2++)
                        {
                            int offset2 = (int)((x2 * (float)(64/tileHeight)) * 64 + (y2 * (float)(64/tileWidth)));

                            RGBA RGBa = ph.getPaletteColor(texturedata[offset2]);

                            if (drawX +x2 > 0 && drawY + y2 > 0 && drawX + x2 < 1280 && drawY + y2 < 1280)
                                bitmap.SetPixel(drawX + x2, drawY + y2, Color.FromArgb(RGBa.r, RGBa.g, RGBa.b));
                        }
                    }

                    //bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            pictureBox1.Image = bitmap;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Refresh();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.rendercurrentLevel();
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

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Set preview zoom to 0-3.
            previewZoom++;
            if (previewZoom > 3)
                previewZoom = 0;

            if (previewZoom == 0) return;

            // Get the tile that was clicked.
            int x = (int)Math.Floor((double)(MousePosition.X - pictureBox1.Location.X - this.Location.X - 8) / 20);
            int y = (int)Math.Floor((double)(MousePosition.Y - pictureBox1.Location.Y - this.Location.Y - 30) / 20);

            // Set the center of the preview to the tile that was clicked.
            previewCenterX = x;
            previewCenterY = y;

            this.rendercurrentLevel();
        }
    }
}