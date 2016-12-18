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
        private readonly string ReedSolomonLine         = "Reed Solomon:        {0,2} {1,2} {2,2} {3,2}";
        private readonly string SyncWordLine            = "Sync Word:              {0:X02}{1:X02}{2:X02}{3:X02}";
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
        public int[] ReedSolomon { get; set; }
        public byte[] SyncWord { get; set; }
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
                if (value.rsErrors != null) {
                    ReedSolomon[0] = value.rsErrors[0];
                    ReedSolomon[1] = value.rsErrors[1];
                    ReedSolomon[2] = value.rsErrors[2];
                    ReedSolomon[3] = value.rsErrors[3];
                }
                if (value.syncWord != null) {
                    SyncWord[0] = value.syncWord[0];
                    SyncWord[1] = value.syncWord[1];
                    SyncWord[2] = value.syncWord[2];
                    SyncWord[3] = value.syncWord[3];
                }
            } 
        }
        #endregion

        public CurrentFrameData(SpriteFont font) {
            this.font = font;
            fontHeight = font.MeasureString("A").Y;
            ReedSolomon = new int[] { 0, 0, 0, 0 };
            SyncWord = new byte[] { 0, 0, 0, 0 };
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
            spriteBatch.DrawString(font, String.Format(ReedSolomonLine, ReedSolomon[0], ReedSolomon[1], ReedSolomon[2], ReedSolomon[3]), new Vector2(Position.X, Position.Y + fontHeight * 4), Color.Black);
            spriteBatch.DrawString(font, String.Format(SignalQualityLine, SignalQuality), new Vector2(Position.X, Position.Y + fontHeight * 5), Color.Black);
            spriteBatch.DrawString(font, String.Format(SyncCorrelationLine, SyncCorrelation), new Vector2(Position.X, Position.Y + fontHeight * 6), Color.Black);
            spriteBatch.DrawString(font, String.Format(PhaseCorrectionLine, PhaseCorrection), new Vector2(Position.X, Position.Y + fontHeight * 7), Color.Black);
            spriteBatch.DrawString(font, String.Format(RunningTimeLine, runningTime.ToString(@"dd\.hh\:mm\:ss")), new Vector2(Position.X, Position.Y + fontHeight * 8), Color.Black);
            spriteBatch.DrawString(font, String.Format(SyncWordLine, SyncWord[0], SyncWord[1], SyncWord[2], SyncWord[3]), new Vector2(Position.X, Position.Y + fontHeight * 9), Color.Black);
        }

        #endregion
    }
}

