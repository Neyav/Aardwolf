using System.Diagnostics;
using System.DirectoryServices;
using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Windowing.Desktop;

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

            for (int i = 0; i < VSWAPH.spriteStart; i++)
            {
                comboBox2.Items.Add("Texture - " + i.ToString());
            }
            for (int i = VSWAPH.spriteStart; i < VSWAPH.soundStart; i++)
            {
                comboBox2.Items.Add("Sprite - " + i.ToString());
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

        private Bitmap rendercurrentLevel(int sizeWidth, int sizeHeight)
        {
            byte[] leveldata = dh.getLevelData(comboBox1.SelectedIndex);

            Bitmap bitmap = new Bitmap(sizeWidth, sizeHeight);

            // There must be a better way to do this.
            for (int x = 0; x < sizeWidth; x++)
            {
                for (int y = 0; y < sizeHeight; y++)
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

                    if (leveldata[offset] >= 90 && leveldata[offset] <= 101)
                    {  // This is a door.
                        VSWAPHeader VSWAPHead = dh.getVSWAPHeader;
                        byte doorType = 0;
                        byte doorWall = (byte)(VSWAPHead.spriteStart - 8); 

                        switch (leveldata[offset])
                        {
                            case 90:
                            case 92:
                            case 94:
                            case 96:
                            case 98:
                            case 100:
                                doorType = (byte)((leveldata[offset] - 90) / 2);
                                break;
                            case 91:
                            case 93:
                            case 95:
                            case 97:
                            case 99:
                            case 101:
                                doorType = (byte)((leveldata[offset] - 91) / 2);
                                break;
                        }
                        
                        switch (doorType)
                        {
                            case 0:
                                texture = doorWall;
                                break;
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                texture = doorWall + 6;
                                break;
                            case 5:
                                texture = doorWall + 4;
                                break;
                        }

                    }
                    else
                        texture = (leveldata[offset] - 1) * 2;

                    tilecount.Add(leveldata[offset]);

                    // Now load the appropriate texture.
                    byte[] texturedata = dh.getTexture(texture);

                    // Determine where the image is to be drawn based on previewZoom.
                    int tileWidth = (int) ((float)sizeWidth / 64);
                    int tileHeight = (int) ((float)sizeHeight / 64);
                    int drawX = x * tileWidth;
                    int drawY = y * tileHeight;

                    // previewZoom 1 = 2x zoom, 2 = 100% zoom.
                    // previewCenterX and previewCenterY are the center of the preview.
                    // We need to calculate where each tile is to be drawn based on the zoom and center.
                 
                    /*if (previewZoom == 1)
                    {
                        drawX = (x - previewCenterX) * sizeWidth / 32 + 0;
                        drawY = (y - previewCenterY) * sizeHeight / 32 + 0;
                        tileWidth = sizeWidth / 32;
                        tileHeight = sizeHeight / 32;
                    }
                    else if (previewZoom == 2)
                    {
                        drawX = (x - previewCenterX) * sizeWidth / 16 + 0;
                        drawY = (y - previewCenterY) * sizeHeight / 16 + 0;
                        tileWidth = sizeWidth / 16;
                        tileHeight = sizeHeight / 16;
                    }*/
                    int tileActor = dh.getTileActor(comboBox1.SelectedIndex, x, y);
                    bool isPushWall = tileActor == 98;

                    // Now we need to draw the texturedata onto the bitmap, scaled for our bitmap size.
                    for (int x2 = 0; x2 < tileWidth; x2++)
                    {
                        for (int y2 = 0; y2 < tileHeight; y2++)
                        {
                            int offset2 = (int)(((float)x2 * (float)(64 / tileWidth)) * (float)64 + ((float)y2 * (float)(64 / tileHeight)));

                            RGBA RGBa = ph.getPaletteColor(texturedata[offset2]);

                            // If it's a push wall give it a strong red tint so it stands out.
                            if (isPushWall)
                            {
                                RGBa.r = 255;
                            }
                            
                            if (drawX + x2 > 0 && drawY + y2 > 0 && drawX + x2 < sizeHeight && drawY + y2 < sizeWidth)
                                bitmap.SetPixel(drawX + x2, drawY + y2, Color.FromArgb(RGBa.r, RGBa.g, RGBa.b));
                        }
                    }

                    // If it's a pushwall draw a red border around it.
                    if (isPushWall)
                    {
                        for (int x2 = 0; x2 < tileWidth; x2++)
                        {
                            for (int y2 = 0; y2 < tileHeight; y2++)
                            {
                                if (drawX + x2 > 0 && drawY + y2 > 0 && drawX + x2 < sizeHeight && drawY + y2 < sizeWidth)
                                {
                                    if (x2 == 0 || x2 == tileWidth - 1 || y2 == 0 || y2 == tileHeight - 1)
                                        bitmap.SetPixel(drawX + x2, drawY + y2, Color.FromArgb(255, 255, 0, 0));
                                }
                            }
                        }
                    }


                }
            }

            return bitmap;
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = this.rendercurrentLevel(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Refresh();

            // Save a copy so we can look at it.
            this.rendercurrentLevel(2048, 2048).Save("level.png");

            // Enable the Render 3D button now that a level has been selected.
            button2.Enabled = true;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte[] texturedata = dh.getTexture(comboBox2.SelectedIndex);

            Bitmap bitmap = new Bitmap(64, 64);

            // If it's a sprite, load the sprite -- Admitedly this is a bit of a hack. I should move getTexture to output a bitmap just like getSprite.
            if (comboBox2.SelectedIndex >= dh.getVSWAPHeader.spriteStart)
            {
                pictureBox2.BackColor = Color.Magenta;
                pictureBox2.Image = dh.getSprite(comboBox2.SelectedIndex - dh.getVSWAPHeader.spriteStart);
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox2.Refresh();

                return;
            }

            pictureBox1.BackColor = Color.Black;

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
            if (previewZoom > 2)
                previewZoom = 0;

            if (previewZoom == 0)
            {
                pictureBox1.Image = this.rendercurrentLevel(1280, 1280);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.Refresh();
                return;
            }

            // Get the tile that was clicked.
            int x = (int)Math.Floor((double)(MousePosition.X - pictureBox1.Location.X - this.Location.X - 8) / 20);
            int y = (int)Math.Floor((double)(MousePosition.Y - pictureBox1.Location.Y - this.Location.Y - 30) / 20);

            // Set the center of the preview to the tile that was clicked.
            previewCenterX = x;
            previewCenterY = y;

            pictureBox1.Image = this.rendercurrentLevel(1280, 1280);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}