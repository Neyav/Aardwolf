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
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;


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
            button3.Enabled = false;

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
            bool _isSOD = false;
            int playerSpawnHeight = 0;
            int playerSpawnWidth = 0;

            if (radioButton2.Checked)
                _isSOD = true;

            maphandler mapdata = new maphandler(_isSOD);
            mapdata.importMapData(dh.getLevelData(selectedLevel), dh.levelHeight(selectedLevel), dh.levelWidth(selectedLevel));

            Bitmap bitmap = new Bitmap(sizeWidth, sizeHeight);

            // There must be a better way to do this.
            for (int x = 0; x < sizeWidth; x++)
            {
                for (int y = 0; y < sizeHeight; y++)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                }
            }

            for (int x = 0; x < mapdata.getMapWidth(); x++)
            {
                for (int y = 0; y < mapdata.getMapHeight(); y++)
                {
                    byte leveldata = mapdata.getTileData(y, x);
                    int texture = 0;
                    // Setup render location and size for any future drawing.
                    int tileWidth = (int)((float)sizeWidth / mapdata.getMapWidth());
                    int tileHeight = (int)((float)sizeHeight / mapdata.getMapHeight());
                    int drawX = x * tileWidth;
                    int drawY = y * tileHeight;
                    Bitmap texturedata = null;

                    if (leveldata > 0)
                    {
                        if (leveldata >= 90 && leveldata <= 101)
                        {  // This is a door.
                            VSWAPHeader VSWAPHead = dh.getVSWAPHeader;
                            byte doorType = 0;
                            byte doorWall = (byte)(VSWAPHead.spriteStart - 8);

                            switch (leveldata)
                            {
                                case 90:
                                case 92:
                                case 94:
                                case 96:
                                case 98:
                                case 100:
                                    doorType = (byte)((leveldata - 90) / 2);
                                    break;
                                case 91:
                                case 93:
                                case 95:
                                case 97:
                                case 99:
                                case 101:
                                    doorType = (byte)((leveldata - 91) / 2);
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
                            texture = (leveldata - 1) * 2;

                        // Now load the appropriate texture.
                        texturedata = dh.getTexture(texture);
                    }

                    Byte tileActor = dh.getTileActor(comboBox1.SelectedIndex, x, y);

                    mapdata.spawnMapObject(tileActor, y, x);

                    // Now we need to draw the texturedata onto the bitmap, scaled for our bitmap size.
                    if (texturedata != null)
                    {
                        // Draw a scaled version of the texture to our image.
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.DrawImage(texturedata, drawX, drawY, tileWidth, tileHeight);

                            // If it's a pushwall make it red with a translucent red border around it.
                            if (mapdata.isTilePushable(y, x))
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

                        // Draw the actor on top of the tile.
                        if (tileActor >= 19 && tileActor <= 22) // Player Start
                        {
                            if (_isSOD)
                                renderSprite = 0;   // SOD doesn't have a player start sprite. Use the demo sprite.
                            else
                                renderSprite = 409;
                            playerSpawnHeight = y;
                            playerSpawnWidth = x;
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

                    //if (mapdata.isTileBlocked(y, x))
                    //{
                    //    // Draw a white border around the blocking tile.
                    //   using (Graphics g = Graphics.FromImage(bitmap))
                    //    {
                    //        g.DrawRectangle(new Pen(Color.White), drawX, drawY, tileWidth - 1, tileHeight - 1);
                    //    }
                    //}

                }
            }

            if (checkBox1.Checked)
            {
                pathfinder finder = new pathfinder(ref mapdata);

                finder.preparePathFinder();

                for (int x = 0; x < mapdata.getMapWidth(); x++)
                {
                    for (int y = 0; y < mapdata.getMapHeight(); y++)
                    {
                        if (finder.tileNodeWorthy(y, x))
                        {
                            int tileWidth = (int)((float)sizeWidth / mapdata.getMapWidth());
                            int tileHeight = (int)((float)sizeHeight / mapdata.getMapHeight());
                            int drawX = x * tileWidth;
                            int drawY = y * tileHeight;

                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                g.FillRectangle(new SolidBrush(Color.FromArgb(60, 0, 255, 0)), drawX, drawY, tileWidth, tileHeight);
                            }
                        }
                    }
                }
                /*pathfinder finder = new pathfinder(ref mapdata);

                if (checkBox2.Checked)
                    finder.ignorePushWalls = true;
                else
                    finder.ignorePushWalls = false;

                if (checkBox3.Checked)
                    finder.allSecrets = true;
                else
                    finder.allSecrets = false;

                finder.prepareBaseFloor();
                finder.setStart(playerSpawnHeight, playerSpawnWidth);
                finder.solveMaze();

                // Draw the pathfinder solution.
                for (int x = 0; x < mapdata.getMapWidth(); x++)
                {
                    for (int y = 0; y < mapdata.getMapHeight(); y++)
                    {
                        if (finder.isTileOnPath(y, x))
                        {
                            int tileWidth = (int)((float)sizeWidth / mapdata.getMapWidth());
                            int tileHeight = (int)((float)sizeHeight / mapdata.getMapHeight());
                            int drawX = x * tileWidth;
                            int drawY = y * tileHeight;

                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                g.FillRectangle(new SolidBrush(Color.FromArgb(60, 0, 255, 0)), drawX, drawY, tileWidth, tileHeight);
                            }
                        }
                    }
                }*/
            }


            return bitmap;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = this.rendercurrentLevel(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Refresh();

            button3.Enabled = true;

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

        private float _angle;

        private void button2_Click(object sender, EventArgs e)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new OpenTK.Mathematics.Vector2i(800, 600),
                Title = "OpenTK Window"
            };

            using (var game = new GameWindow(GameWindowSettings.Default, nativeWindowSettings))
            {
                game.Load += () =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;
                };

                game.Resize += (ResizeEventArgs args) =>
                {
                    GL.Viewport(0, 0, game.Size.X, game.Size.Y);
                };

                game.UpdateFrame += (FrameEventArgs args) =>
                {
                    // add game logic, input handling
                    _angle += 0.01f;
                };

                game.RenderFrame += (FrameEventArgs args) =>
                {
                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadIdentity();
                    GL.Rotate(_angle, Vector3.UnitZ);

                    GL.Begin(PrimitiveType.Triangles);

                    GL.Color3(Color.Red);
                    GL.Vertex2(-0.5f, -0.5f);
                    GL.Color3(Color.Green);
                    GL.Vertex2(0.5f, -0.5f);
                    GL.Color3(Color.Blue);
                    GL.Vertex2(0.0f, 0.5f);

                    GL.End();

                    game.SwapBuffers();
                };

                // Run the game at 60 updates per second
                game.Run();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (Image fullSizeImage = this.rendercurrentLevel(4096, 4096))
            {
                fullSizeImage.Save("level.png");
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}