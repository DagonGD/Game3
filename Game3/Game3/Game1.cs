using System;
using System.Collections.Generic;
using System.Linq;
using Game3.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Game3
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
// ReSharper disable InconsistentNaming
        private GraphicsDeviceManager graphics;
        private FPSCounter fpsCounter;
        private Workarea workarea;
        private Settings settings;
        private Map map;
        private FirstPersonCamera camera;
        //private Axies axies;
        private GameMap gameMap;
        private Player player;
// ReSharper restore InconsistentNaming

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            fpsCounter = new FPSCounter(this);
            Components.Add(fpsCounter);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            settings = Settings.Load("Settings.xml");
            workarea = Workarea.Load("Workarea.xml", Content);
            workarea.Settings = settings;
            workarea.Game = this;

            map = Map.Load("Map1.xml", workarea);
            gameMap=new GameMap(this, map);
            gameMap.LoadContent(Content);

            player = new Player("PLAYER", map){ Fraction = 1, Position = new Vector3(10f,1.8f,15f), Angles = Vector3.Zero};
            map.Units.Add(player);
            camera = new FirstPersonCamera(this, player);

            //axies=new Axies(this);

            IsFixedTimeStep = settings.IsFixedTimeStep;
            graphics.SynchronizeWithVerticalRetrace = settings.SynchronizeWithVerticalRetrace;
            graphics.PreferredBackBufferWidth = settings.ScreenWidth;
            graphics.PreferredBackBufferHeight = settings.ScreenHeight;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            #region Keyboard
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Escape))
                Exit();

            if (keyState.IsKeyDown(Keys.F))
            {
                if (!graphics.IsFullScreen)
                {
                    graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                }
                else
                {
                    graphics.PreferredBackBufferWidth = settings.ScreenWidth;
                    graphics.PreferredBackBufferHeight = settings.ScreenHeight;
                }
                graphics.IsFullScreen = !graphics.IsFullScreen;
                graphics.ApplyChanges();
            }
            #endregion

            gameMap.Update(gameTime);
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(map.FogColor));

            //axies.Draw(camera);
            gameMap.Draw(camera);

            base.Draw(gameTime);
        }
    }
}
