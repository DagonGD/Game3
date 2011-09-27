using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Game3
{
    /// <summary>
    /// Настройки игры
    /// </summary>
    [Serializable]
    public class Settings
    {
        public int ScreenWidth;
        public int ScreenHeight;
        public bool FullScreen;

        public bool IsFixedTimeStep;
        public bool SynchronizeWithVerticalRetrace;

        public float MouseSpeedX;
        public float MouseSpeedY;

        public bool DebugMode;

        #region Сериализация
        public void Save(string filename)
        {
            using (Stream fileStream = File.Open(filename, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(fileStream, this);
                fileStream.Close();
            }
        }

        static public Settings Load(string filename)
        {
            using (Stream fileStream = File.Open(filename, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                Settings settings = serializer.Deserialize(fileStream) as Settings;
                fileStream.Close();
                return settings;
            }
        }
        #endregion
    }
}
