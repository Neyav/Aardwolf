using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using AardwolfCore;
using AardwolfCore.Actors;
using AardwolfCore.Actors.Enemies;
using AardwolfCore.Animation;
using System.Runtime.CompilerServices;

namespace Aardwolf.Render
{
    public class RenderEngine
    {

        private float _angle;
        private gamesession _gamesession;
        private int _shaderprogram;
        private VAO cubevao, quadvao;
        private VBO cubevbo, quadvbo;
        private EBO cubeebo, quadebo;
        private Camera camera;
        private int DoorTexture;
        SystemActor systemActor = new SystemActor(false);
        private int[] textures;
        private int[] sprites;
        maphandler mapdata;
        RGBA CeilingColor;
        RGBA FloorColor;
        bool readyToRender;
        dataHandler _gameData;

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

            string fragmentShaderSource = @"#version 330 core

in vec3 ourColor;
in vec2 TexCoord;

out vec4 FragColor;

uniform sampler2D ourTexture;
uniform vec3 solidColor;
uniform bool useSolidColor;

void main()
{
    if (useSolidColor)
    {
        // Render using the RGB color
        FragColor = vec4(solidColor, 1.0);
    }
    else
    {
        // Render using the texture
        FragColor = texture(ourTexture, TexCoord);

        //if (!gl_FrontFacing) 
        //{
        //    FragColor.a *= 0.3;
        //}
    }

    // Optionally discard fully transparent fragments
    if (FragColor.a == 0.0) 
    { 
        discard;
    }
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
        int LoadTexture(Bitmap bitmap)
        {
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // Load the bitmap data into the texture
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);

            // Set texture filtering to nearest neighbor
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // Set texture wrapping to clamp to edge to prevent texture bleeding
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2D, 0); // Unbind the texture

            return textureID;
        }


        void renderTileSurface(float _x, float _y, float _z, RGBA _tileColour)
        {
            quadvao.Bind();
            quadvbo.Bind();
            quadebo.Bind();

            Vector3 tilePosition = new Vector3(_x, _y, _z);
            Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-90.0f)) * Matrix4.CreateTranslation(tilePosition);

            int modelLoc = GL.GetUniformLocation(_shaderprogram, "model");
            GL.UniformMatrix4(modelLoc, false, ref model);

            int solidColorLoc = GL.GetUniformLocation(_shaderprogram, "solidColor");
            int useSolidColorLoc = GL.GetUniformLocation(_shaderprogram, "useSolidColor");

            // Set the solid color
            GL.Uniform3(solidColorLoc, new Vector3(_tileColour.r / 255.0f, _tileColour.g / 255.0f, _tileColour.b / 255.0f));

            // Enable solid color rendering
            bool useSolidColor = true;
            GL.Uniform1(useSolidColorLoc, useSolidColor ? 1 : 0);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        void renderDoorTile(dynamicMapObject doorObject)
        {
            if (doorObject.type != mapObjectTypes.MAPOBJECT_DOOR)
                return;

            Vector3 doorTilePosition = new Vector3(doorObject.poswidth, 0, doorObject.posheight);
            Matrix4 model;
            Matrix4 rotation = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(90));

            if (doorObject.activatedDirection == mapDirection.DIR_EAST)
                model = Matrix4.CreateTranslation(doorTilePosition);
            else
                model = rotation * Matrix4.CreateTranslation(doorTilePosition);

            int modelLoc = GL.GetUniformLocation(_shaderprogram, "model");
            GL.UniformMatrix4(modelLoc, false, ref model);

            int useSolidColorLoc = GL.GetUniformLocation(_shaderprogram, "useSolidColor");

            // Disable solid color rendering
            bool useSolidColor = false;
            GL.Uniform1(useSolidColorLoc, useSolidColor ? 1 : 0);

            // Bind the texture
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textures[DoorTexture]);

            // Set the texture uniform
            int texLocation = GL.GetUniformLocation(_shaderprogram, "ourTexture");
            GL.Uniform1(texLocation, 0);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        void renderSprite(float _x, float _y, float _z, int _spritetexture, Camera camera)
        {
            quadvao.Bind();
            quadvbo.Bind();
            quadebo.Bind();

            // The renderer considers _x, _z to be dead center as an integer.
            // The game engine considers _x, _z to start at the corner of the tile, and proceed to the other corner.
            // Transfer it to the appropriate position for rendering.
            Vector3 spritePosition = new Vector3(_x - 0.5f, _y, _z - 0.5f);

            // Use the camera's forward vector to calculate the yaw angle
            Vector3 forward = camera.Front;

            // Calculate the yaw angle based on the forward vector
            float spriteYaw = MathF.Atan2(forward.X, forward.Z);

            // Create the rotation matrix using the calculated yaw angle
            Matrix4 rotation = Matrix4.CreateRotationY(spriteYaw);

            // Combine the rotation and translation matrices to create the model matrix
            Matrix4 model = rotation * Matrix4.CreateTranslation(spritePosition);

            int modelLoc = GL.GetUniformLocation(_shaderprogram, "model");
            GL.UniformMatrix4(modelLoc, false, ref model);

            int useSolidColorLoc = GL.GetUniformLocation(_shaderprogram, "useSolidColor");

            // Disable solid color rendering
            bool useSolidColor = false;
            GL.Uniform1(useSolidColorLoc, useSolidColor ? 1 : 0);

            // Bind the texture
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, sprites[_spritetexture]);

            // Set the texture uniform
            int texLocation = GL.GetUniformLocation(_shaderprogram, "ourTexture");
            GL.Uniform1(texLocation, 0);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }


        void render3DTexturedCube(float _x, float _y, float _z, int _tiletexture, mapDirection _doorAdjacent, mapDirection _wallAdjacent)
        {
            Vector3 CubePos = new Vector3(_x, _y, _z);
            Matrix4 model = Matrix4.CreateTranslation(CubePos);

            int modelLoc = GL.GetUniformLocation(_shaderprogram, "model");
            GL.UniformMatrix4(modelLoc, false, ref model);

            int _textureEastWest = _tiletexture * 2 - 1;
            int _textureNorthSouth = _tiletexture * 2 - 2;

            cubevao.Bind();  // Bind VAO
            cubevbo.Bind();
            cubeebo.Bind();

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

                    int useSolidColorLoc = GL.GetUniformLocation(_shaderprogram, "useSolidColor");

                    // Disable solid color rendering
                    GL.Uniform1(useSolidColorLoc, 0);

                    // Draw the face
                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, new IntPtr(face * 6 * sizeof(uint)));
                }
            }
        }

        private void generateQuad()
        {
            float[] vertices = {
    // Positions        // Colors         // Texture Coords
     0.5f,  0.5f, 0.0f,  1.0f, 1.0f, 1.0f,  0.0f, 0.0f, // Top-right
     0.5f, -0.5f, 0.0f,  1.0f, 1.0f, 1.0f,  0.0f, 1.0f, // Bottom-right
    -0.5f, -0.5f, 0.0f,  1.0f, 1.0f, 1.0f,  1.0f, 1.0f, // Bottom-left
    -0.5f,  0.5f, 0.0f,  1.0f, 1.0f, 1.0f,  1.0f, 0.0f  // Top-left
};

            uint[] indices = {
    0, 1, 3, // First triangle
    1, 2, 3  // Second triangle
};

            quadvao = new VAO();

            quadvbo = new VBO();
            quadvbo.Bind(BufferTarget.ArrayBuffer);
            quadvbo.SetData(vertices, BufferUsageHint.StaticDraw);


            quadebo = new EBO();
            quadebo.SetData(indices, BufferUsageHint.StaticDraw);

            quadvao.SetAttributes();

        }
        private void generateCube()
        {
            float[] vertices = {
    // Positions          // Colors          // Texture Coords
    -0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f, // Front face
     0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f,
     0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f,
    -0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,

    -0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f, // Back face
     0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f,
     0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,
    -0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f,

    -0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f, // Top face
     0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f,
     0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,
    -0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f,

    -0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f, // Bottom face
     0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f,
     0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f,
    -0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,

    -0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f, // Left face
    -0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,
    -0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f,
    -0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f,

     0.5f, -0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f, // Right face
     0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,
     0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f,
     0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f
};

            uint[] indices = {
    0,  1,  2,  2,  3,  0, // Front face
    4,  5,  6,  6,  7,  4, // Back face
    8,  9, 10, 10, 11,  8, // Top face
    12, 13, 14, 14, 15, 12, // Bottom face
    16, 17, 18, 18, 19, 16, // Left face
    20, 21, 22, 22, 23, 20  // Right face
};


            cubevao = new VAO();

            cubevbo = new VBO();
            cubevbo.Bind(BufferTarget.ArrayBuffer);
            cubevbo.SetData(vertices, BufferUsageHint.StaticDraw);


            cubeebo = new EBO();
            cubeebo.SetData(indices, BufferUsageHint.StaticDraw);

            cubevao.SetAttributes();
        }

        public bool initalizeLevel(dataHandler gameData, int levelNumber)
        {
            readyToRender = false;

            mapdata = new maphandler(gameData.isSpearOfDestinyData());
            mapdata.importMapData(gameData.getLevelData(levelNumber), gameData.levelHeight(levelNumber), gameData.levelWidth(levelNumber));

            if (!mapdata.isMapLoaded())
                return false;

            for (int x = 0; x < mapdata.getMapWidth(); x++)
            {
                for (int y = 0; y < mapdata.getMapHeight(); y++)
                {
                    int tileActor = 0;

                    if ((tileActor = gameData.getTileActor(levelNumber, x, y)) != 0)
                    {
                        mapdata.defineMapObject(tileActor, y, x);
                    }
                }
            }

            CeilingColor = gameData.returnVGACeilingColor(levelNumber);
            FloorColor = gameData.returnVGAFloorColor();

            textures = new int[gameData.numberOfTextures()];
            sprites = new int[gameData.numberOfSprites()];
            DoorTexture = gameData.getDoorTextureNumber();
            _gameData = gameData;

            readyToRender = true;

            return true;
        }

        public void startRenderLoop()
        {
            if (!readyToRender)
                return;

            Vector2 lastMousePosition = (0, 0);

            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new OpenTK.Mathematics.Vector2i(800, 600),
                Title = "Aardwolf Test"
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
                    // Enable blending
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                    // Shader program
                    _shaderprogram = CreateShaderProgram();

                    // Framebuffer setup
                    framebuffer = GL.GenFramebuffer();
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

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

                    // Load the textures.
                    for (int i = 0; i < _gameData.numberOfTextures(); i++)
                    {
                        textures[i] = LoadTexture(_gameData.getTexture(i));
                    }

                    for (int i = 0; i < _gameData.numberOfSprites(); i++)
                    {
                        sprites[i] = LoadTexture(_gameData.getSprite(i));
                    }

                    generateCube();
                    generateQuad();

                    GL.BindVertexArray(0); // Unbind VAO                    

                    CheckOpenGLError();

                    // Initialize the camera
                    Vector3 cameraPosition = new Vector3(mapdata.playerSpawnWidth, 0.0f, mapdata.playerSpawnHeight);
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
                            else
                            {
                                renderTileSurface(widthIterator, -0.5f, heightIterator, FloorColor);
                                // Render the ceiling but only if the camera isn't above the ceiling height.
                                if (camera.Position.Y < 0.5f)
                                    renderTileSurface(widthIterator, 0.5f, heightIterator, CeilingColor);

                                int staticObjID = mapdata.getStaticObjectID(heightIterator, widthIterator);
                                dynamicMapObject dynamicObject = mapdata.getDoorObject(heightIterator, widthIterator);

                                if (staticObjID > 0)
                                {
                                    renderSprite(widthIterator + 0.5f, 0, heightIterator + 0.5f, staticObjID - 21, camera);
                                }
                                else if (dynamicObject.type == mapObjectTypes.MAPOBJECT_DOOR)
                                {
                                    renderDoorTile(dynamicObject);
                                }

                                AnimatedActor tileActor = mapdata.getActorAtPosition(heightIterator, widthIterator);
                                if (tileActor != null)
                                {
                                    // Render the animated actor
                                    renderSprite(widthIterator + 0.5f, 0, heightIterator + 0.5f, tileActor.getAnimationFrame(camera.Yaw, args.Time), camera);

                                }
                            }
                        }
                    }

                    GL.BindVertexArray(0);  // Unbind VAO

                    game.SwapBuffers();
                };


                game.Run();
            }

        }

        public RenderEngine()
        {
            readyToRender = false;
        }

        ~RenderEngine()
        {

        }
    }
}
