using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game3.Components
{
    class Interface : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;
        private readonly Game _game;
        private Unit _unit;
        private FPSCounter _fpsCounter;

        public Interface(Game game, Unit unit, FPSCounter fpsCounter=null):base(game)
        {
            _game = game;
            _unit = unit;
            _fpsCounter = fpsCounter;
            _spriteBatch = new SpriteBatch(_game.GraphicsDevice);
        }

        private void DrawText(string text, Vector2 pos)
        {
            _spriteBatch.DrawString(Workarea.Current.Font, text, Vector2.Zero, Color.White, 0f, pos, 1f, SpriteEffects.None, 1f);
        }

        public override void Draw(GameTime gameTime)
        {
            float y = 20;

            _spriteBatch.Begin();
            DrawText(string.Format("Health: {0:F0};", _unit.Health), new Vector2(0f, y-=20));
            if (Workarea.Current.Settings.DebugMode)
            {
                DrawText(string.Format("Pos: {0:F2}; {1:F2}; {2:F2};", _unit.Position.X, _unit.Position.Y, _unit.Position.Z), new Vector2(0f, y -= 20));
                DrawText(string.Format("Angles: {0:F2}; {1:F2};", MathHelper.ToDegrees(_unit.Angles.X), MathHelper.ToDegrees(_unit.Angles.Y)), new Vector2(0f, y -= 20));
                DrawText(string.Format("Impulse: {0:F2}; {1:F2}; {2:F2};", _unit.Impulse.X, _unit.Impulse.Y, _unit.Impulse.Z), new Vector2(0f, y -= 20));
            }
            if(_fpsCounter!=null)
                DrawText(string.Format("FPS: {0};", _fpsCounter.Fps), new Vector2(0f, y-=20));
            if (_unit.Type.IsFlyable)
                DrawText(string.Format("Flyable"), new Vector2(0f, y -= 20));
            if(_unit.State==0)
                DrawText(string.Format("Dead"), new Vector2(0f, y -= 20));
            DrawText("X", new Vector2(Workarea.Current.Game.GraphicsDevice.Viewport.Width, Workarea.Current.Game.GraphicsDevice.Viewport.Height)/-2f);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
