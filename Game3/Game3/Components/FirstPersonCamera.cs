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
        private Game _game;
        public const float Growth = 1.8f;
        public FirstPersonCamera(Game game, Unit unit)
        {
            if (unit == null)
                throw new Exception("Юнит для управления камерой не найден");
            _unit = unit;
            _game = game;

            UpdateProjection();
        }

        public void UpdateProjection()
        {
            Proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), _game.GraphicsDevice.Viewport.AspectRatio, 0.1f, 100.0f);
        }

        public Matrix View
        {
            get
            {
                Matrix ret= Matrix.CreateTranslation(-_unit.Position)*
                       Matrix.CreateRotationZ(_unit.Angles.Z)*
                       Matrix.CreateRotationY(_unit.Angles.Y)*
                       Matrix.CreateRotationX(_unit.Angles.X);
                if (_unit.State != 0)
                    ret *= Matrix.CreateTranslation(new Vector3(0f, -Growth, 0f));
                else
                    ret *= Matrix.CreateTranslation(new Vector3(0f, -Growth, 0f)/3f);
                return ret;
            }
        }

        public Matrix Proj{ get; set; }
    }
}
