#if !HEADLESS
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.IO;
using OpenSatelliteProject.PacketData.Enums;
using OpenSatelliteProject.Log;
using OpenSatelliteProject.Tools;
using System.Net.Sockets;

namespace OpenSatelliteProject {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Main : Game {

        private ProgConfig config = new ProgConfig();
        private ImageManager FDImageManager;
        private ImageManager XXImageManager;
        private ImageManager NHImageManager;
        private ImageManager SHImageManager;
        private ImageManager USImageManager;

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
        Constellation cons;
        MouseCursor cursor;

        int lastPhaseCorrection = -1;
        int heartBeatCount = 0;

        public Main() {

            #region Create Config File
            config.RecordIntermediateFile = config.RecordIntermediateFile;
            config.ChannelDataServerName = config.ChannelDataServerName;
            config.ChannelDataServerPort = config.ChannelDataServerPort;
            config.ConstellationServerName = config.ConstellationServerName;
            config.ConstellationServerPort = config.ConstellationServerPort;
            config.StatisticsServerName = config.StatisticsServerName;
            config.StatisticsServerPort = config.StatisticsServerPort;
            config.EnableDCS = config.EnableDCS;
            config.EnableEMWIN = config.EnableEMWIN;
            config.EraseFilesAfterGeneratingFalseColor = config.EraseFilesAfterGeneratingFalseColor;
            config.GenerateFDFalseColor = config.GenerateFDFalseColor;
            config.GenerateNHFalseColor = config.GenerateNHFalseColor;
            config.GenerateSHFalseColor = config.GenerateSHFalseColor;
            config.GenerateUSFalseColor = config.GenerateXXFalseColor;
            config.HTTPPort = config.HTTPPort;
            config.GenerateInfraredImages = config.GenerateInfraredImages;
            config.GenerateVisibleImages = config.GenerateVisibleImages;
            config.GenerateWaterVapourImages = config.GenerateWaterVapourImages;
            config.MaxGenerateRetry = config.MaxGenerateRetry;
            config.SysLogServer = config.SysLogServer;
            config.SysLogFacility = config.SysLogFacility;
            config.UseNOAAFormat = config.UseNOAAFormat;
            config.EnableWeatherData = config.EnableWeatherData;
            config.Save();
            #endregion

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            cfd = null;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferMultiSampling = true;

            this.Exiting += (object sender, EventArgs e) => {
                //cn.Stop();
            };

            FileHandler.SkipEMWIN = !config.EnableEMWIN;
            FileHandler.SkipDCS = !config.EnableDCS;
            FileHandler.SkipWeatherData = !config.EnableWeatherData;
            ImageManager.EraseFiles = config.EraseFilesAfterGeneratingFalseColor;
            ImageManager.GenerateInfrared = config.GenerateInfraredImages;
            ImageManager.GenerateVisible = config.GenerateVisibleImages;
            ImageManager.GenerateWaterVapour = config.GenerateWaterVapourImages;
            ImageManager.MaxRetryCount = config.MaxGenerateRetry;
            ImageManager.UseNOAAFileFormat = config.UseNOAAFormat;

            Connector.ChannelDataServerName = config.ChannelDataServerName;
            Connector.StatisticsServerName = config.StatisticsServerName;
            Connector.ConstellationServerName = config.ConstellationServerName;

            Connector.ChannelDataServerPort = config.ChannelDataServerPort;
            Connector.StatisticsServerPort = config.StatisticsServerPort;
            Connector.ConstellationServerPort = config.ConstellationServerPort;

            if (LLTools.IsLinux) {
                SyslogClient.SysLogServerIp = config.SysLogServer;
                try {
                    SyslogClient.Send(new Message(config.SysLogFacility, Level.Information, "Your syslog connection is working! OpenSatelliteProject is enabled to send logs."));
                } catch (SocketException) {
                    UIConsole.GlobalConsole.Warn("Your syslog is not enabled to receive UDP request. Please refer to https://opensatelliteproject.github.io/OpenSatelliteProject/");
                }
            }

            if (config.GenerateFDFalseColor) {
                string fdFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_FULLDISK);
                FDImageManager = new ImageManager(Path.Combine("channels", fdFolder));
            }

            if (config.GenerateXXFalseColor) {
                string xxFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_AREA_OF_INTEREST);
                XXImageManager = new ImageManager(Path.Combine("channels", xxFolder));
            }

            if (config.GenerateNHFalseColor) {
                string nhFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_NORTHERN);
                NHImageManager = new ImageManager(Path.Combine("channels", nhFolder));
            }

            if (config.GenerateSHFalseColor) {
                string shFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_SOUTHERN);
                SHImageManager = new ImageManager(Path.Combine("channels", shFolder));
            }

            if (config.GenerateUSFalseColor) {
                string usFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_UNITEDSTATES);
                USImageManager = new ImageManager(Path.Combine("channels", usFolder));
            }
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
            demuxManager.RecordToFile = config.RecordIntermediateFile;
            cn.StatisticsAvailable += (Statistics_st data) => {
                mtx.WaitOne();
                statistics = data;
                mtx.ReleaseMutex();

                if (data.phaseCorrection != lastPhaseCorrection && lastPhaseCorrection != -1) {
                    //UIConsole.GlobalConsole.Error(String.Format("Costas Loop Desync! Phase correction was {0} and now is {1}.", lastPhaseCorrection, data.phaseCorrection));
                    //desyncCount++;
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
            cn.ConstellationDataAvailable += (float[] data) => cons.updateConstellationData(data);
            statistics = new Statistics_st();
            cn.Start();

            if (config.GenerateFDFalseColor) {
                FDImageManager.Start();
            }
            if (config.GenerateXXFalseColor) {
                XXImageManager.Start();
            }
            if (config.GenerateNHFalseColor) {
                NHImageManager.Start();
            }
            if (config.GenerateSHFalseColor) {
                SHImageManager.Start();
            }
            if (config.GenerateUSFalseColor) {
                USImageManager.Start();
            }
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

            cons = new Constellation(GraphicsDevice, font);
            cons.Position = new Vector2(400, 20);
            cons.Size = new Vector2(180, 180);


            satelliteBusyLed = new UILed(GraphicsDevice, font);
            heartBeatLed = new UILed(GraphicsDevice, font);
            statisticsSocketLed = new UILed(GraphicsDevice, font);
            dataSocketLed = new UILed(GraphicsDevice, font);
            frameLockLed = new UILed(GraphicsDevice, font);

            int firstLed = 260;
            satelliteBusyLed.Position = new Vector2(20, firstLed);
            satelliteBusyLed.Text = "Satellite Busy";

            heartBeatLed.Position = new Vector2(20, firstLed + 30);
            heartBeatLed.Text = "Heart Beat";

            statisticsSocketLed.Text = "Statistics Connected";
            statisticsSocketLed.Position = new Vector2(20, firstLed + 60);

            dataSocketLed.Text = "Data Connected";
            dataSocketLed.Position = new Vector2(20, firstLed + 90);

            frameLockLed.Text = "Frame Lock";
            frameLockLed.Position = new Vector2(20, firstLed + 120);

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
            cons.update(gameTime);
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
            cons.draw(spriteBatch, gameTime);

            cursor.draw(spriteBatch, gameTime);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args) {
            base.OnExiting(sender, args);
            cn.Stop();
            if (config.GenerateFDFalseColor) {
                FDImageManager.Stop();
            }
            if (config.GenerateXXFalseColor) {
                XXImageManager.Stop();
            }
            if (config.GenerateNHFalseColor) {
                NHImageManager.Stop();
            }
            if (config.GenerateSHFalseColor) {
                SHImageManager.Stop();
            }
            if (config.GenerateUSFalseColor) {
                USImageManager.Stop();
            }
            Environment.Exit(Environment.ExitCode);
        }
    }
}
#endif
