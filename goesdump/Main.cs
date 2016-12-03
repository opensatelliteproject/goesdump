using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using System.Threading;

namespace OpenSatelliteProject {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Main : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Connector cn;
        SpriteFont font;
        CurrentFrameData cfd;

        DemuxManager demuxManager;
        Mutex mtx;
        Statistics_st statistics;

        public Main() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            cfd = null;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferMultiSampling = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            base.Initialize();
            mtx = new Mutex();
            cn = new Connector();
            demuxManager = new DemuxManager();
            cn.StatisticsAvailable += (Statistics_st data) => {
                mtx.WaitOne();
                statistics = data;
                mtx.ReleaseMutex();
            };
            cn.ChannelDataAvailable += (byte[] data) => demuxManager.parseBytes(data);
            statistics = new Statistics_st();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("defaultFont"); // Use the name of your sprite font file here instead of 'Score'.
            UIConsole.GlobalConsole.Font = font;
            UIConsole.GlobalConsole.Position = new Vector2(20, GraphicsDevice.Viewport.Height - UIConsole.GlobalConsole.MaxHeight - 20);
            cfd = new CurrentFrameData(font);
            cfd.Position = new Vector2(20, 20);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Console.WriteLine("Closing app");
                Exit();
            }

            mtx.WaitOne();
            cfd.Statistics = statistics;
            mtx.ReleaseMutex();

            cfd.update(gameTime);
            UIConsole.GlobalConsole.update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            graphics.GraphicsDevice.Clear(Color.LightGray);
            spriteBatch.Begin();

            cfd.draw(spriteBatch, gameTime);
            UIConsole.GlobalConsole.draw(spriteBatch, gameTime);

            spriteBatch.End();
            //TODO: Add your drawing code here
            
            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args) {
            base.OnExiting(sender, args);
            cn.Stop();
        }
    }
}

