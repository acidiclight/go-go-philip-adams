using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;

namespace GoGoPhilipAdamsRedux
{
    public sealed class Victor
    {
        private int _size;
        private Texture2D _texture;
        private float _frequency = 20;
        private float _amplitude = 0.05f;
        private float _speed = 0.075f;
        private float _centerY;
        
        public Vector2 Location { get; set; } = new Vector2(1f, 0.5f);

        public Rectangle CalculateBounds(GraphicsDevice graphics)
        {
            var sw = graphics.PresentationParameters.BackBufferWidth;
            var sh = graphics.PresentationParameters.BackBufferHeight;

            var x = MathHelper.Lerp(-_size, sw + _size, Location.X);
            var y = MathHelper.Lerp(-_size, sh + _size, Location.Y);

            return new Rectangle((int)x - (_size / 2), (int)y - (_size / 2), _size, _size);
        }

        public Victor(Texture2D texture, int size, float y, float amp, float freq, float speed)
        {
            _speed = speed;
            _texture = texture;
            _size = size;

            Location = new Vector2(1, y);
            _centerY = y;
            _amplitude = amp;
            _frequency = freq;
        }

        public void Update(GameTime gameTime)
        {
            var x = Location.X - (_speed * (float)gameTime.ElapsedGameTime.TotalSeconds);

            Location = new Vector2(x, _centerY + (float)(Math.Sin(x * _frequency) * _amplitude));

        }

        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            var sw = batch.GraphicsDevice.PresentationParameters.BackBufferWidth;
            var sh = batch.GraphicsDevice.PresentationParameters.BackBufferHeight;

            var x = MathHelper.Lerp(-_size, sw + _size, Location.X);
            var y = MathHelper.Lerp(-_size, sh + _size, Location.Y);

            var rect = new Rectangle((int)x - (_size / 2), (int)y - (_size / 2), _size, _size);

            batch.Draw(_texture, rect, Color.White);


        }
    }
}
