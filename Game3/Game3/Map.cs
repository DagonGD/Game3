using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        /// <summary>Размеры карты</summary>
        public Vector3 Sizes { get; set; }
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

        public override string ToString() { return Name; }
        #endregion

        #region Карта высот
        public float this[int i, int j]
        {
            get
            {
                int index = i * (int)Sizes.X + j;
                return Heightmap[index];
            }
            set
            {
                int index = i * (int)Sizes.Y + j;
                Heightmap[index] = value;
            }
        }

        /// <summary>
        /// Получение высоты в точке с заданными координатами
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public float GetHeight(Vector3 position)
        {
            if (position.X < 0 || position.X > Sizes.X || position.Z < 0 || position.Z > Sizes.Y)
                return float.MinValue;

            //Интерполяция
            int i = (int)position.X;
            int j = (int)position.Z;

            Vector3 v1 = new Vector3(i, this[i, j], j);
            Vector3 v2 = new Vector3(i + 1, this[i + 1, j], j);
            Vector3 v3 = new Vector3(i, this[i, j + 1], j + 1);
            Vector3 v4 = new Vector3(i + 1, this[i + 1, j + 1], j + 1);

            Vector3 vx1 = Vector3.Lerp(v1, v2, position.X - i);
            Vector3 vx2 = Vector3.Lerp(v3, v4, position.X - i);

            Vector3 res = Vector3.Lerp(vx1, vx2, position.Z - j);

            //return this[i,j];
            return res.Y;
        }

        /// <summary>
        /// Поднять ландшафт в заданной точке и в соседних на заданную высоту
        /// </summary>
        /// <param name="pos">Координаты поднятия. Координата Y игнорируется</param>
        /// <param name="height">На сколько поднять</param>
        public void PickUpLandscape(Vector3 pos, float height)
        {
            int x = (int)pos.X;
            int y = (int)pos.Z;

            this[x, y] += height;
            float radius = height*2f;

            for (int i = 0; i < Sizes.X; i++)
            {
                for (int j = 0; j < Sizes.Z; j++)
                {
                    float dist = Math.Abs(Vector3.Distance(pos, new Vector3(i, pos.Y, j)));

                    if (dist < Workarea.Eps || dist > radius)
                        continue;

                    this[i, j] += (float)(height / Math.Pow(dist, 1d / 2d));
                }
            }
        }

        /// <summary>
        /// Создание карты высот из картинки
        /// </summary>
        /// <param name="filename"></param>
        public void CreateHightmapFromPicture(string filename)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Пересечения
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
        #endregion
    }
}
