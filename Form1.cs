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
using AardwolfCore;
using System.Drawing.Drawing2D;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;



namespace Aardwolf
{
    public partial class Form1 : Form
    {
        dataHandler dh = new dataHandler();
        private int _shaderprogram;
        private int cubevao, cubevbo, cubeebo;
        private int quadvao, quadvbo, quadebo;
        private int[] textures;
        private Camera camera;
        private int DoorTexture;

        maphandler mapdata;

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
            VSWAPHeader VSWAPHead = dh.getVSWAPHeader;
            byte doorWall = (byte)(VSWAPHead.spriteStart - 8);

            if (radioButton2.Checked)
                _isSOD = true;

            mapdata = new maphandler(_isSOD);
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
                    int texture = -1;
                    // Setup render location and size for any future drawing.
                    int tileWidth = (int)((float)sizeWidth / mapdata.getMapWidth());
                    int tileHeight = (int)((float)sizeHeight / mapdata.getMapHeight());
                    int drawX = x * tileWidth;
                    int drawY = y * tileHeight;
                    Bitmap texturedata = null;

                    if (leveldata > 0)
                        texture = (leveldata - 1) * 2;

                    if (mapdata.isDoorOpenable(y, x, false, false))
                        texture = doorWall;
                    else if (mapdata.isDoorOpenable(y, x, true, false))
                        texture = doorWall + 6;
                    else if (mapdata.isDoorOpenable(y, x, false, true))
                        texture = doorWall + 6;

                    if (texture >= 0)
                    {
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

            if (checkBox1.Checked || checkBox4.Checked )
            {
                pathfinder finder = new pathfinder(ref mapdata);

                if (checkBox2.Checked)
                    finder.ignorePushWalls = true;

                finder.preparePathFinder();

                if (checkBox1.Checked)
                {
                    if (!finder.solveMaze())
                    {
                        // Draw a BIG X over the map to indicate path failure.
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.DrawLine(new Pen(Color.Red, 5), 0, 0, sizeWidth, sizeHeight);
                            g.DrawLine(new Pen(Color.Red, 5), sizeWidth, 0, 0, sizeHeight);
                        }
                    }
                }

                if (checkBox4.Checked) // Display the nodes and their connections for debugging.
                {
                    for (int x = 0; x < mapdata.getMapWidth(); x++)
                    {
                        for (int y = 0; y < mapdata.getMapHeight(); y++)
                        {
                            if (finder.returnNode(y, x) != null)
                            {
                                int tileWidth = (int)((float)sizeWidth / mapdata.getMapWidth());
                                int tileHeight = (int)((float)sizeHeight / mapdata.getMapHeight());
                                int drawX = x * tileWidth;
                                int drawY = y * tileHeight;

                                using (Graphics g = Graphics.FromImage(bitmap))
                                {
                                    g.FillRectangle(new SolidBrush(Color.FromArgb(60, 0, 255, 0)), drawX, drawY, tileWidth, tileHeight);
                                }

                                List<pathNode> pathNodes = finder.returnConnectedNodes(y, x);

                                // Draw a line between the center of the current tile and the center of the connected tile.
                                foreach (pathNode node in pathNodes)
                                {
                                    int drawX2 = node.widthPosition * tileWidth + (tileWidth / 2);
                                    int drawY2 = node.heightPosition * tileHeight + (tileHeight / 2);

                                    using (Graphics g = Graphics.FromImage(bitmap))
                                    {
                                        g.DrawLine(new Pen(Color.FromArgb(50, 0, 255, 0)), drawX + (tileWidth / 2), drawY + (tileHeight / 2), drawX2, drawY2);
                                    }
                                }
                            }

                        }
                    }
                }

                if (checkBox5.Checked)
                {
                    List<pathNode> traversableNodes = finder.returnTraversableNodes();
                    bool completed = false;

                    // For each Node draw a red circle at the center of the tile.
                    foreach (pathNode node in traversableNodes)
                    {
                        int tileWidth = (int)((float)sizeWidth / mapdata.getMapWidth());
                        int tileHeight = (int)((float)sizeHeight / mapdata.getMapHeight());
                        int drawX = node.widthPosition * tileWidth + (tileWidth / 2);
                        int drawY = node.heightPosition * tileHeight + (tileHeight / 2);                        

                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.FillEllipse(new SolidBrush(Color.FromArgb(100, 255, 0, 0)), drawX - 15, drawY - 15, 30, 30);

                            if (node.endPoint)
                                completed = true;                            

                            // Get the connected nodes for this node.
                            List<pathNode> connectedNodes = finder.returnConnectedNodes(node.heightPosition, node.widthPosition);

                            // Draw a line between the center of the current tile and the center of the connected tile.
                            foreach (pathNode connectedNode in connectedNodes)
                            {
                                int drawX2 = connectedNode.widthPosition * tileWidth + (tileWidth / 2);
                                int drawY2 = connectedNode.heightPosition * tileHeight + (tileHeight / 2);

                                g.DrawLine(new Pen(Color.FromArgb(100, 255, 0, 0), 2), drawX, drawY, drawX2, drawY2);
                            }
                        }
                    }

                    if (completed)
                    {
                        // Write confirmed in all black on the top left of the image.
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.DrawString("Confirmed", new Font("Arial", 24), new SolidBrush(Color.Yellow), 0, 0);
                        }

                    }
                }

                // Draw the pathfinder solution.
                List<pathNode> shortestRoute = finder.returnRoute();

                Debug.WriteLine("Shortest route length: " + shortestRoute.Count);

                for (int i = 0; i < shortestRoute.Count - 1; i++)
                {
                    pathNode node = shortestRoute[i];
                    pathNode nextNode = shortestRoute[i + 1];

                    // Draw a thick red line between the center of the current tile and the center of the connected tile.
                    int drawX = node.widthPosition * (int)((float)sizeWidth / mapdata.getMapWidth()) + ((int)((float)sizeWidth / mapdata.getMapWidth()) / 2);
                    int drawY = node.heightPosition * (int)((float)sizeHeight / mapdata.getMapHeight()) + ((int)((float)sizeHeight / mapdata.getMapHeight()) / 2);
                    int drawX2 = nextNode.widthPosition * (int)((float)sizeWidth / mapdata.getMapWidth()) + ((int)((float)sizeWidth / mapdata.getMapWidth()) / 2);
                    int drawY2 = nextNode.heightPosition * (int)((float)sizeHeight / mapdata.getMapHeight()) + ((int)((float)sizeHeight / mapdata.getMapHeight()) / 2);

                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawLine(new Pen(Color.FromArgb(100, 0, 0), 5), drawX, drawY, drawX2, drawY2);
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

        private int CreateShaderProgram()
        {
            string vertexShaderSource = @"
    #version 330 core

layout(location = 0) in vec3 aPos;       // The position variable has attribute position 0
layout(location = 1) in vec3 aColor;     // The color variable has attribute position 1
layout(location = 2) in vec2 aTexCoord;  // The texture coordinate attribute has position 2

out vec3 ourColor;      // Output a color to the fragment shader
out vec2 TexCoord;      // Output the texture coordinates to the fragment shader

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = projection * view * model * vec4(aPos, 1.0);
    ourColor = aColor;
    TexCoord = aTexCoord;
}";

            string fragmentShaderSource = @"
    #version 330 core

out vec4 FragColor;

in vec3 ourColor;
in vec2 TexCoord;

uniform sampler2D ourTexture;

void main()
{
    FragColor = texture(ourTexture, TexCoord) * vec4(ourColor, 1.0);
}";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompile(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompile(fragmentShader);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            CheckProgramLink(program);

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }

        void CheckShaderCompile(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Shader compile error: {infoLog}");
            }
        }

        void CheckProgramLink(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Program link error: {infoLog}");
            }
        }

        void CheckOpenGLError()
        {
            ErrorCode err;
            while ((err = GL.GetError()) != ErrorCode.NoError)
            {
                Console.WriteLine("OpenGL Error: " + err);
            }
        }
        private int LoadTexture(Bitmap bitmap)
        {
            int texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                       System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0,
                          PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texture;
        }

        void render3DTexturedCube(float _x, float _y, float _z, int _tiletexture, mapDirection _doorAdjacent, mapDirection _wallAdjacent)
        {
            Vector3 CubePos = new Vector3(_x, _y, _z);
            Matrix4 model = Matrix4.CreateTranslation(CubePos);

            int modelLoc = GL.GetUniformLocation(_shaderprogram, "model");
            GL.UniformMatrix4(modelLoc, false, ref model);

            int _textureEastWest = _tiletexture * 2 - 1;
            int _textureNorthSouth = _tiletexture * 2 - 2;

            GL.BindVertexArray(cubevao);  // Bind VAO

            // Draw each face with its respective texture
            for (int face = 0; face < 6; ++face)
            {
                int textureToBind = -1;

                // Determine the texture to bind based on the face index
                switch (face)
                {
                    case 4: // West face
                        if ((_wallAdjacent & mapDirection.DIR_WEST) == mapDirection.DIR_WEST)
                            break;
                        if ((_doorAdjacent & mapDirection.DIR_WEST) == mapDirection.DIR_WEST)
                        {
                            textureToBind = DoorTexture + 3;
                            break;
                        }
                        else
                        {
                            textureToBind = _textureEastWest;
                            break;
                        }
                    case 5: // East Face
                        if ((_wallAdjacent & mapDirection.DIR_EAST) == mapDirection.DIR_EAST)
                            break;
                        if ((_doorAdjacent & mapDirection.DIR_EAST) == mapDirection.DIR_EAST)
                        {
                            textureToBind = DoorTexture + 3;
                            break;
                        }
                        else
                        {
                            textureToBind = _textureEastWest;
                            break;
                        }
                        break;
                    case 0: // North Face
                        if ((_wallAdjacent & mapDirection.DIR_NORTH) == mapDirection.DIR_NORTH)
                            break;
                        if ((_doorAdjacent & mapDirection.DIR_NORTH) == mapDirection.DIR_NORTH)
                        {
                            textureToBind = DoorTexture + 2;
                            break;
                        }
                        else
                        {
                            textureToBind = _textureNorthSouth;
                            break;
                        }                        
                    case 1: // South Face
                        if ((_wallAdjacent & mapDirection.DIR_SOUTH) == mapDirection.DIR_SOUTH)
                            break;
                        if ((_doorAdjacent & mapDirection.DIR_SOUTH) == mapDirection.DIR_SOUTH)
                        {
                            textureToBind = DoorTexture + 2;
                            break;
                        }
                        else
                        {
                            textureToBind = _textureNorthSouth;
                            break;
                        }
                    default:
                        textureToBind = -1;
                        break;
                        textureToBind = _textureNorthSouth; // Use the base texture for top and bottom
                        break;
                }

                if (textureToBind >= 0)
                {
                    // Bind the texture
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, textures[textureToBind]);

                    // Set the texture uniform
                    int texLocation = GL.GetUniformLocation(_shaderprogram, "ourTexture");
                    GL.Uniform1(texLocation, 0);

                    // Draw the face
                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, new IntPtr(face * 6 * sizeof(uint)));
                }
            }
        }

        private void generateCube()
        {
            float[] vertices = {
    // Positions          // Colors          // Texture Coords
    -0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f, // Front face
     0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f,
     0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,
    -0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f,

    -0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f, // Back face
     0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f,
     0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,
    -0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f,

    -0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f, // Top face
     0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f,
     0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,
    -0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f,

    -0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f, // Bottom face
     0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f,
     0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,
    -0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f,

    -0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f, // Left face
    -0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f,
    -0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,
    -0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f,

     0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f, // Right face
     0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f,
     0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,
     0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f
};

            uint[] indices = {
    0,  1,  2,  2,  3,  0, // Front face
    4,  5,  6,  6,  7,  4, // Back face
    8,  9, 10, 10, 11,  8, // Top face
    12, 13, 14, 14, 15, 12, // Bottom face
    16, 17, 18, 18, 19, 16, // Left face
    20, 21, 22, 22, 23, 20  // Right face
};


            cubevao = GL.GenVertexArray();
            GL.BindVertexArray(cubevao);

            cubevbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, cubevbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            cubeebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, cubeebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            // Color attribute
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            // Texture coord attribute
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textures = new int[dh.numberOfTextures()];
            DoorTexture = dh.getDoorTextureNumber();
            float frameView = 14.0f;            
            Vector2 lastMousePosition = (0,0);

            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new OpenTK.Mathematics.Vector2i(800, 600),
                Title = "OpenTK Window"
            };           

            // Usage in your Load event
            using (var game = new GameWindow(GameWindowSettings.Default, nativeWindowSettings))
            {
                game.Load += () =>
                {
                    int framebuffer;

                    lastMousePosition = game.MousePosition;

                    GL.Enable(EnableCap.DepthTest);
                    //GL.Enable(EnableCap.CullFace);
                    //GL.FrontFace(FrontFaceDirection.Ccw); // Counter-clockwise defined as front

                    // Shader program
                    _shaderprogram = CreateShaderProgram();

                    // Framebuffer setup
                    framebuffer = GL.GenFramebuffer();
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

                    for (int i = 0; i < dh.numberOfTextures(); i++)
                    {
                        textures[i] = LoadTexture(dh.getTexture(i));
                    }

                    int texture = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, texture);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 800, 600, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);

                    if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                    {
                        throw new Exception("Framebuffer is not complete!");
                    }

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                    generateCube();

                    GL.BindVertexArray(0); // Unbind VAO

                    using (var bitmap = dh.getTexture(10))
                    {
                        LoadTexture(bitmap); // Load texture from in-memory Bitmap
                    }

                    CheckOpenGLError();

                    // Initialize the camera
                    Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
                    Vector3 worldUp = Vector3.UnitY;
                    float yaw = -90.0f;
                    float pitch = 0.0f;
                    camera = new Camera(cameraPosition, worldUp, yaw, pitch);

                    game.VSync = VSyncMode.On;
                };

                game.Resize += (ResizeEventArgs args) =>
                {
                    GL.Viewport(0, 0, game.Size.X, game.Size.Y);
                };

                game.UpdateFrame += (FrameEventArgs args) =>
                {
                    float deltaTime = (float)args.Time;

                    // Process keyboard input for camera movement
                    var keyboardState = game.KeyboardState;

                    if (keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W))
                        camera.ProcessKeyboard(Direction.Forward, deltaTime);
                    if (keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S))
                        camera.ProcessKeyboard(Direction.Backward, deltaTime);
                    if (keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
                        camera.ProcessKeyboard(Direction.Left, deltaTime);
                    if (keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D))
                        camera.ProcessKeyboard(Direction.Right, deltaTime);
                    if (keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
                        game.CursorState = CursorState.Normal;

                    // Process mouse movement for camera rotation
                    var mouseState = game.MouseState;
                    Vector2 mousePosition = mouseState.Position;
                    float xOffset = mousePosition.X - lastMousePosition.X;
                    float yOffset = lastMousePosition.Y - mousePosition.Y; // Reversed since y-coordinates range from bottom to top
                    lastMousePosition = mousePosition;

                    camera.ProcessMouseMovement(xOffset, yOffset);
                };

                game.MouseWheel += (MouseWheelEventArgs args) =>
                {
                    float yOffset = args.OffsetY;
                    camera.ProcessMouseScroll(yOffset);
                };

                game.MouseDown += (MouseButtonEventArgs args) =>
                {
                    if (args.Button == MouseButton.Left)
                    {
                        game.CursorState = CursorState.Grabbed;
                    }
                };

                game.RenderFrame += (FrameEventArgs args) =>
                {
                    //GL.ClearColor(Color.CornflowerBlue);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    GL.UseProgram(_shaderprogram);

                    // Bind the texture
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, textures[45]);

                    // Set the texture uniform
                    int textureLocation = GL.GetUniformLocation(_shaderprogram, "ourTexture");
                    GL.Uniform1(textureLocation, 0);

                    // Set the model, view, and projection matrices
                    Matrix4 view = camera.GetViewMatrix(); 
                    Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.Zoom), game.Size.X / (float)game.Size.Y, 0.1f, 100.0f);

                    int viewLoc = GL.GetUniformLocation(_shaderprogram, "view");
                    int projectionLoc = GL.GetUniformLocation(_shaderprogram, "projection");

                    GL.UniformMatrix4(viewLoc, false, ref view);
                    GL.UniformMatrix4(projectionLoc, false, ref projection);

                    for (int widthIterator = 0; widthIterator < mapdata.getMapWidth(); widthIterator++)
                    {
                        for (int heightIterator = 0; heightIterator < mapdata.getMapHeight(); heightIterator++)
                        {
                            int tiledata = mapdata.getTileData(heightIterator, widthIterator);
                            if (tiledata != 0)
                                render3DTexturedCube(widthIterator, 0, heightIterator, tiledata,
                                    mapdata.isTileDoorAdjacent(heightIterator, widthIterator),
                                    mapdata.adjacentBlockingTiles(heightIterator, widthIterator));
                        }
                    }

                    GL.BindVertexArray(0);  // Unbind VAO

                    game.SwapBuffers();
                };


                game.Run();
            }


        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Enabled = true;
                checkBox3.Enabled = true;
            }
            else
            {
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                checkBox2.Enabled = false;
                checkBox3.Enabled = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (System.Drawing.Image fullSizeImage = this.rendercurrentLevel(4096, 4096))
            {
                fullSizeImage.Save("level.png");
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                checkBox5.Enabled = true;
            }
            else
            {
                checkBox5.Enabled = false;
                checkBox5.Checked = false;
            }
        }
    }
}