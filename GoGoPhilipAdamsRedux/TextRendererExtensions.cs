using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiColorText
{
    public static class TextRendererExtensions
    {
        public static Vector2 MeasureString(this SpriteFont font, string text)
        {
            return TextRenderer.Default.MeasureString(font, text);
        }

        public static Vector2 MeasureString(this SpriteFont font, string text, float lineWidth)
        {
            return TextRenderer.Default.MeasureString(font, text, lineWidth);
        }

        public static void DrawColoredString(this SpriteBatch batch, SpriteFont font, string text, Vector2 position, float lineWidth = 0)
        {
            TextRenderer.Default.DrawString(batch, text, font, position, lineWidth);
        }

    }
}
