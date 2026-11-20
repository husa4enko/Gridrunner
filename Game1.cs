using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Gridrunner;

public class Game1 : Game
{
    private const int TileSize = 32;
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;
    private const int SquadSize = 4;

    private readonly GraphicsDeviceManager _graphics;
    private readonly Arena _arena = new();
    private readonly List<Operative> _operatives = new();

    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;

    private OperativeClass _selectedClass = OperativeClass.CyberTank;
    private int _selectedOperativeIndex = -1;

    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;

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

        // A 1×1 texture used to draw tiles, units, and grid lines.
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
            Exit();

        // Number keys select an operative class.
        if (WasKeyJustPressed(keyboard, Keys.D1))
            _selectedClass = OperativeClass.CyberTank;
        if (WasKeyJustPressed(keyboard, Keys.D2))
            _selectedClass = OperativeClass.NeuralSniper;
        if (WasKeyJustPressed(keyboard, Keys.D3))
            _selectedClass = OperativeClass.SystemHacker;
        if (WasKeyJustPressed(keyboard, Keys.D4))
            _selectedClass = OperativeClass.AssaultOperative;
        if (WasKeyJustPressed(keyboard, Keys.D5))
            _selectedClass = OperativeClass.CombatMedic;
        if (WasKeyJustPressed(keyboard, Keys.D6))
            _selectedClass = OperativeClass.StealthInfiltrator;

        MouseState mouse = Mouse.GetState();

        bool clicked =
            mouse.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released;

        if (clicked)
            HandleBoardClick(mouse.X, mouse.Y);

        Window.Title =
            $"Gridrunner: Protocol 2099 | Class: {_selectedClass} | Squad: {_operatives.Count}/{SquadSize}";

        _previousMouseState = mouse;
        _previousKeyboardState = keyboard;

        base.Update(gameTime);
    }

    private bool WasKeyJustPressed(KeyboardState current, Keys key)
    {
        return current.IsKeyDown(key) &&
               !_previousKeyboardState.IsKeyDown(key);
    }

    private void HandleBoardClick(int mouseX, int mouseY)
    {
        if (!TryGetTileAt(mouseX, mouseY, out Point tile))
            return;

        // Clicking an operative selects it.
        for (int i = 0; i < _operatives.Count; i++)
        {
            if (_operatives[i].Tile == tile)
            {
                _selectedOperativeIndex = i;
                return;
            }
        }

        // If an operative is selected, try to move it one tile.
        if (_selectedOperativeIndex >= 0 &&
            _selectedOperativeIndex < _operatives.Count)
        {
            Operative selected = _operatives[_selectedOperativeIndex];

            int distance =
                Math.Abs(selected.Tile.X - tile.X) +
                Math.Abs(selected.Tile.Y - tile.Y);

            // Movement must be exactly one tile, up/down/left/right.
            if (distance != 1)
                return;

            // Units can only move onto floor tiles.
            if (_arena.GetTile(tile.X, tile.Y) != TileType.Floor)
                return;

            // Don't allow moving onto another operative.
            foreach (Operative other in _operatives)
            {
                if (other != selected && other.Tile == tile)
                    return;
            }

            selected.Tile = tile;
            _selectedOperativeIndex = -1;
            return;
        }

        // If no operative is selected, place one on the left half.
        if (_operatives.Count >= SquadSize ||
            tile.X >= Arena.Columns / 2 ||
            _arena.GetTile(tile.X, tile.Y) != TileType.Floor)
        {
            return;
        }

        _operatives.Add(new Operative(_selectedClass, tile));
    }

    private bool TryGetTileAt(int mouseX, int mouseY, out Point tile)
    {
        int arenaWidth = Arena.Columns * TileSize;
        int arenaHeight = Arena.Rows * TileSize;
        int left = (WindowWidth - arenaWidth) / 2;
        int top = (WindowHeight - arenaHeight) / 2;

        int x = (mouseX - left) / TileSize;
        int y = (mouseY - top) / TileSize;

        bool insideArena =
            mouseX >= left &&
            mouseY >= top &&
            x >= 0 && x < Arena.Columns &&
            y >= 0 && y < Arena.Rows;

        tile = insideArena ? new Point(x, y) : Point.Zero;
        return insideArena;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(10, 12, 24));

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        DrawArena();
        DrawOperatives();
        DrawHud();

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawArena()
    {
        int arenaWidth = Arena.Columns * TileSize;
        int arenaHeight = Arena.Rows * TileSize;
        int left = (WindowWidth - arenaWidth) / 2;
        int top = (WindowHeight - arenaHeight) / 2;

        // Draw floor and wall tiles.
        for (int row = 0; row < Arena.Rows; row++)
        {
            for (int column = 0; column < Arena.Columns; column++)
            {
                var rectangle = new Rectangle(
                    left + column * TileSize,
                    top + row * TileSize,
                    TileSize,
                    TileSize);

                Color color = _arena.GetTile(column, row) == TileType.Wall
                    ? new Color(190, 40, 120)
                    : new Color(17, 22, 38);

                _spriteBatch.Draw(_pixel, rectangle, color);
            }
        }

        // Draw grid lines.
        Color gridColor = new Color(35, 170, 190);

        for (int column = 0; column <= Arena.Columns; column++)
        {
            int x = left + column * TileSize;
            DrawRectangle(
                new Rectangle(x, top, 1, arenaHeight),
                gridColor);
        }

        for (int row = 0; row <= Arena.Rows; row++)
        {
            int y = top + row * TileSize;
            DrawRectangle(
                new Rectangle(left, y, arenaWidth, 1),
                gridColor);
        }
    }

    private void DrawOperatives()
    {
        int left = (WindowWidth - Arena.Columns * TileSize) / 2;
        int top = (WindowHeight - Arena.Rows * TileSize) / 2;

        for (int i = 0; i < _operatives.Count; i++)
        {
            Operative operative = _operatives[i];

            var rectangle = new Rectangle(
                left + operative.Tile.X * TileSize + 5,
                top + operative.Tile.Y * TileSize + 5,
                TileSize - 10,
                TileSize - 10);

            _spriteBatch.Draw(
                _pixel,
                rectangle,
                GetClassColor(operative.Class));

            // Draw a white border around the selected operative.
            if (i == _selectedOperativeIndex)
                DrawOutline(rectangle, Color.White);
        }
    }

    private void DrawHud()
    {
        int arenaWidth = Arena.Columns * TileSize;
        int arenaHeight = Arena.Rows * TileSize;
        int left = (WindowWidth - arenaWidth) / 2;
        int top = (WindowHeight - arenaHeight) / 2;

        // Marker showing the currently selected class color.
        DrawRectangle(
            new Rectangle(left, top - 24, 20, 12),
            GetClassColor(_selectedClass));

        // Squad slots: filled slots show unit colors, empty slots are gray.
        for (int i = 0; i < SquadSize; i++)
        {
            Color color = i < _operatives.Count
                ? GetClassColor(_operatives[i].Class)
                : new Color(55, 60, 75);

            DrawRectangle(
                new Rectangle(left + 30 + i * 24, top - 24, 18, 12),
                color);
        }
    }

    private void DrawOutline(Rectangle rectangle, Color color)
    {
        DrawRectangle(
            new Rectangle(rectangle.X - 2, rectangle.Y - 2,
                rectangle.Width + 4, 2),
            color);

        DrawRectangle(
            new Rectangle(rectangle.X - 2, rectangle.Bottom,
                rectangle.Width + 4, 2),
            color);

        DrawRectangle(
            new Rectangle(rectangle.X - 2, rectangle.Y,
                2, rectangle.Height),
            color);

        DrawRectangle(
            new Rectangle(rectangle.Right, rectangle.Y,
                2, rectangle.Height),
            color);
    }

    private static Color GetClassColor(OperativeClass operativeClass)
    {
        return operativeClass switch
        {
            OperativeClass.CyberTank => new Color(255, 170, 40),
            OperativeClass.NeuralSniper => new Color(80, 180, 255),
            OperativeClass.SystemHacker => new Color(190, 90, 255),
            OperativeClass.AssaultOperative => new Color(255, 70, 70),
            OperativeClass.CombatMedic => new Color(80, 255, 140),
            OperativeClass.StealthInfiltrator => new Color(230, 230, 255),
            _ => Color.White
        };
    }

    private void DrawRectangle(Rectangle rectangle, Color color)
    {
        _spriteBatch.Draw(_pixel, rectangle, color);
    }
}

public enum OperativeClass
{
    CyberTank,
    NeuralSniper,
    SystemHacker,
    AssaultOperative,
    CombatMedic,
    StealthInfiltrator
}

public class Operative
{
    public OperativeClass Class { get; }
    public Point Tile { get; set; }

    public Operative(OperativeClass operativeClass, Point tile)
    {
        Class = operativeClass;
        Tile = tile;
    }
}
