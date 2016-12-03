using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace OpenSatelliteProject {
    public interface Drawable {
        void draw(SpriteBatch spriteBatch, GameTime gameTime);
    }
}

