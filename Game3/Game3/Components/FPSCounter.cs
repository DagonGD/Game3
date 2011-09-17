using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game3.Components
{
    class FPSCounter : DrawableGameComponent
    {
        int _fps;
        int _frames;
        double _seconds;
        //SpriteBatch _spriteBatch;
        //SpriteFont _spriteFont;

        public FPSCounter(Game game)
            : base(game)
        {
            //_spriteBatch = new SpriteBatch(game.GraphicsDevice);
            //_spriteFont = game.Content.Load<SpriteFont>("Fonts//Courier New");
            //_spriteFont = game.Content.Load<SpriteFont>("Fonts//Times New Roman");
        }

        public override void Update(GameTime gameTime)
        {
            _seconds += gameTime.ElapsedGameTime.TotalSeconds;

            if (_seconds >= 1)
            {
                _fps = _frames;
                _seconds = 0;
                _frames = 0;
                Game.Window.Title = string.Format("fps:{0}, GC(0):{1}, GC(1):{2}, GC(2):{3}", _fps, GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _frames++;
            //_spriteBatch.Begin();
            //_spriteBatch.DrawString(_spriteFont, _fps.ToString(), new Vector2(0, 0), Color.White);
            //_spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
