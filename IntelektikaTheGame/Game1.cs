using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using IntelektikaTheGame.GameLogic;
using IntelektikaTheGame.World;
using System.Linq;

namespace IntelektikaTheGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private GameWorld _world;
        private GameLogic.GameLogic _logic;
        private FlowLogic _flow;
        private Dictionary<string, Texture2D> _textures;

        private double _turnTimer = 0;
        private const double TurnDelay = 0.05;

        private bool _isWaitingBetweenTeams = false;
        private double _pauseTimer = 0;
        private const double PostTurnPause = 0.25;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 780;
        }

        protected override void Initialize()
        {
            _world = new GameWorld(50, 30);
            _logic = new GameLogic.GameLogic();
            _flow = new FlowLogic();

            MapPresets.GenerateChokePointMap(_world);
            SpawnPresets.SpawnTeams(_world);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _textures = new Dictionary<string, Texture2D>();

            //Tiles
            string[] tileNames = { "tile_grass", "tile_forest", "tile_water", "tile_mountain" };
            foreach (var name in tileNames)
                _textures[name] = Content.Load<Texture2D>($"Sprites/Tiles/{name}");

            //Units
            string[] unitNames = { "att_melee", "att_archer", "att_flier", "att_healer", "att_cav",
                                   "def_melee", "def_archer", "def_flier", "def_healer", "def_cav" };
            foreach (var name in unitNames)
                _textures[name] = Content.Load<Texture2D>($"Sprites/Units/{name}");

            //Indicators
            _textures["misc_indicator"] = Content.Load<Texture2D>("Sprites/Misc/misc_indicator");
            _textures["misc_indicatorred"] = Content.Load<Texture2D>("Sprites/Misc/misc_indicatorred");
            _textures["misc_indicatorblue"] = Content.Load<Texture2D>("Sprites/Misc/misc_indicatorblue");
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            if (!_flow.IsGameOver)
            {
                if (_isWaitingBetweenTeams)
                {
                    _pauseTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    if (_pauseTimer >= PostTurnPause)
                    {
                        _isWaitingBetweenTeams = false;
                        _pauseTimer = 0;
                    }
                }
                else
                {
                    _turnTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    if (_turnTimer >= TurnDelay)
                    {
                        _flow.ProcessTurn(_world, _logic);
                        _turnTimer = 0;

                        _isWaitingBetweenTeams = true;
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            float scale = 0.4f;
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(scale));

            //1. Draw Tiles
            for (int x = 0; x < _world.WorldWidth; x++)
            {
                for (int y = 0; y < _world.WorldHeight; y++)
                {
                    var tile = _world.Grid[x, y];
                    Vector2 pos = new Vector2(x * 64, y * 64);
                    _spriteBatch.Draw(_textures[tile.ArtUsed], pos, Color.White);
                }
            }

            //2. Draw Visual Paths
            var allUnits = GetAllUnits(_world);
            foreach (var unit in allUnits)
            {
                if (unit.CurrentVisualPath != null && unit.CurrentVisualPath.Count > 0)
                {
                    string texKey = unit.PathType switch
                    {
                        "attack" => "misc_indicatorred",
                        "heal" => "misc_indicatorblue",
                        _ => "misc_indicator"
                    };

                    foreach (var tile in unit.CurrentVisualPath)
                    {
                        (int pathX, int pathY) = GetTileCoords(tile, _world);
                        if (pathX != -1)
                        {
                            _spriteBatch.Draw(_textures[texKey], new Vector2(pathX * 64, pathY * 64), Color.White * 0.6f);
                        }
                    }
                }
            }

            //3. Draw Units
            for (int x = 0; x < _world.WorldWidth; x++)
            {
                for (int y = 0; y < _world.WorldHeight; y++)
                {
                    var unit = _world.Grid[x, y].OccupyingUnit;
                    if (unit != null)
                    {
                        Vector2 unitPos = new Vector2(x * 64 - 32, y * 64 - 64);
                        _spriteBatch.Draw(_textures[unit.picRef], unitPos, Color.White);
                    }
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private List<Figurine> GetAllUnits(GameWorld world)
        {
            var units = new List<Figurine>();
            for (int x = 0; x < world.WorldWidth; x++)
                for (int y = 0; y < world.WorldHeight; y++)
                    if (world.Grid[x, y].OccupyingUnit != null)
                        units.Add(world.Grid[x, y].OccupyingUnit);
            return units;
        }

        private (int, int) GetTileCoords(Tile tile, GameWorld world)
        {
            for (int x = 0; x < world.WorldWidth; x++)
                for (int y = 0; y < world.WorldHeight; y++)
                    if (world.Grid[x, y] == tile) return (x, y);
            return (-1, -1);
        }
    }
}