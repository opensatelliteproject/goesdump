using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenSatelliteProject {
    public class UILed: Drawable, Updatable {

        // Pixels
        private static readonly int texScaleFactor = 4;
        private static readonly int ledRadius = 10;
        private static readonly int borderWidth = 2;
        private static readonly int ledToTextSpace = 6;

        public Color Color { get; set; }
        public Color TextColor { get; set; }
        public Vector2 Position { get; set; }
        public string Text { get; set; }

        private Color lastColor;
        private string lastText;
        private Texture2D texture;
        private SpriteFont font;
        private Rectangle ledRectangle;
        private Vector2 textPosition;
        private Vector2 lastPosition;

        public Vector2 Size { 
            get {
                Vector2 textSize = font.MeasureString(Text);
                return new Vector2(textSize.X + ledToTextSpace + ledRadius, Math.Max(ledRadius * 2, textSize.Y));
            } 
        }

        public UILed(GraphicsDevice graphicsDevice, SpriteFont font) {
            Color = Color.Red;
            lastColor = Color.Red;
            TextColor = Color.Black;
            Position = new Vector2(0, 0);
            lastPosition = new Vector2(0, 0);
            lastText = "";
            Text = "";
            int effectiveDiameter = 2 * ledRadius * texScaleFactor;
            this.font = font;
            texture = new Texture2D(graphicsDevice, effectiveDiameter, effectiveDiameter);
            refreshTexture();
            refreshSizes();
        }

        private void refreshTexture() {
            int effectiveDiameter = 2 * ledRadius * texScaleFactor;
            Color[] colorData = new Color[effectiveDiameter * effectiveDiameter];

            float radius = effectiveDiameter / 2f;
            float radiusSquared = radius * radius;
            float borderWidthSquared = (borderWidth * texScaleFactor * 2) * (borderWidth * texScaleFactor * 2);

            for (int x = 0; x < effectiveDiameter; x++) {
                for (int y = 0; y < effectiveDiameter; y++) {
                    int index = (int) (x * effectiveDiameter + y);
                    Vector2 pos = new Vector2(x - radius, y - radius);
                    if (pos.LengthSquared() <= radiusSquared - borderWidthSquared) {
                        colorData[index] = Color;
                    } else if (pos.LengthSquared() <= radiusSquared) {
                        colorData[index] = Color.Black;
                    } else {
                        colorData[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(colorData);
        }

        private void refreshSizes() {
            Vector2 size = this.Size;
            Vector2 textSize = font.MeasureString(Text);

            float deltaY = size.Y - ledRadius * 2;
            float textDeltaY = size.Y - textSize.Y;

            ledRectangle = new Rectangle((int)Position.X, (int)(Position.Y - deltaY), ledRadius * 2, ledRadius * 2);
            textPosition = new Vector2(ledRectangle.X + ledRadius * 2 + ledToTextSpace, Position.Y + textDeltaY / 2f);
        }

        #region Updatable implementation

        public void update(GameTime gameTime) {
            if (lastColor != Color) {
                lastColor = Color;
                refreshTexture();
            }

            if (lastText != Text || lastPosition != Position) {
                lastText = Text;
                lastPosition = Position;
                refreshSizes();
            }
        }

        #endregion

        #region Drawable implementation

        public void draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, GameTime gameTime) {
            spriteBatch.Draw(texture, ledRectangle, Color.White);
            spriteBatch.DrawString(font, Text, textPosition, TextColor);
        }

        #endregion
    }
}

