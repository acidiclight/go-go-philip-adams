using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoGoPhilipAdamsRedux
{
    public sealed class Bullet
    {
        private bool _isPlayer = false;
        private float _speed;
        private Vector2 _location;
        private int _width = 1;
        private int _height = 1;
        private Texture2D _texture = null;
        public Vector2 Location => _location;
        public float Speed => _speed;
        public bool IsPlayer => _isPlayer;

        public Rectangle CalculateBounds(GraphicsDevice graphics)
        {
            float x = MathHelper.Lerp(-_width, graphics.PresentationParameters.BackBufferWidth + _width, _location.X);
            float y = MathHelper.Lerp(-_height, graphics.PresentationParameters.BackBufferHeight + _height, _location.Y);

            x -= (_width / 2);
            y -= (_height / 2);

            return new Rectangle((int)x, (int)y, _width, _height);
        }

        public Bullet(Texture2D texture, bool isPlayer, float speed, float x, float y)
        {
            _texture = texture;
            _location = new Vector2(x, y);
            _speed = speed;
            _isPlayer = isPlayer;
            _width = _texture.Width;
            _height = _texture.Height;
        }

        public void Update(GameTime gameTime)
        {
            float trueSpeed = (_isPlayer) ? _speed : -_speed;

            _location.X += trueSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (IsPlayer)
            {
                Console.WriteLine("[bullet] {0}, {1}", _location.X, _location.Y);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            var rect = CalculateBounds(batch.GraphicsDevice);
            batch.Draw(_texture, rect, Color.White);
        }
    }
}
