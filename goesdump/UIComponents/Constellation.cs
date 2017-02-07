using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace OpenSatelliteProject {
    public class Constellation: Drawable, Updatable {
        private float[] data;
        private bool dirty;
        private Texture2D texture;
        private Vector2 _size;
        private GraphicsDevice graphicsDevice;
        private Rectangle posRec;
        private SpriteFont font;
        private Vector2 _textPosition;
        public Vector2 _position;

        public Vector2 Size { 
            get { 
                return _size; 
            }
            set {
                _size = value;
                dirty = true;
            }
        }

        public Vector2 Position { 
            get { 
                return _position;
            }
            set {
                _position = value;
                dirty = true;
            }
        }

        public Constellation(GraphicsDevice graphicsDevice, SpriteFont font) {
            data = new float[1024];
            for (int i = 0; i < 1024; i++) {
                data[i] = 0;
            }
            dirty = true;
            Size = new Vector2(300, 300);
            Position = new Vector2(0, 0);
            texture = new Texture2D(graphicsDevice, (int)_size.X, (int)_size.Y);
            this.graphicsDevice = graphicsDevice;
            this.font = font;
        }

        #region Drawable implementation

        public void draw(SpriteBatch spriteBatch, GameTime gameTime) {
            spriteBatch.Draw(texture, posRec, Color.White);
            spriteBatch.DrawString(font, "Constellation", _textPosition, Color.Black);
        }

        #endregion

        #region Updatable implementation

        public void update(GameTime gameTime) {
            if (dirty) {
                refreshTexture();
                refreshText();
                posRec = new Rectangle((int)Position.X, (int)Position.Y, (int)_size.X, (int)_size.Y);
            }
        }

        #endregion

        private void refreshText() {
            Vector2 textSize = font.MeasureString("Constellation");
            _textPosition = new Vector2(Position.X + _size.X / 2 - textSize.X / 2, Position.Y);
            _textPosition.X = (float) Math.Floor(_textPosition.X);
            _textPosition.Y = (float) Math.Floor(_textPosition.Y);
        }

        private void drawCircle(float px, float py, int radius, ref Color[] data, float width) {
            drawCircle((int)px, (int)py, radius, ref data, (int)width);
        }

        private void drawCircle(int px, int py, int radius, ref Color[] data, int width, Color color) {
            int x = radius;
            int y = 0;
            int err = 0;

            while (x >= y) {
                putPixel(x + px, y + py, ref data, width, color);
                putPixel(y + px, x + py, ref data, width, color);
                putPixel(-x + px, y + py, ref data, width, color);
                putPixel(-y + px, x + py, ref data, width, color);
                putPixel(-x + px, -y + py, ref data, width, color);
                putPixel(-y + px, -x + py, ref data, width, color);
                putPixel(x + px, -y + py, ref data, width, color);
                putPixel(y + px, -x + py, ref data, width, color);

                if (err <= 0) {
                    y += 1;
                    err += 2*y + 1;
                }

                if (err > 0) {
                    x -= 1;
                    err -= 2*x + 1;
                }
            }
        }

        private void putPixel(int x, int y, ref Color[] data, int width, Color color) {
            if (x >= 0 && y >= 0) {
                int idx = y * width + x;
                if (idx >= data.Length) {
                    return;
                }
                data[idx] = color;
            }
        }

        private void refreshTexture() {
            if (_size.X != texture.Width || _size.Y != texture.Height) {
                texture = new Texture2D(graphicsDevice, (int)_size.X, (int)_size.Y);
            }

            Color[] data = new Color[(int)_size.X * (int)_size.Y];

            for (int i = 0; i < _size.X * _size.Y; i++) {
                data[i] = Color.Transparent;
            }

            int offsetY = 20;
            int sizeX = (int)_size.X;
            int sizeY = (int)_size.Y - offsetY;

            for (int y = 0; y < sizeY; y++) {
                for (int x = 0; x < sizeX; x++) {
                    if (x == 0 || x == (sizeX - 1) || x == (sizeX / 2)) {
                        putPixel(x, y + offsetY, ref data, sizeX, Color.Black);
                    } else if (y == 0 || y == (sizeY - 1) || y == (sizeY / 2)) {
                        putPixel(x, y + offsetY, ref data, sizeX, Color.Black);
                    } else {
                        putPixel(x, y + offsetY, ref data, sizeX, Color.Gray);
                    }
                }
            }

            for (int i = 0; i < 1024; i += 2) {
                // Comes flipped
                float Q = (this.data[i + 0] * sizeY) / 2;
                float I = (this.data[i + 1] * sizeX) / 2;

                int x = (int) (I + sizeX / 2);
                int y = (int) (Q + sizeY / 2);

                drawCircle(x, y + offsetY, 2, ref data, sizeX, Color.White);
            }

            texture.SetData(data);
        }

        public void updateConstellationData(float[] data) {
            this.data = data;
            this.dirty = true;
        }
    }
}

