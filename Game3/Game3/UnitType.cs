using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;

namespace Game3
{
    /// <summary>
    /// Тип юнита
    /// </summary>
    [Serializable]
    public class UnitType
    {
        public UnitType()
        {
            Scale = 1f;
        }

        #region Свойства
        /// <summary>Наименование типа юнита</summary>
        public string Name { get; set; }
        /// <summary>Код типа юнита</summary>
        public string Code { get; set; }
        /// <summary>Максимальный запас здоровья</summary>
        public float HealthMax { get; set; }
        /// <summary>Мин урон</summary>
        public float DamageMin { get; set; }
        /// <summary>Макс урон</summary>
        public float DamageMax { get; set; }
        /// <summary>Скорость передвижения</summary>
        public float Speed { get; set; }
        /// <summary>Дистанция видимости</summary>
        public float VisibilityRange { get; set; }
        /// <summary>Дистанция атаки</summary>
        public float AttackRange { get; set; }
        /// <summary>Пауза между атаками</summary>
        public float AttackDelay { get; set; }

        //private Model _model;
        /// <summary>Модель</summary>
        [XmlIgnore]
        public Model Model { get; set; }//{ get { return _model ?? (_model = Workarea.Current.Game.Content.Load<Model>("Models/" + Code)); }}
        /// <summary>Масштаб</summary>
        public float Scale { get; set; }
        #endregion

        public override string ToString() { return Name; }
    }
}
