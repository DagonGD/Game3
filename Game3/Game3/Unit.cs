using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game3
{
    /// <summary>
    /// Юнит
    /// </summary>
    [Serializable]
    public class Unit
    {
        #region Конструкторы
        public Unit()
        {

        }

        public Unit(string typeCode, Map map)
        {         
            TypeCode = typeCode;
            Map = map;
            Health = Type.HealthMax;
            State = 1;
            Impulse = Vector3.Zero;
            Scales = Vector3.One;
        }
        #endregion

        #region Свойства юнита
        /// <summary>Личное имя юнита</summary>
        public string Name { get; set; }
        /// <summary>Фракция юнита. Юниты из пртивоположных фракций атакуют друг-друга. Если равно 0, то юнит - декорация.</summary>
        public int Fraction { get; set; }
        /// <summary>Статус юнита. Если 0 то юнит мертв</summary>
        public int State { get; set; }
        /// <summary>Теукщее здоровье</summary>
        public float Health { get; set; }
        /// <summary>Позиция юнита </summary>
        public Vector3 Position { get; set; }
        /// <summary>Углы с осями</summary>
        public Vector3 Angles { get; set; }
        /// <summary>Масштабы</summary>
        public Vector3 Scales { get; set; }
        /// <summary>Время последней атаки</summary>
        public double LastAttackTime { get; set; }
        /// <summary>Указатель на карту, на которой расположен юнит</summary>
        [XmlIgnore]
        public Map Map { get; set; }
        /// <summary>Код класса юнита </summary>
        public string TypeCode { get; set; }

        private UnitType _type;
        /// <summary>Тип юнита</summary>
        [XmlIgnore]
        public UnitType Type { get { return _type ?? (_type = Map.Workarea.GetUnitType(TypeCode)); } }
        #endregion

        #region Физика
        public readonly Vector3 JumpAcceleration = new Vector3(0f, 3f, 0f);
        public const float MaxSafeAcceleration = -4f;
        public const float GravitationalDamage = 1f;
        public const float Eps = 0.001f;
        public Vector3 Impulse { get; set; }

        /// <summary>
        /// Обработка влияния на юнит гравитации и импульса
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdatePhysics(GameTime gameTime)
        {
            if (Fraction != 0)
            {
                //Если юнит не летающий и находится над землей, то ускорить его падение
                if (!Type.IsFlyable && !IsOnGround())
                {
                    Impulse += Map.Gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                //Осуществить влияние импульса
                Position += Impulse * (float)gameTime.ElapsedGameTime.TotalSeconds;

                //Если юнит достиг земли
                if (IsOnGround())
                {
                    //Удар об землю
                    Position = new Vector3(Position.X, Map.GetHeight(Position.X, Position.Z), Position.Z);
                    if (Impulse.Y < MaxSafeAcceleration)
                        Health += Impulse.Y * GravitationalDamage;
                    Impulse = Vector3.Zero;
                }
            }
        }
        #endregion

        #region Вспомогательные свойства
        private Matrix[] _tramsforms;
        /// <summary>
        /// Массив матриц трансфорсации для каждой сетки модели
        /// </summary>
        public Matrix[] Transforms
        {
            get
            {
                if (_tramsforms == null && Type.Model != null)
                {
                    _tramsforms = new Matrix[Type.Model.Bones.Count];
                    Type.Model.CopyAbsoluteBoneTransformsTo(_tramsforms);
                }
                return _tramsforms;
            }
        }
        #endregion

        #region Методы
        /// <summary>
        /// Нарисовать себя
        /// </summary>
        /// <param name="camera"></param>
        public virtual void Draw(ICamera camera)
        {
            if (Type.Model == null)
            {
                Color color = Fraction == 1 ? Color.White : Color.Black;

                VertexPositionColor[] vertexData = new VertexPositionColor[3];
                vertexData[0] = new VertexPositionColor(Position, Color.Red);
                vertexData[1] = new VertexPositionColor(Position + new Vector3(0.0f, 0.5f, 0.0f), color);
                vertexData[2] = new VertexPositionColor(Position + new Vector3(0.5f, 0.0f, 0.5f), color);
                Map.Workarea.Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexData, 0, 1);
            }
            else
            {
                foreach (ModelMesh mesh in Type.Model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.FogEnabled = Map.FogEnabled;
                        effect.FogStart = Map.ForStart;
                        effect.FogEnd = Map.FogEnd;
                        effect.FogColor = Map.FogColor;

                        effect.LightingEnabled = Map.LightingEnabled;
                        if(Map.DirectionalLight0!=null)
                            Map.DirectionalLight0.Fill(effect.DirectionalLight0);
                        if (Map.DirectionalLight1 != null)
                            Map.DirectionalLight1.Fill(effect.DirectionalLight1);
                        if (Map.DirectionalLight2 != null)
                            Map.DirectionalLight2.Fill(effect.DirectionalLight2);

                        if (Map.EnableDefaultLighting)
                            effect.EnableDefaultLighting();

                        effect.World = Transforms[mesh.ParentBone.Index] * Type.World*Matrix.CreateScale(Scales) *
                                        Matrix.CreateRotationX(Angles.X) * Matrix.CreateRotationY(Angles.Y) * Matrix.CreateRotationZ(Angles.Z) *
                                        Matrix.CreateTranslation(Position);
                        effect.View = camera.View;
                        effect.Projection = camera.Proj;
                    }
                    mesh.Draw();
                }
            }
        }

        /// <summary>
        /// Основная логика поведения юнита
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            if (Health < Eps)
            {
                State = 0;
                Health = 0;
                return;
            }

            #region Атака или передвижение к ближайшему врагу
            if (Fraction != 0)
            {
                Unit nearestEnemy = FindNearestVisibleEnemy();

                if (nearestEnemy != null)
                {
                    RotateTo(nearestEnemy);

                    if (DistanceTo(nearestEnemy) > Type.AttackRange)
                    {
                        //Перемещение
                        Step(nearestEnemy, Type.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
                    }
                    else
                    {
                        if ((gameTime.TotalGameTime.TotalSeconds - LastAttackTime) > Type.AttackDelay)
                        {
                            //Атака
                            LastAttackTime = gameTime.TotalGameTime.TotalSeconds;
                            nearestEnemy.Health -= Type.DamageMax;
                        }
                    }
                }
            }
            #endregion

            UpdatePhysics(gameTime);
        }

        /// <summary>
        /// Проверка находится ли юнит на земле
        /// </summary>
        /// <returns></returns>
        public bool IsOnGround()
        {
            return Position.Y < Map.GetHeight(Position.X, Position.Z) + Eps;
        }

        /// <summary>
        /// Вычисление расстояния до заданного юнита
        /// </summary>
        /// <param name="unit">Заданный юнит</param>
        /// <returns>Расстояние</returns>
        public float DistanceTo(Unit unit)
        {
            return Vector3.Distance(Position, unit.Position);
        }

        /// <summary>
        /// Поворот юнита вокруг оси Y по напрввлению к заданному
        /// </summary>
        /// <param name="unit">Заданный юнит</param>
        public void RotateTo(Unit unit)
        {
            Vector3 direction = unit.Position - Position;

            float angleY = (float)Math.Atan(direction.Z / direction.X);
            if (direction.X > 0)
                angleY = -angleY - (float) Math.PI/2.0f;
            else
                angleY = -angleY + (float) Math.PI/2.0f;
            Angles = new Vector3(Angles.X, angleY, Angles.Z);
        }

        /// <summary>
        /// Поиск ближайшего врага
        /// </summary>
        /// <returns></returns>
        public Unit FindNearestVisibleEnemy()
        {
            Unit enemy = null;

            foreach (Unit unit in Map.Units)
            {
                if (unit == this)
                    continue;

                float distance = DistanceTo(unit);
                if (((enemy == null) || (distance < unit.DistanceTo(enemy))) && (distance < Type.VisibilityRange) && (Fraction != unit.Fraction) && (unit.Fraction != 0))
                    enemy = unit;
            }

            return enemy;
        }

        /// <summary>
        /// Перемещает текущий юнит в направлении заданного на заданное расстояние
        /// </summary>
        /// <param name="unit">Заданный юнит</param>
        /// <param name="distance">Расстояние</param>
        public void Step(Unit unit, float distance)
        {
            Position += Vector3.Normalize(unit.Position - Position)*distance;
        }

        public override string ToString() { return Name; }

        #region Столкновения
        /// <summary>
        /// BoundingBox для юнита. Если не задан, то при обработки столконовений будет использована автоматически вычисляемая BoundingSphere
        /// </summary>
        public BoundingBox? BoundingBox
        {
            get
            {
                if (!Type.BoundingBox.HasValue)
                    return null;

                Matrix transform = Matrix.CreateTranslation(Position) * Matrix.CreateScale(Scales) *
                                   Matrix.CreateRotationZ(Angles.Z)*Matrix.CreateRotationY(Angles.Y)*Matrix.CreateRotationX(Angles.X);

                return new BoundingBox(Vector3.Transform(Type.BoundingBox.Value.Min, transform),
                                       Vector3.Transform(Type.BoundingBox.Value.Max, transform));
            }
        }

        /// <summary>
        /// Проверка столкновения юнита с заданной пирамидой вида
        /// </summary>
        /// <param name="boundingFrustum">Пирамида вида</param>
        /// <returns>Истина если пересекаются</returns>
        public bool Intersects(BoundingFrustum boundingFrustum)
        {
            if (BoundingBox != null)
                return boundingFrustum.Intersects(BoundingBox.Value);

            if (Type.Model == null)
                return true;
            
            return Type.Model.Meshes.Any(mesh =>boundingFrustum.Intersects(
                        mesh.BoundingSphere.Transform(Transforms[mesh.ParentBone.Index]*Type.World*Matrix.CreateTranslation(Position)*Matrix.CreateScale(Scales))));
        }

        /// <summary>
        /// Проверка столкновения юнита с заданной сферой
        /// </summary>
        /// <param name="boundingSphere">Сфера</param>
        /// <returns>Истина если пересекаются</returns>
        public bool Intersects(BoundingSphere boundingSphere)
        {
            if (BoundingBox != null)
                return BoundingBox.Value.Intersects(boundingSphere);

            return Type.Model == null ? false : Type.Model.Meshes.Any(mesh => boundingSphere.Intersects(
                        mesh.BoundingSphere.Transform(Transforms[mesh.ParentBone.Index] * Type.World * Matrix.CreateTranslation(Position) * Matrix.CreateScale(Scales))));
        }

        public bool Intersects(BoundingBox boundingBox)
        {
            if (BoundingBox.HasValue)
                return BoundingBox.Value.Intersects(boundingBox);

            return Type.Model == null ? false : Type.Model.Meshes.Any(mesh => mesh.BoundingSphere.Transform(Transforms[mesh.ParentBone.Index] * Type.World * Matrix.CreateTranslation(Position) * Matrix.CreateScale(Scales)).Intersects(boundingBox));
        }

        /// <summary>
        /// Проверка столкновения юнита с заданным
        /// </summary>
        /// <param name="unit">Заданный юнит</param>
        /// <returns></returns>
        [Obsolete("Проверяет только столкновения сфер, но не параллелепипедов")]
        public virtual bool Intersects(Unit unit)
        {
            if (unit == this)
                return false;

            if (Type.Model == null || unit.Type.Model == null)
                return false;

            //Проверка столкновения каждой сферы юнита с каждой сферой заданного юнита
            foreach (var mesh in Type.Model.Meshes)
            {
                foreach (var mesh1 in unit.Type.Model.Meshes)
                {
                    if (mesh.BoundingSphere.Transform(Transforms[mesh.ParentBone.Index] * Type.World * Matrix.CreateTranslation(Position) * Matrix.CreateScale(Scales)).Intersects(
                        mesh1.BoundingSphere.Transform(unit.Transforms[mesh1.ParentBone.Index] * Type.World * Matrix.CreateTranslation(unit.Position) * Matrix.CreateScale(Scales))))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Нахождение расстояния до ближайшего юнита, пересеченного лучем
        /// </summary>
        /// <param name="ray">Луч</param>
        /// <returns>Расстояние</returns>
        public float? Intersects(Ray ray)
        {
            if (BoundingBox.HasValue)
                return ray.Intersects(BoundingBox.Value);

            if (Type.Model == null)
                return null;

            float? ret = null;

            foreach (var mesh in Type.Model.Meshes)
            {
                float? dist = ray.Intersects(mesh.BoundingSphere.Transform(Transforms[mesh.ParentBone.Index] * Type.World *
                                                      Matrix.CreateTranslation(Position) * Matrix.CreateScale(Scales)));
                if (dist != null && (ret == null || dist > ret))
                    ret = dist;
            }

            return ret;
        }
        #endregion
        #endregion
    }
}
