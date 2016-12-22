using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace OpenSatelliteProject {
    public class MouseCursor: Drawable, Updatable {

        private static readonly int cursorSize = 32;

        private Rectangle position;
        private Texture2D cursor;

        public MouseCursor(Texture2D mouseCursor) {
            this.cursor = mouseCursor;
            this.position = new Rectangle(0, 0, cursorSize, cursorSize);
        }

        #region Drawable implementation

        public void draw(SpriteBatch spriteBatch, Microsoft.Xna.Framework.GameTime gameTime) {
            spriteBatch.Draw(cursor, position, Color.White);
        }

        #endregion

        #region Updatable implementation

        public void update(Microsoft.Xna.Framework.GameTime gameTime) {
            Point mPos = Mouse.GetState().Position;
            position = new Rectangle(mPos.X, mPos.Y, cursorSize, cursorSize);
        }

        #endregion
    }
}

