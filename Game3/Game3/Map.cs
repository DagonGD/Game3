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
        /// <summary>название зоны</summary>
        public string Name { get; set; }
        /// <summary>Ширина карты</summary>
        public float Width { get; set; }
        /// <summary>Высота карты</summary>
        public float Height { get; set; }
        /// <summary>Глубина карты</summary>
        public float Depth { get; set; }
        /// <summary>Гравитация на карте (обычно -9.8f)</summary>
        public Vector3 Gravity { get; set; }
        /// <summary>Юниты на карте</summary>
        public List<Unit> Units { get; set; }

        #region Туман
        public bool FogEnabled { get; set; }
        public Vector3 FogColor { get; set; }
        public float ForStart { get; set; }
        public float FogEnd { get; set; }
        #endregion

        #region Освещение
        public bool EnableDefaultLighting { get; set; }
        public bool LightingEnabled { get; set; }
        public MapLight DirectionalLight0 { get; set; }
        public MapLight DirectionalLight1 { get; set; }
        public MapLight DirectionalLight2 { get; set; }
        #endregion

        [XmlIgnore]
        public Workarea Workarea;
        /// <summary>Карта высот</summary>
        public float[] Heightmap;

        
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

        /// <summary>
        /// Находит ближайший юнит, пересеченный лучем
        /// </summary>
        /// <param name="ray">Луч</param>
        /// <returns>Юнит</returns>
        public Unit Intersects(Ray ray)
        {
            Unit ret = null;
            List<Unit> targets = Units.Where(unit => unit.Intersects(ray) != null).ToList();

            foreach (var unit in targets)
            {
                if (ret == null || unit.Intersects(ray) < ret.Intersects(ray))
                    ret = unit;
            }
            return ret;
        }

        /// <summary>
        /// Получение высоты в точке с заданными координатами
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public float GetHeight(float x, float y)
        {
            if (x < 0 || x > Width || y < 0 || y > Height)
                return float.MinValue;

            return Heightmap[(int) (x*(Width+1) + y)];
        }

        public void SetHeight(float x,float y, float height)
        {
            if (x < 0 || x > Width  || y < 0 || y > Height )
                return;

            Heightmap[(int)(x * (Width + 1) + y)] = height;
        }

        /// <summary>
        /// Поднять ландшафт в заданной точке и в соседних на заданную высоту
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="height"></param>
        [Obsolete("Неправильно работает", true)]
        public void PickUpLandscape(int x, int y, float height)
        {
            SetHeight(x, y, GetHeight(x, y) + height);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if(i==x && j==y)
                        continue;

                    var dist = (float)(Math.Sqrt(Math.Pow(x - i, 2) + Math.Pow(y - j, 2)));
                    SetHeight(i, j, GetHeight(i, j) + height/dist);
                }
            }
        }

        public override string ToString() { return Name; }
        #endregion
    }
}
