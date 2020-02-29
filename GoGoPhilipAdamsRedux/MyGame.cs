﻿
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using SpriteFontPlus;

namespace GoGoPhilipAdamsRedux
{
    public class MyGame : Game
    {
        private GraphicsDeviceManager _gfx;
        private SpriteBatch _batch;
        private List<Texture2D> _backgrounds;
        private Texture2D _currentBG = null;
        private Texture2D _newBG = null;
        private double _bgTime = 0;
        private float _bgFade = 0;
        private Random _rnd = new Random();
        private Texture2D _penguin = null;
        private const int _penguinSize = 32;
        private float _penguinX = 0.15f;
        private float _penguinY = 0.5f;
        private double _playerSpeed = 0.25;
        private Rectangle PenguinBounds;
        private Texture2D _enemy = null;
        private List<Victor> _victors = new List<Victor>();
        private const int STARTING_LIVES_LEFT = 3;
        private const double INVINCIBILITY_TIME_AFTER_DEATH = 5;
        private int _livesLeft = 0;
        private double _invincibilityTime = 0;
        private bool _invincibleRenderPlayer = false;
        private double _invicibleFlashTime = 0;
        private const int MAX_WAVE_ENEMIES = 7;
        private const double SECS_BETWEEN_WAVE = 12;
        private const double SECS_BETWEEN_ENEMY = 1;
        private const int SCORE_VICTOR_KILLED = 100;
        private const int SCORE_LOSS_VICTOR_ESCAPE = 100;
        private const float MIN_WAVE_AMPLITUDE_EVER = 0.0075f;
        private const float MAX_WAVE_AMPLITUDE_EVER = 0.125f;
        private int _score = 0;
        private const double MIN_WAVE_FREQ_EVER = 0.5;
        private const double MAX_WAVE_FREQ_EVER = 10;
        private SpriteFont _uiSmall = null;
        private SpriteFont _uiMedium = null;
        private SpriteFont _uiLarge = null;
        private List<Bullet> _hitlist = new List<Bullet>();
        private KeyboardState _lastKeyboard;
        private double _waveTimeLeft = 0;
        private int _enemiesToSpawn = 0;
        private double _nextEnemyTime = 0;
        private bool _spawning = false;
        private float _enemyPos = 0;
        private float _waveAmplitude;
        private float _waveFrequency;
        private Texture2D _bulletTexture = null;
        private List<Bullet> _bullets = new List<Bullet>();
        private const float BULLET_SPEED_PLAYER = 0.3f;
        private const float BULLET_SPEED_ENEMY = 0.05f;
        private const float STARTING_WAVE_SPEED = 0.075f;
        private double _nextBulletTime = TIME_BETWEEN_ENEMY_BULLETS;
        private const double TIME_BETWEEN_ENEMY_BULLETS = 4;
        private float _waveSpeed;
        private const float WAVE_SPEED_INCREASE = 0.015f;
        private const float MAX_WAVE_SPEED = 0.25f;

#if DEBUG
        private string GetDebugText()
        {
            return $@"Victors on-screen: {_victors.Count}
Bullets on-screen: {_bullets.Count}
Player pos: {_penguinX}, {_penguinY}
Wave speed: {_waveSpeed}";
        }
#endif

        public Rectangle Bounds => new Rectangle(
            0,
            0,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight);

        public MyGame()
        {
            _gfx = new GraphicsDeviceManager(this);
            _gfx.PreferredBackBufferWidth = 1280;
            _gfx.PreferredBackBufferHeight = 720;

            IsFixedTimeStep = true;
        }

        private void AddScore(int score)
        {
            _score += Math.Abs(score);
        }

        private void RemoveScore(int score)
        {
            _score = Math.Max(0, _score - Math.Abs(score));
        }

        private SpriteFont LoadFont(string filePath, float fontSize)
        {
            using (var stream = File.OpenRead(filePath))
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                var bake = TtfFontBaker.Bake(bytes, fontSize, 2048, 2048, new[] { CharacterRange.BasicLatin });

                return bake.CreateSpriteFont(GraphicsDevice);
            }
        }

        private void Die()
        {
            _livesLeft--;
            if(_livesLeft <= 0)
            {
                Exit();
            }
            else
            {
                _penguinX = 0.15f;
                _penguinY = 0.5f;
                _invincibilityTime = INVINCIBILITY_TIME_AFTER_DEATH;
                _invincibleRenderPlayer = false;
                _invicibleFlashTime = 0;
            }
        }

        protected override void LoadContent()
        {
            _uiSmall = LoadFont("Contemporary-Regular.ttf", 12);
            _uiMedium = LoadFont("Contemporary-Regular.ttf", 18);
            _uiLarge = LoadFont("Contemporary-Regular.ttf", 24);

            _batch = new SpriteBatch(GraphicsDevice);

            _livesLeft = STARTING_LIVES_LEFT;
            _invincibilityTime = 0;

            _backgrounds = new List<Texture2D>();

            _bulletTexture = new Texture2D(GraphicsDevice, 2, 2);
            _bulletTexture.SetData<uint>(new[] { uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue });

            _enemy = Texture2D.FromFile(GraphicsDevice, Path.Combine("Sprites", "enemy.png"));

            _penguin = Texture2D.FromFile(GraphicsDevice, Path.Combine("Sprites", "trans-osft-penguin.png"));

            if(Directory.Exists("Backgrounds"))
            {
                foreach(var file in Directory.GetFiles("Backgrounds"))
                {
                    try
                    {
                        var tex = Texture2D.FromFile(GraphicsDevice, file);
                        if(tex != null)
                        {
                            _backgrounds.Add(tex);
                        }
                    }
                    catch { }
                }
            }

            if(_backgrounds.Count > 0)
            {
                _currentBG = _backgrounds[_rnd.Next(0, _backgrounds.Count)];
            }

            _waveSpeed = STARTING_WAVE_SPEED;

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (_spawning)
            {
                if(_nextEnemyTime >= 0)
                {
                    _nextEnemyTime -= gameTime.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    if(_enemiesToSpawn > 0)
                    {
                        _victors.Add(new Victor(_enemy, _penguinSize, _enemyPos, _waveAmplitude, _waveFrequency, _waveSpeed));
                        _enemiesToSpawn--;
                        _nextEnemyTime = SECS_BETWEEN_ENEMY;
                    }
                    else
                    {
                        _waveSpeed = MathHelper.Clamp(_waveSpeed + WAVE_SPEED_INCREASE, STARTING_WAVE_SPEED, MAX_WAVE_SPEED);
                        _spawning = false;
                        _waveTimeLeft = SECS_BETWEEN_WAVE;
                    }
                }
            }
            else
            {
                if (_waveTimeLeft >= 0)
                {
                    _waveTimeLeft -= gameTime.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    _spawning = true;
                    _enemiesToSpawn = MAX_WAVE_ENEMIES;
                    _nextEnemyTime = 0;
                    _enemyPos = (float)((_rnd.NextDouble() * 0.6) + 0.2);

                    var freqRange = MAX_WAVE_FREQ_EVER - MIN_WAVE_FREQ_EVER;
                    var ampRange = MAX_WAVE_AMPLITUDE_EVER - MIN_WAVE_AMPLITUDE_EVER;

                    _waveAmplitude = (float)((_rnd.NextDouble() * ampRange) + MIN_WAVE_AMPLITUDE_EVER);
                    _waveFrequency = (float)((_rnd.NextDouble() * freqRange) + MIN_WAVE_FREQ_EVER);
                }
            }

            if(_newBG == null)
            {
                _bgTime += gameTime.ElapsedGameTime.TotalSeconds;
                if(_bgTime >= 10)
                {
                    _bgTime = 0;
                    _newBG = _backgrounds[_rnd.Next(0, _backgrounds.Count)];
                    _bgFade = 0;
                }
            }
            else
            {
                _bgFade = MathHelper.Clamp(_bgFade + ((float)gameTime.ElapsedGameTime.TotalSeconds * 4), 0, 1);
                if(_bgFade >= 1)
                {
                    _currentBG = _newBG;
                    _newBG = null;
                    _bgTime = 0;
                }
            }

            if(_nextBulletTime > 0)
            {
                _nextBulletTime -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                _nextBulletTime = TIME_BETWEEN_ENEMY_BULLETS;
                if(_victors.Count > 0)
                {
                    var vicr123 = _victors[_rnd.Next(0, _victors.Count)];
                    _bullets.Add(new Bullet(_bulletTexture, false, BULLET_SPEED_ENEMY, vicr123.Location.X, vicr123.Location.Y));
                }
            }

            double playerXAxis = 0;
            double playerYAxis = 0;

            var keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Left))
            {
                playerXAxis = -1;
            }
            else if (keyboard.IsKeyDown(Keys.Right))
            {
                playerXAxis = 1;
            }

            if (keyboard.IsKeyDown(Keys.Up))
            {
                playerYAxis = -1;
            }
            else if(keyboard.IsKeyDown(Keys.Down))
            {
                playerYAxis = 1;
            }

            _penguinX = MathHelper.Clamp((float)(_penguinX + ((playerXAxis * gameTime.ElapsedGameTime.TotalSeconds)) * _playerSpeed), 0, 1);
            _penguinY = MathHelper.Clamp((float)(_penguinY + ((playerYAxis * gameTime.ElapsedGameTime.TotalSeconds)) * _playerSpeed), 0, 1);

            var playerXInterpolated = MathHelper.Lerp(-_penguinSize, Bounds.Width + _penguinSize, _penguinX);
            var playerYInterpolated = MathHelper.Lerp(-_penguinSize, Bounds.Height + _penguinSize, _penguinY);

            PenguinBounds = new Rectangle((int)playerXInterpolated - (_penguinSize/2), (int)playerYInterpolated - (_penguinSize/2), _penguinSize, _penguinSize);

            foreach (var victor in _victors)
            {
                victor.Update(gameTime);

                if (_invincibilityTime <= 0)
                {
                    var vicBounds = victor.CalculateBounds(GraphicsDevice);
                    if (vicBounds.Intersects(PenguinBounds))
                    {
                        Die();
                    }
                }
            }

            var removed = _victors.RemoveAll(x => x.Location.X < 0 || (_invincibilityTime <= 0 && x.CalculateBounds(GraphicsDevice).Intersects(PenguinBounds)));

            if(removed > 0)
            {
                Console.WriteLine("[cleanup] removed {0} victor entities", removed);
                RemoveScore(SCORE_LOSS_VICTOR_ESCAPE * removed);
            }

            foreach (var bullet in _bullets)
            {
                bullet.Update(gameTime);

                var bounds = bullet.CalculateBounds(GraphicsDevice);
                
                if(bullet.IsPlayer)
                {
                    var hitVictors = _victors.RemoveAll(x => x.CalculateBounds(GraphicsDevice).Intersects(bounds));

                    if(hitVictors > 0)
                    {
                        AddScore(SCORE_VICTOR_KILLED * hitVictors);
                        _hitlist.Add(bullet);
                    }
                }
                else
                {
                    // TODO: player defeat.
                    if(PenguinBounds.Intersects(bounds) && _invincibilityTime <= 0)
                    {
                        Die();
                        _hitlist.Add(bullet);
                    }
                }
            }

            var bulletsCulled = _bullets.RemoveAll(x => x.Location.X <= 0 || x.Location.X >= 1 || _hitlist.Contains(x));
            _hitlist.Clear();

            if(bulletsCulled > 0)
            {
                Console.WriteLine("[cleanup] Culled {0} bullets.  Unlike you, I just dodged a bullet.", bulletsCulled);
            }

            if(keyboard.IsKeyDown(Keys.Space) && !_lastKeyboard.IsKeyDown(Keys.Space))
            {
                _bullets.Add(new Bullet(_bulletTexture, true, BULLET_SPEED_PLAYER, _penguinX, _penguinY));
            }

            _lastKeyboard = keyboard;

            if(_invincibilityTime > 0)
            {
                _invincibilityTime -= gameTime.ElapsedGameTime.TotalSeconds;

                if(_invicibleFlashTime >= 1)
                {
                    _invicibleFlashTime = 0;
                    _invincibleRenderPlayer = !_invincibleRenderPlayer;
                }
                else
                {
                    _invicibleFlashTime += (gameTime.ElapsedGameTime.TotalSeconds * 16);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _batch.Begin();

            if(_currentBG != null)
            {
                _batch.Draw(_currentBG, Bounds, Color.Gray);
            }

            if(_newBG != null)
            {
                _batch.Draw(_newBG, Bounds, Color.Gray * _bgFade);
            }

            bool renderPlayer = (_invincibilityTime > 0) ? _invincibleRenderPlayer : true;
            if (renderPlayer)
            {
                _batch.Draw(_penguin, PenguinBounds, Color.White);
            }

            foreach (var bullet in _bullets) bullet.Draw(gameTime, _batch);
            foreach (var victor in _victors) victor.Draw(gameTime, _batch);

            var scoreText = $"SCORE: {_score}";

            _batch.DrawString(_uiMedium, scoreText, new Vector2(27, 27), Color.Black);
            _batch.DrawString(_uiMedium, scoreText, new Vector2(25, 25), Color.White);

            var livesLeftText = $"LIVES: {_livesLeft}";
            var livesMeasure = _uiMedium.MeasureString(livesLeftText);
            var livesPos = new Vector2((Bounds.Width - livesMeasure.X) - 25, 25);

            _batch.DrawString(_uiMedium, livesLeftText, livesPos + new Vector2(2, 2), Color.Black);
            _batch.DrawString(_uiMedium, livesLeftText, livesPos, Color.White);

#if DEBUG
            var debugText = GetDebugText();
            var debugMeasure = _uiSmall.MeasureString(debugText);
            var debugPos = new Vector2(15, (Bounds.Height - debugMeasure.Y) - 15);

            _batch.DrawString(_uiSmall, debugText, debugPos + new Vector2(2, 2), Color.Black);
            _batch.DrawString(_uiSmall, debugText, debugPos, Color.White);

#endif

            _batch.End();

            base.Draw(gameTime);
        }
    }
}
