using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game3.Components
{
    class FirstPersonCamera:ICamera
    {
        private Unit _unit;
        public FirstPersonCamera(Game game, Unit unit)
        {
            if (unit == null)
                throw new Exception("Юнит для управления камерой не найден");
            _unit = unit;

            Proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), game.GraphicsDevice.Viewport.AspectRatio, 0.1f, 100.0f);
        }

        public Matrix View
        {
            get
            {
                return Matrix.CreateTranslation(-_unit.Position) *
                 Matrix.CreateRotationZ(_unit.Angles.Z) *
                 Matrix.CreateRotationY(_unit.Angles.Y) *
                 Matrix.CreateRotationX(_unit.Angles.X);
            }
        }

        public Matrix Proj{ get; set; }
    }
}
