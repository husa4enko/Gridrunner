
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
    private readonly List<Enemy> _enemies = new();

    private readonly EnemyAI _enemyAI;

    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;

    private OperativeClass _selectedClass = OperativeClass.CyberTank;
    private int _selectedOperativeIndex = -1;

    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;

    private bool _enemiesSpawned;
    private bool _gameFinished;
    private bool _victory;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = WindowWidth,
            PreferredBackBufferHeight = WindowHeight
        };

        _enemyAI = new EnemyAI(_arena);

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

        // A 1x1 texture used to draw tiles and units.
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
            Exit();

        if (_gameFinished)
        {
            _previousKeyboardState = keyboard;
            return;
        }

        // Select operative classes.
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

        // Space activates the enemy turn.
        if (_enemiesSpawned &&
            WasKeyJustPressed(keyboard, Keys.Space))
        {
            EndPlayerTurn();
        }

        MouseState mouse = Mouse.GetState();

        bool clicked =
            mouse.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released;

        if (clicked && !_gameFinished)
            HandleBoardClick(mouse.X, mouse.Y);

        UpdateWindowTitle();

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

        // Check whether the clicked tile contains an enemy.
        foreach (Enemy enemy in _enemies)
        {
            if (enemy.Tile == tile)
            {
                AttackEnemy(enemy);
                return;
            }
        }

        // Clicking an operative selects it.
        for (int i = 0; i < _operatives.Count; i++)
        {
            if (_operatives[i].Tile == tile)
            {
                _selectedOperativeIndex = i;
                return;
            }
        }

        // Move the selected operative.
        if (_selectedOperativeIndex >= 0 &&
            _selectedOperativeIndex < _operatives.Count)
        {
            Operative selected = _operatives[_selectedOperativeIndex];

            int distance =
                Math.Abs(selected.Tile.X - tile.X) +
                Math.Abs(selected.Tile.Y - tile.Y);

            // Movement must be exactly one tile.
            if (distance != 1)
                return;

            if (_arena.GetTile(tile.X, tile.Y) != TileType.Floor)
                return;

            // Do not move onto another operative.
            foreach (Operative other in _operatives)
            {
                if (other != selected && other.Tile == tile)
                    return;
            }

            // Do not move onto an enemy.
            foreach (Enemy enemy in _enemies)
            {
                if (enemy.Tile == tile)
                    return;
            }

            selected.Tile = tile;
            _selectedOperativeIndex = -1;
            return;
        }

        // Place operatives on the left half of the arena.
        if (_operatives.Count >= SquadSize ||
            tile.X >= Arena.Columns / 2 ||
            _arena.GetTile(tile.X, tile.Y) != TileType.Floor)
        {
            return;
        }

        _operatives.Add(new Operative(_selectedClass, tile));

        // Spawn enemies after all four operatives are placed.
        if (_operatives.Count == SquadSize && !_enemiesSpawned)
            SpawnEnemies();
    }

    private void AttackEnemy(Enemy enemy)
    {
        if (_selectedOperativeIndex < 0 ||
            _selectedOperativeIndex >= _operatives.Count)
        {
            return;
        }

        Operative operative = _operatives[_selectedOperativeIndex];

        int distance =
            Math.Abs(operative.Tile.X - enemy.Tile.X) +
            Math.Abs(operative.Tile.Y - enemy.Tile.Y);

        // Operatives can attack only adjacent enemies.
        if (distance != 1)
            return;

        enemy.TakeDamage(1);

        // Remove defeated enemies.
        if (!enemy.IsAlive)
            _enemies.Remove(enemy);

        _selectedOperativeIndex = -1;

        CheckGameState();
    }

    private void SpawnEnemies()
    {
        _enemiesSpawned = true;

        _enemies.Add(new Enemy(new Point(17, 2)));
        _enemies.Add(new Enemy(new Point(17, 6)));
        _enemies.Add(new Enemy(new Point(17, 10)));

        CheckGameState();
    }

    private void EndPlayerTurn()
    {
        if (_gameFinished)
            return;

        _selectedOperativeIndex = -1;

        _enemyAI.TakeEnemyTurn(_enemies, _operatives);

        CheckGameState();
    }

    private void CheckGameState()
    {
        if (_enemiesSpawned && _enemies.Count == 0)
        {
            _gameFinished = true;
            _victory = true;
        }
        else if (_operatives.Count == 0 && _enemiesSpawned)
        {
            _gameFinished = true;
            _victory = false;
        }

        if (_selectedOperativeIndex >= _operatives.Count)
            _selectedOperativeIndex = -1;
    }

    private void UpdateWindowTitle()
    {
        if (_gameFinished)
        {
            Window.Title = _victory
                ? "GRIDRUNNER | VICTORY - All enemies eliminated!"
                : "GRIDRUNNER | DEFEAT - Squad eliminated!";

            return;
        }

        if (!_enemiesSpawned)
        {
            Window.Title =
                $"Gridrunner: Protocol 2099 | Class: {_selectedClass} | " +
                $"Squad: {_operatives.Count}/{SquadSize}";
        }
        else
        {
            Window.Title =
                $"Gridrunner: Protocol 2099 | " +
                $"Operatives: {_operatives.Count} | " +
                $"Enemies: {_enemies.Count} | " +
                "SPACE: Enemy Turn";
        }
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
        DrawEnemies();
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

            // Health bar.
            DrawHealthBar(
                rectangle.X,
                rectangle.Y - 5,
                rectangle.Width,
                operative.Health,
                Operative.MaxHealth);

            // Selection outline.
            if (i == _selectedOperativeIndex)
                DrawOutline(rectangle, Color.White);
        }
    }

    private void DrawEnemies()
    {
        int left = (WindowWidth - Arena.Columns * TileSize) / 2;
        int top = (WindowHeight - Arena.Rows * TileSize) / 2;

        foreach (Enemy enemy in _enemies)
        {
            var rectangle = new Rectangle(
                left + enemy.Tile.X * TileSize + 5,
                top + enemy.Tile.Y * TileSize + 5,
                TileSize - 10,
                TileSize - 10);

            // Red enemy body.
            DrawRectangle(rectangle, new Color(220, 45, 55));

            // Darker center.
            DrawRectangle(
                new Rectangle(
                    rectangle.X + 7,
                    rectangle.Y + 7,
                    rectangle.Width - 14,
                    rectangle.Height - 14),
                new Color(120, 15, 30));

            // Enemy health bar.
            DrawHealthBar(
                rectangle.X,
                rectangle.Y - 5,
                rectangle.Width,
                enemy.Health,
                Enemy.MaxHealth);
        }
    }

    private void DrawHealthBar(
        int x,
        int y,
        int width,
        int health,
        int maxHealth)
    {
        DrawRectangle(
            new Rectangle(x, y, width, 3),
            new Color(65, 20, 25));

        if (health <= 0)
            return;

        int healthWidth = width * health / maxHealth;

        DrawRectangle(
            new Rectangle(x, y, healthWidth, 3),
            new Color(70, 240, 110));
    }

    private void DrawHud()
    {
        int arenaWidth = Arena.Columns * TileSize;
        int arenaHeight = Arena.Rows * TileSize;

        int left = (WindowWidth - arenaWidth) / 2;
        int top = (WindowHeight - arenaHeight) / 2;

        // Selected class marker.
        DrawRectangle(
            new Rectangle(left, top - 24, 20, 12),
            GetClassColor(_selectedClass));

        // Squad slots.
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
            new Rectangle(
                rectangle.X - 2,
                rectangle.Y - 2,
                rectangle.Width + 4,
                2),
            color);

        DrawRectangle(
            new Rectangle(
                rectangle.X - 2,
                rectangle.Bottom,
                rectangle.Width + 4,
                2),
            color);

        DrawRectangle(
            new Rectangle(
                rectangle.X - 2,
                rectangle.Y,
                2,
                rectangle.Height),
            color);

        DrawRectangle(
            new Rectangle(
                rectangle.Right,
                rectangle.Y,
                2,
                rectangle.Height),
            color);
    }

    private static Color GetClassColor(OperativeClass operativeClass)
    {
        return operativeClass switch
        {
            OperativeClass.CyberTank =>
                new Color(255, 170, 40),

            OperativeClass.NeuralSniper =>
                new Color(80, 180, 255),

            OperativeClass.SystemHacker =>
                new Color(190, 90, 255),

            OperativeClass.AssaultOperative =>
                new Color(255, 70, 70),

            OperativeClass.CombatMedic =>
                new Color(80, 255, 140),

            OperativeClass.StealthInfiltrator =>
                new Color(230, 230, 255),

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

public class Operative : BattleUnit
{
    public const int MaxHealth = 3;

    public OperativeClass Class { get; }

    public Operative(OperativeClass operativeClass, Point tile)
        : base(tile, MaxHealth, GetSpeed(operativeClass))
    {
        Class = operativeClass;
    }

    private static int GetSpeed(OperativeClass operativeClass)
    {
        return operativeClass switch
        {
            OperativeClass.CyberTank => 4,
            OperativeClass.NeuralSniper => 7,
            OperativeClass.SystemHacker => 5,
            OperativeClass.AssaultOperative => 8,
            OperativeClass.CombatMedic => 6,
            OperativeClass.StealthInfiltrator => 9,
            _ => 5
        };
    }
}