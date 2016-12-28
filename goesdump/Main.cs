using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.IO;

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
        UILed satelliteBusyLed;
        UILed heartBeatLed;
        UILed statisticsSocketLed;
        UILed dataSocketLed;
        UILed frameLockLed;
        MouseCursor cursor;

        int lastPhaseCorrection = -1;
        int desyncCount = 0;
        int heartBeatCount = 0;

        public Main() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            cfd = null;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferMultiSampling = true;

            this.Exiting += (object sender, EventArgs e) => {
                //cn.Stop();
            };
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

                if (data.phaseCorrection != lastPhaseCorrection && lastPhaseCorrection != -1) {
                    UIConsole.GlobalConsole.Error(String.Format("Costas Loop Desync! Phase correction was {0} and now is {1}.", lastPhaseCorrection, data.phaseCorrection));
                    desyncCount++;
                }
                frameLockLed.Color = data.frameLock == 1 ? Color.Lime : Color.Red;
                lastPhaseCorrection = data.phaseCorrection;
                satelliteBusyLed.Color = data.vcid != 63 && data.frameLock == 1 ? Color.Lime : Color.Red;
                if (heartBeatLed.Color != Color.Lime) {
                    heartBeatLed.Color = Color.Lime;
                    heartBeatCount = 0;
                }
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


            satelliteBusyLed = new UILed(GraphicsDevice, font);
            heartBeatLed = new UILed(GraphicsDevice, font);
            statisticsSocketLed = new UILed(GraphicsDevice, font);
            dataSocketLed = new UILed(GraphicsDevice, font);
            frameLockLed = new UILed(GraphicsDevice, font);

            satelliteBusyLed.Position = new Vector2(20, 220);
            satelliteBusyLed.Text = "Satellite Busy";

            heartBeatLed.Position = new Vector2(20, 250);
            heartBeatLed.Text = "Heart Beat";

            statisticsSocketLed.Text = "Statistics Connected";
            statisticsSocketLed.Position = new Vector2(20, 280);

            dataSocketLed.Text = "Data Connected";
            dataSocketLed.Position = new Vector2(20, 310);

            frameLockLed.Text = "Frame Lock";
            frameLockLed.Position = new Vector2(20, 340);

            Texture2D mouseCursor = Content.Load<Texture2D>("arrow");
            cursor = new MouseCursor(mouseCursor);
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
            satelliteBusyLed.update(gameTime);
            heartBeatLed.update(gameTime);

            if (heartBeatCount == 10) {
                heartBeatLed.Color = Color.Red;
            } else {
                heartBeatCount++;
            }

            statisticsSocketLed.Color = cn.StatisticsConnected ? Color.Lime : Color.Red;
            dataSocketLed.Color = cn.DataConnected ? Color.Lime : Color.Red;

            statisticsSocketLed.update(gameTime);
            dataSocketLed.update(gameTime);
            frameLockLed.update(gameTime);
            cursor.update(gameTime);

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
            satelliteBusyLed.draw(spriteBatch, gameTime);
            heartBeatLed.draw(spriteBatch, gameTime);
            statisticsSocketLed.draw(spriteBatch, gameTime);
            dataSocketLed.draw(spriteBatch, gameTime);
            frameLockLed.draw(spriteBatch, gameTime);

            cursor.draw(spriteBatch, gameTime);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args) {
            base.OnExiting(sender, args);
            cn.Stop();
            Environment.Exit(Environment.ExitCode);
        }
    }
}

