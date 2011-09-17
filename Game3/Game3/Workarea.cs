using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
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
        public static Workarea Current;
        [XmlIgnore]
        public SpriteFont Font;
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
        #endregion
    }
}
