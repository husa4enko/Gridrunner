using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Gridrunner;

public class Game1 : Game
{
    private const int TileSize = 32;
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;

    private readonly GraphicsDeviceManager _graphics;
    private readonly Arena _arena = new();

    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = WindowWidth,
            PreferredBackBufferHeight = WindowHeight
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Window.Title = "Gridrunner: Protocol 2099";
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // A 1×1 texture lets us draw colored rectangles.
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(10, 12, 24));

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        DrawArena();

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawArena()
    {
        int arenaWidth = Arena.Columns * TileSize;
        int arenaHeight = Arena.Rows * TileSize;
        int left = (WindowWidth - arenaWidth) / 2;
        int top = (WindowHeight - arenaHeight) / 2;

        for (int row = 0; row < Arena.Rows; row++)
        {
            for (int column = 0; column < Arena.Columns; column++)
            {
                var tileRectangle = new Rectangle(
                    left + column * TileSize,
                    top + row * TileSize,
                    TileSize,
                    TileSize);

                Color tileColor = _arena.GetTile(column, row) == TileType.Wall
                    ? new Color(190, 40, 120)
                    : new Color(17, 22, 38);

                _spriteBatch.Draw(_pixel, tileRectangle, tileColor);
            }
        }

        // Draw the grid lines over the tiles.
        Color gridColor = new Color(35, 170, 190);

        for (int column = 0; column <= Arena.Columns; column++)
        {
            int x = left + column * TileSize;
            DrawRectangle(new Rectangle(x, top, 1, arenaHeight), gridColor);
        }

        for (int row = 0; row <= Arena.Rows; row++)
        {
            int y = top + row * TileSize;
            DrawRectangle(new Rectangle(left, y, arenaWidth, 1), gridColor);
        }
    }

    private void DrawRectangle(Rectangle rectangle, Color color)
    {
        _spriteBatch.Draw(_pixel, rectangle, color);
    }
}
