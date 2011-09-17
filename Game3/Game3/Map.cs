using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Game3
{
    /// <summary>
    /// Карта
    /// </summary>
    [Serializable]
    public class Map
    {
        #region Конструкторы
        public Map()
        {

        }

        public Map(Workarea wa)
        {
            Units = new List<Unit>();
            Workarea = wa;
        }
        #endregion

        #region Сериализация
        public void Save(string filename)
        {
            using (Stream fileStream = File.Open(filename, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Map));
                serializer.Serialize(fileStream, this);
                fileStream.Close();
            }
        }

        static public Map Load(string filename, Workarea workarea)
        {
            using (Stream fileStream = File.Open(filename, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Map));
                Map map = serializer.Deserialize(fileStream) as Map;
                fileStream.Close();
                map.Workarea = workarea;

                foreach (var unit in map.Units)
                {
                    unit.Map = map;
                }

                return map;
            }
        }
        #endregion

        #region Свойства
        public string Name { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

        public float Depth { get; set; }

        public List<Unit> Units { get; set; }

        [XmlIgnore]
        public Workarea Workarea;

        public float[] Heightmap;

        public bool FogEnabled { get; set; }
        public Vector3 FogColor { get; set; }
        #endregion

        #region Методы
        public void Update(GameTime gameTime)
        {
            Units.RemoveAll(unit => unit.State == 0);

            foreach (var unit in Units)
            {
                unit.Update(gameTime);
            }
        }

        public void Draw(ICamera camera)
        {
            BoundingFrustum boundingFrustum = new BoundingFrustum(camera.View * camera.Proj);

            foreach (var unit in Units)
            {
                if(unit.Intersects(boundingFrustum))
                    unit.Draw(camera);
            }
        }

        /// <summary>
        /// Проверяет пересекает ли заданный юнит хотя бы один юнит на карте
        /// </summary>
        /// <param name="unit">Заданный юнит</param>
        public bool Intersects(Unit unit)
        {
            return Units.Any(unit.Intersects);
        }

        public override string ToString() { return Name; }
        #endregion
    }
}
