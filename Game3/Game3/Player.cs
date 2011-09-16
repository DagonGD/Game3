using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Game3
{
    /// <summary>
    /// Игрок
    /// </summary>
    class Player:Unit
    {
        public Player(string typeCode, Map map):base(typeCode,map)
        {
            
        }
        /// <summary>
        /// Вывод на экран информации о состоянии игрока
        /// </summary>
        /// <param name="camera">Камера</param>
        public override void Draw(ICamera camera)
        {
            //TODO: Вывод количества жизней и т.д
        }

        /// <summary>
        /// Управление игроком с помощью клавиатуры и мыши
        /// </summary>
        /// <param name="gameTime">Игровое время</param>
        public override void Update(GameTime gameTime)
        {
            Form frm = Control.FromHandle(Map.Workarea.Game.Window.Handle) as Form;
            if (!frm.Focused)
            {
                return;
            }

            const int turnSpeed = 5;

            int centerX = Map.Workarea.Game.GraphicsDevice.Viewport.Width / 2;
            int centerY = Map.Workarea.Game.GraphicsDevice.Viewport.Height / 2;

            MouseState mouseState = Mouse.GetState();
            Mouse.SetPosition(centerX, centerY);

            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float yaw = MathHelper.ToRadians((mouseState.X - centerX) * seconds * Workarea.Current.Settings.MouseSpeedX);
            float pitch = MathHelper.ToRadians((mouseState.Y - centerY) * seconds * Workarea.Current.Settings.MouseSpeedY);

            Angles = new Vector3(MathHelper.Clamp(Angles.X + pitch, MathHelper.ToRadians(-90f), MathHelper.ToRadians(90f)), Angles.Y + yaw, Angles.Z);

            Vector3 forward = -Vector3.Normalize(new Vector3(
                                             (float)Math.Sin(-Angles.Y) * (float)Math.Cos(Angles.X),
                                             (float)Math.Sin(Angles.X),
                                             (float)Math.Cos(-Angles.Y) * (float)Math.Cos(Angles.X)));

            Vector3 left = -Vector3.Normalize(new Vector3(
                                             (float)Math.Cos(Angles.Y),
                                             0f,
                                             (float)Math.Sin(Angles.Y)));


            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.W))
                Position += forward * seconds * Type.Speed;
            if (state.IsKeyDown(Keys.S))
                Position -= forward * seconds * Type.Speed;
            if (state.IsKeyDown(Keys.A))
                Position += left * seconds * Type.Speed;
            if (state.IsKeyDown(Keys.D))
                Position -= left * seconds * Type.Speed;
            if (state.IsKeyDown(Keys.Space))
                Position += Vector3.Up * seconds * Type.Speed;
            if (state.IsKeyDown(Keys.C))
                Position += Vector3.Down * seconds * Type.Speed;
        }

        public override string ToString() { return Name; }
    }
}
