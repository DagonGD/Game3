using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game3.Components
{
    class GameMap
    {
        private Map _map;
        private Game _game;
        private VertexPositionNormalTexture[] _vertices;
        private short[] _indices;
        private BasicEffect _basicEffect;
        private Texture2D _grass;
        private readonly int _width;
        private readonly int _height;

        public GameMap(Game game, Map map)
        {
            _game = game;
            _map = map;
            _width =(int) map.Sizes.X + 1;
            _height = (int)map.Sizes.Y + 1;
            _basicEffect = new BasicEffect(game.GraphicsDevice)
                                            {
                                                FogEnabled = map.FogEnabled,
                                                FogStart = map.ForStart,
                                                FogEnd = map.FogEnd,
                                                FogColor = map.FogColor,
                                            };

            _basicEffect.LightingEnabled = map.LightingEnabled;
            if (map.DirectionalLight0 != null)
                map.DirectionalLight0.Fill(_basicEffect.DirectionalLight0);
            if (map.DirectionalLight1 != null)
                map.DirectionalLight1.Fill(_basicEffect.DirectionalLight1);
            if (map.DirectionalLight2 != null)
                map.DirectionalLight2.Fill(_basicEffect.DirectionalLight2);

            if (map.EnableDefaultLighting)
                _basicEffect.EnableDefaultLighting();

            CreateVertices();
        }

        public void LoadContent(ContentManager content)
        {
            _grass = content.Load<Texture2D>("Textures/grass");
            _basicEffect.TextureEnabled = true;
            _basicEffect.Texture = _grass;
        }

        public void Update(GameTime gameTime)
        {
            _map.Update(gameTime);
        }

        public void Draw(ICamera camera)
        {
            _map.Draw(camera);
            DrawTerrain(camera);
        }

        private void CreateVertices()
        {
            _vertices = new VertexPositionNormalTexture[_width * _height];

            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    int index = i * _width + j;
                    _vertices[index].Position = new Vector3(i, _map[i,j], j);
                    //_vertices[index].Position = new Vector3(i, _map.GetHeight(new Vector3(i,0f,j)), j);
                    _vertices[index].TextureCoordinate = new Vector2((float)i / 4f, (float)j / 4f);
                }
            }

            _indices = new short[(_width - 1) * (_height - 1) * 6];

            short counter = 0;
            for (short i = 0; i < _width - 1; i++)
            {
                for (short j = 0; j < _height - 1; j++)
                {
                    short i1 = Convert.ToInt16(i * _width + j);
                    short i2 = Convert.ToInt16((i + 1) * _width + j);
                    short i3 = Convert.ToInt16(i * _width + j + 1);
                    short i4 = Convert.ToInt16((i + 1) * _width + j + 1);

                    _indices[counter++] = i1;
                    _indices[counter++] = i2;
                    _indices[counter++] = i3;

                    _indices[counter++] = i3;
                    _indices[counter++] = i2;
                    _indices[counter++] = i4;
                }
            }

            for (short i = 0; i < _indices.Length / 3; i++)
            {
                short i1 = _indices[i * 3];
                short i2 = _indices[i * 3 + 1];
                short i3 = _indices[i * 3 + 2];

                Vector3 p = _vertices[i1].Position - _vertices[i2].Position;
                Vector3 q = _vertices[i2].Position - _vertices[i3].Position;
                Vector3 n = Vector3.Cross(p, q);

                _vertices[i1].Normal += n;
                _vertices[i2].Normal += n;
                _vertices[i3].Normal += n;
            }

            for (short i = 0; i < _vertices.Length; i++)
            {
                _vertices[i].Normal = Vector3.Normalize(_vertices[i].Normal);
            }
        }

        private void DrawTerrain(ICamera camera)
        {
            _basicEffect.View = camera.View;
            _basicEffect.Projection = camera.Proj;


            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertices, 0, _vertices.Length, _indices, 0, _indices.Length / 3);
            }
        }
    }
}
