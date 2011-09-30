using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game3
{
    /// <summary>
    /// Рабочая область
    /// </summary>
    [Serializable]
    public class Workarea
    {
        #region Конструкторы
        public Workarea()
        {
            UnitTypes = new List<UnitType>();
        }
        #endregion

        #region Ресурсы
        public List<UnitType> UnitTypes { get; set; }
        [XmlIgnore]
        public Settings Settings { get; set; }
        [XmlIgnore]
        public Game1 Game { get; set; }
        [XmlIgnore]
        public ICamera Camera { get; set; }
        [XmlIgnore]
        public static Workarea Current;
        [XmlIgnore]
        public SpriteFont Font;
        [XmlIgnore]
        public SoundEffect Shotgun, Ghost;
        #endregion

        #region Сериализация
        public void Save(string filename)
        {
            using (Stream fileStream = File.Open(filename, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Workarea));
                serializer.Serialize(fileStream, this);
                fileStream.Close();
            }
        }

        static public Workarea Load(string filename, ContentManager Content)
        {
            Workarea workarea;
            using (Stream fileStream = File.Open(filename, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Workarea));
                workarea = serializer.Deserialize(fileStream) as Workarea;
                fileStream.Close();
            }

            //Загрузка моделей
            foreach (var type in workarea.UnitTypes)
            {
                try
                {
                    type.Model = Content.Load<Model>("Models/" + type.Code);
                }
                catch (ContentLoadException)
                {
                    type.Model = null;
                }
            }

            //Загрузка шрифтов
            workarea.Font = Content.Load<SpriteFont>("Fonts/Courier New");

            //Загрузка звуков
            workarea.Shotgun = Content.Load<SoundEffect>("Sounds/shotgun");
            workarea.Ghost = Content.Load<SoundEffect>("Sounds/ghostly1");

            Current = workarea;

            return workarea;
        }
        #endregion

        #region Методы
        public UnitType GetUnitType(string code)
        {
            UnitType ret = UnitTypes.FirstOrDefault(s => s.Code == code);
            if (ret == null)
                throw new Exception("Неизвестный тип юнита - " + code);
            return ret;
        }
        //public static Vector3 GetAngles(Matrix matrix)
        //{
        //    return new Vector3((float)Math.Atan(matrix.M23 / matrix.M33), (float)Math.Asin(-matrix.M13), (float)Math.Atan(matrix.M12 / matrix.M11));
        //}
        private static Vector3 QuaternionToEuler2(Quaternion q)
        {
            Vector3 euler;

            float sqx = q.X * q.X;
            float sqy = q.Y * q.Y;
            float sqz = q.Z * q.Z;
            float sqw = q.W * q.W;

            float unit = sqx + sqy + sqz + sqw;
            float test = (q.X * q.W - q.Y * q.Z);

            // Handle singularity
            if (test > 0.4999999f * unit)
            {
                euler.X = MathHelper.PiOver2;
                euler.Y = 2.0f * (float)System.Math.Atan2(q.Y, q.W);
                euler.Z = 0;
            }
            else if (test < -0.4999999f * unit)
            {
                euler.X = -MathHelper.PiOver2;
                euler.Y = 2.0f * (float)System.Math.Atan2(q.Y, q.W);
                euler.Z = 0;
            }
            else
            {
                float ey_Y = 2 * (q.X * q.Z + q.Y * q.W);
                float ey_X = 1 - 2 * (sqy + sqx);
                float ez_Y = 2 * (q.X * q.Y + q.Z * q.W);
                float ez_X = 1 - 2 * (sqx + sqz);
                euler.X = (float)System.Math.Asin(2 * test);
                euler.Y = (float)System.Math.Atan2(ey_Y, ey_X);
                euler.Z = (float)System.Math.Atan2(ez_Y, ez_X);
            }
            return euler;
        }

        public static string GetInfo(Matrix matrix)
        {
            Vector3 scales;
            Quaternion quaternion;
            Vector3 translation;
            matrix.Decompose(out scales, out quaternion, out translation);

            return string.Format("Scales:{0}; Angles:{1}; Trans:{2}", scales,
                //GetAngles(matrix), 
                QuaternionToEuler2(quaternion),
                translation);
        }
        #endregion
    }
}
