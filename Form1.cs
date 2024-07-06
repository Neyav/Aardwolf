using System.Diagnostics;
using System.DirectoryServices;
using System;
using System.Drawing;
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
                comboBox2.Items.Add("Sprite - " + (i - VSWAPH.spriteStart).ToString());
            }
        }

        private Bitmap rendercurrentLevel(int sizeWidth, int sizeHeight)
        {
            int selectedLevel = comboBox1.SelectedIndex;
            byte[] leveldata = dh.getLevelData(selectedLevel);

            Bitmap bitmap = new Bitmap(sizeWidth, sizeHeight);

            // There must be a better way to do this.
            for (int x = 0; x < sizeWidth; x++)
            {
                for (int y = 0; y < sizeHeight; y++)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                }
            }

            for (int x = 0; x < dh.levelWidth(selectedLevel); x++)
            {
                for (int y = 0; y < dh.levelHeight(selectedLevel); y++)
                {
                    int offset = (y * dh.levelHeight(selectedLevel) + x) * 2;

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

                    // Now load the appropriate texture.
                    Bitmap texturedata = dh.getTexture(texture);

                    // Determine where the image is to be drawn.
                    int tileWidth = (int) ((float)sizeWidth / dh.levelWidth(selectedLevel));
                    int tileHeight = (int) ((float)sizeHeight / dh.levelHeight(selectedLevel));
                    int drawX = x * tileWidth;
                    int drawY = y * tileHeight;

                    Byte tileActor = dh.getTileActor(comboBox1.SelectedIndex, x, y);

                    bool isPushWall = tileActor == 98;

                    // Now we need to draw the texturedata onto the bitmap, scaled for our bitmap size.
                    if (texturedata != null)
                    {
                        // Draw a scaled version of the texture to our image.
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            graphics.DrawImage(texturedata, drawX, drawY, tileWidth, tileHeight);
                        }

                        // If it's a pushwall make it red with a translucent red border around it.
                        if (isPushWall)
                        {
                            
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                // Draw a red border around the pushwall.
                                g.DrawRectangle(new Pen(Color.Red), drawX, drawY, tileWidth - 1, tileHeight - 1);
                            
                                // Make a semi-transparent red brush the size of the internals of the pushwall, excluding the 1 pixel border.
                                // Paint it transparent red.
                                SolidBrush brush = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
                                g.FillRectangle(brush, drawX + 1, drawY + 1, tileWidth - 2, tileHeight - 2);
                            }

                        }
                    }
                    else
                    { // draw a sprite.

                        int renderSprite = -1;
                        bool _isSOD = false;

                        if (radioButton2.Checked)
                            _isSOD = true;

                        // Draw the actor on top of the tile.
                        if (tileActor >= 19 && tileActor <= 22) // Player Start
                        {
                            if (_isSOD)
                                renderSprite = 0;   // SOD doesn't have a player start sprite. Use the demo sprite.
                            else
                                renderSprite = 409;
                        }
                        else if (tileActor >= 23 && tileActor <= 74) // Assorted map objects
                        {
                            renderSprite = tileActor - 21;
                        }

                        if (renderSprite >= 0) // We have a sprite to render.
                        {
                            Bitmap sprite = dh.getSprite(renderSprite);

                            // Scale and draw the Bitmap sprite onto the level bitmap.

                            using (Graphics graphics = Graphics.FromImage(bitmap))
                            {
                                graphics.DrawImage(sprite, drawX, drawY, tileWidth, tileHeight);
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

            // Save a copy so we can look at it. Make it fullsized so it's BEAUTFIUL. ;)
            this.rendercurrentLevel(4096, 4096).Save("level.png");

            // Enable the Render 3D button now that a level has been selected.
            button2.Enabled = true;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (comboBox2.SelectedIndex >= dh.getVSWAPHeader.spriteStart)
            {
                pictureBox2.BackColor = Color.Magenta;
                pictureBox2.Image = dh.getSprite(comboBox2.SelectedIndex - dh.getVSWAPHeader.spriteStart);
            }
            else
            {
                pictureBox2.BackColor = Color.Black;
                pictureBox2.Image = dh.getTexture(comboBox2.SelectedIndex);
            }

            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.Refresh();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}