using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game3
{
    /// <summary>
    /// Описание точечного источника света
    /// </summary>
    /// <remarks>
    /// Т.к. DirectionalLight не сериализуется пришлось создать отдельный класс
    /// </remarks>
    [Serializable]
    public class MapLight
    {
        /// <summary>Включен</summary>
        public bool Enabled;
        /// <summary>Цвет</summary>
        public Vector3 DiffuseColor;
        /// <summary>Цвет</summary>
        public Vector3 SpecularColor;
        /// <summary>Направление</summary>
        public Vector3 Direction;

        /// <summary>
        /// Применение настроек к DirectionalLight
        /// </summary>
        /// <param name="directionalLight"></param>
        public void Fill(DirectionalLight directionalLight)
        {
            directionalLight.Enabled = Enabled;
            directionalLight.DiffuseColor = DiffuseColor;
            directionalLight.SpecularColor = SpecularColor;
            directionalLight.Direction = Direction;
        }
    }
}