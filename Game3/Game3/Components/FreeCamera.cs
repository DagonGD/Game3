using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Game3.Components
{
    class FreeCamera : GameComponent, ICamera
    {
        public Matrix View { get; set; }
        public Matrix Proj { get; set; }

        public Vector3 Position;
        private Vector3 _angle;

        public FreeCamera(Game game, Vector3 position, Vector3 angle, Matrix proj)
            : base(game)
        {
            this.Position = position;
            this._angle = angle;
            Proj = proj;
            //View = Matrix.CreateLookAt(position, lookAt, Vector3.Up);
        }

        public override void Update(GameTime gameTime)
        {
            Form frm = Control.FromHandle(Game.Window.Handle) as Form;
            if (!frm.Focused)
            {
                return;
            }

            const int speed = 3;
            const int turnSpeed = 3;

            int centerX = Game.GraphicsDevice.Viewport.Width / 2;
            int centerY = Game.GraphicsDevice.Viewport.Height / 2;

            MouseState mouseState = Mouse.GetState();
            Mouse.SetPosition(centerX, centerY);

            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float yaw = MathHelper.ToRadians((mouseState.X - centerX) * seconds * turnSpeed);
            float pitch = MathHelper.ToRadians((mouseState.Y - centerY) * seconds * turnSpeed);

            float angleX = MathHelper.Clamp(_angle.X + pitch, MathHelper.ToRadians(-90f), MathHelper.ToRadians(90f));
            _angle = new Vector3(angleX, _angle.Y + yaw, _angle.Z);

            Vector3 forward = -Vector3.Normalize(new Vector3(
                                             (float)Math.Sin(-_angle.Y) * (float)Math.Cos(_angle.X),
                                             (float)Math.Sin(_angle.X),
                                             (float)Math.Cos(-_angle.Y) * (float)Math.Cos(_angle.X)));

            Vector3 left = -Vector3.Normalize(new Vector3(
                                             (float)Math.Cos(_angle.Y),
                                             0f,
                                             (float)Math.Sin(_angle.Y)));

            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.W))
                Position += forward * seconds * speed;
            if (state.IsKeyDown(Keys.S))
                Position -= forward * seconds * speed;
            if (state.IsKeyDown(Keys.A))
                Position += left * seconds * speed;
            if (state.IsKeyDown(Keys.D))
                Position -= left * seconds * speed;
            if (state.IsKeyDown(Keys.Space))
                Position += Vector3.Up * seconds * speed;
            if (state.IsKeyDown(Keys.C))
                Position += Vector3.Down * seconds * speed;

            View = Matrix.CreateTranslation(-Position) *
                   Matrix.CreateRotationZ(_angle.Z) *
                   Matrix.CreateRotationY(_angle.Y) *
                   Matrix.CreateRotationX(_angle.X);

            base.Update(gameTime);
        }
    }
}
