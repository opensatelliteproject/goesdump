using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace OpenSatelliteProject {
    public class CurrentFrameData: Drawable, Updatable {

        #region Format Strings
        private readonly string SatelliteIDLine         = "SC ID:                       {0,3}";
        private readonly string VirtualChannelIDLine    = "VC ID:                       {0,3}";
        private readonly string PacketNumberLine        = "Packet Number:        {0,10}";   
        private readonly string ViterbiLine             = "Viterbi Errors: {0,4} / {1,4} bits";
        private readonly string SignalQualityLine       = "Signal Quality:             {0,3}%";
        private readonly string SyncCorrelationLine     = "Sync Correlation:            {0,3}";
        private readonly string PhaseCorrectionLine     = "Phase Correction:            {0,3}";
        private readonly string RunningTimeLine         = "Running Time:        {0,11}";
        #endregion

        #region Private Fields
        private SpriteFont font;
        private float fontHeight;
        private DateTime startTime;
        private TimeSpan runningTime;
        #endregion

        #region Properties
        public int SatelliteID { get; set; }
        public int VirtualChannelID { get; set; }
        public UInt64 PacketNumber { get; set; }
        public int TotalBits { get; set; }
        public int ViterbiErrors { get; set; }
        public int SignalQuality { get; set; }
        public int SyncCorrelation { get; set; }
        public int PhaseCorrection { get; set; }
        public Vector2 Position { get; set; }

        public Statistics_st Statistics { 
            set {
                SatelliteID = value.scid;
                VirtualChannelID = value.vcid;
                PacketNumber = value.packetNumber;
                TotalBits = value.frameBits;
                ViterbiErrors = value.vitErrors;
                SignalQuality = value.signalQuality;
                SyncCorrelation = value.syncCorrelation;
                PhaseCorrection = value.phaseCorrection;
                startTime = Tools.UnixTimeStampToDateTime(value.startTime);
            } 
        }
        #endregion

        public CurrentFrameData(SpriteFont font) {
            this.font = font;
            fontHeight = font.MeasureString("A").Y;
        }

        #region Drawable implementation

        public void update(GameTime gameTime) {
            runningTime = DateTime.Now.Subtract(startTime);
        }

        public void draw(SpriteBatch spriteBatch, GameTime gameTime) {
            spriteBatch.DrawString(font, String.Format(SatelliteIDLine, SatelliteID), new Vector2(Position.X, Position.Y + fontHeight * 0), Color.Black);
            spriteBatch.DrawString(font, String.Format(VirtualChannelIDLine, VirtualChannelID), new Vector2(Position.X, Position.Y + fontHeight * 1), Color.Black);
            spriteBatch.DrawString(font, String.Format(PacketNumberLine, PacketNumber), new Vector2(Position.X, Position.Y+ fontHeight * 2), Color.Black);
            spriteBatch.DrawString(font, String.Format(ViterbiLine, ViterbiErrors, TotalBits), new Vector2(Position.X, Position.Y + fontHeight * 3), Color.Black);
            spriteBatch.DrawString(font, String.Format(SignalQualityLine, SignalQuality), new Vector2(Position.X, Position.Y + fontHeight * 4), Color.Black);
            spriteBatch.DrawString(font, String.Format(SyncCorrelationLine, SyncCorrelation), new Vector2(Position.X, Position.Y + fontHeight * 5), Color.Black);
            spriteBatch.DrawString(font, String.Format(PhaseCorrectionLine, PhaseCorrection), new Vector2(Position.X, Position.Y + fontHeight * 6), Color.Black);
            spriteBatch.DrawString(font, String.Format(RunningTimeLine, runningTime.ToString(@"dd\.hh\:mm\:ss")), new Vector2(Position.X, Position.Y + fontHeight * 7), Color.Black);
        }

        #endregion
    }
}

