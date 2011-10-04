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
            _basicEffect= new BasicEffect(Workarea.Current.Game.GraphicsDevice) { VertexColorEnabled = true};
        }

        public Unit(string typeCode, Map map)
        {         
            TypeCode = typeCode;
            Map = map;
            Health = Type.HealthMax;
            State = 1;
            Impulse = Vector3.Zero;
            Scales = Vector3.One;
            if (Workarea.Current.Game!=null)
                _basicEffect = new BasicEffect(Workarea.Current.Game.GraphicsDevice) { VertexColorEnabled = true };
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
                    Position = new Vector3(Position.X, Map.GetHeight(Position), Position.Z);
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

        private readonly BasicEffect _basicEffect;
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

                        effect.World = GetResultingTransformation(Transforms[mesh.ParentBone.Index]);

                        effect.View = camera.View;
                        effect.Projection = camera.Proj;
                    }
                    mesh.Draw();
                }
            }

            if(Map.Workarea.Settings.DebugMode)
                DrawBoundingBox(camera);
        }

        /// <summary>
        /// Отрисовка ограничивающего параллелепипеда
        /// </summary>
        /// <param name="camera"></param>
        public void DrawBoundingBox(ICamera camera)
        {
            if(!BoundingBox.HasValue)
                   return;

            VertexPositionColor[] vertexData = new VertexPositionColor[12];
            vertexData[0] = new VertexPositionColor(BoundingBox.Value.Min, Color.Red);
            vertexData[1] = new VertexPositionColor(new Vector3(BoundingBox.Value.Max.X, BoundingBox.Value.Min.Y, BoundingBox.Value.Min.Z), Color.Red);

            vertexData[2] = new VertexPositionColor(BoundingBox.Value.Min, Color.Green);
            vertexData[3] = new VertexPositionColor(new Vector3(BoundingBox.Value.Min.X, BoundingBox.Value.Max.Y, BoundingBox.Value.Min.Z), Color.Green);

            vertexData[4] = new VertexPositionColor(BoundingBox.Value.Min, Color.Blue);
            vertexData[5] = new VertexPositionColor(new Vector3(BoundingBox.Value.Min.X, BoundingBox.Value.Min.Y, BoundingBox.Value.Max.Z), Color.Blue);

            vertexData[6] = new VertexPositionColor(BoundingBox.Value.Max, Color.Red);
            vertexData[7] = new VertexPositionColor(new Vector3(BoundingBox.Value.Min.X, BoundingBox.Value.Max.Y, BoundingBox.Value.Max.Z), Color.Red);

            vertexData[8] = new VertexPositionColor(BoundingBox.Value.Max, Color.Green);
            vertexData[9] = new VertexPositionColor(new Vector3(BoundingBox.Value.Max.X, BoundingBox.Value.Min.Y, BoundingBox.Value.Max.Z), Color.Green);

            vertexData[10] = new VertexPositionColor(BoundingBox.Value.Max, Color.Blue);
            vertexData[11] = new VertexPositionColor(new Vector3(BoundingBox.Value.Max.X, BoundingBox.Value.Max.Y, BoundingBox.Value.Min.Z), Color.Blue);

            _basicEffect.View = camera.View;
            _basicEffect.Projection = camera.Proj;
            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Map.Workarea.Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertexData, 0, 6);
            }
        }

        /// <summary>
        /// Основная логика поведения юнита
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            if (Health < Workarea.Eps)
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

                            //TODO:Убрать
                            if(TypeCode=="GHOST1" || TypeCode=="GHOST2")
                                Map.Workarea.Ghost.Play();
                        }
                    }
                }
            }
            #endregion

            if(Map.Workarea.Settings.DebugMode)
                UpdatePhysics(gameTime);
        }

        /// <summary>
        /// Проверка находится ли юнит на земле
        /// </summary>
        /// <returns></returns>
        public bool IsOnGround()
        {
            return Position.Y < Map.GetHeight(Position) + Workarea.Eps;
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
                angleY = -angleY - (float)Math.PI / 2.0f;
            else
                angleY = -angleY + (float)Math.PI / 2.0f;
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
        /// Получает результирующую мировую матрицу
        /// </summary>
        /// <remarks>
        /// Определяет попядок перемножения матриц
        /// </remarks>
        /// <param name="transforms">Дополнительная матрица преобразования для части модели</param>
        /// <returns>Матрица вида</returns>
        public Matrix GetResultingTransformation(Matrix transforms)
        {
            Matrix ret = Matrix.CreateScale(Scales)*
                         Matrix.CreateRotationX(Angles.X)*Matrix.CreateRotationY(Angles.Y)*Matrix.CreateRotationZ(Angles.Z)*
                         transforms*Type.World*Matrix.CreateTranslation(Position);
            return ret;
        }

        /// <summary>
        /// BoundingBox для юнита. Если не задан, то при обработки столконовений будет использована автоматически вычисляемая BoundingSphere
        /// </summary>
        public BoundingBox? BoundingBox
        {
            get
            {
                if (!Type.BoundingBox.HasValue)
                    return null;

                Matrix transform = GetResultingTransformation(Matrix.Identity);

                BoundingBox ret = new BoundingBox(Vector3.Transform(Type.BoundingBox.Value.Min, transform),
                                       Vector3.Transform(Type.BoundingBox.Value.Max, transform));
                
                if(ret.Min.X>ret.Max.X) 
                {
                    float tmp = ret.Min.X;
                    ret.Min.X = ret.Max.X;
                    ret.Max.X = tmp;
                }
                if (ret.Min.Y > ret.Max.Y)
                {
                    float tmp = ret.Min.Y;
                    ret.Min.Y = ret.Max.Y;
                    ret.Max.Y = tmp;
                }
                if (ret.Min.Z > ret.Max.Z)
                {
                    float tmp = ret.Min.Z;
                    ret.Min.Z = ret.Max.Z;
                    ret.Max.Z = tmp;
                }
                return ret;
            }
        }

        /// <summary>
        /// Проверка столкновения юнита с заданной пирамидой вида
        /// </summary>
        /// <param name="boundingFrustum">Пирамида вида</param>
        /// <returns>Истина если пересекаются</returns>
        public bool Intersects(BoundingFrustum boundingFrustum)
        {
            //if (BoundingBox != null)
            //    return boundingFrustum.Intersects(BoundingBox.Value);

            if (Type.Model == null)
                return true;

            return Type.Model.Meshes.Any(mesh => boundingFrustum.Intersects(
                mesh.BoundingSphere.Transform(GetResultingTransformation(Transforms[mesh.ParentBone.Index]))));
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

            return Type.Model == null
                       ? false
                       : Type.Model.Meshes.Any(mesh => boundingSphere.Intersects(
                           mesh.BoundingSphere.Transform(GetResultingTransformation(Transforms[mesh.ParentBone.Index])
                           )));
        }

        /// <summary>
        /// Проверка столкновения юнита с заданным параллелепипедом
        /// </summary>
        /// <param name="boundingBox">Параллелепипед</param>
        /// <returns>Истина если пересекаются</returns>
        public bool Intersects(BoundingBox boundingBox)
        {
            if (BoundingBox.HasValue)
                return BoundingBox.Value.Intersects(boundingBox);

            return Type.Model == null ? false : Type.Model.Meshes.Any(mesh => mesh.BoundingSphere.Transform(
                GetResultingTransformation(Transforms[mesh.ParentBone.Index])
                ).Intersects(boundingBox));
        }

        /// <summary>
        /// Проверка столкновения юнита с заданным
        /// </summary>
        /// <param name="unit">Заданный юнит</param>
        /// <returns>Истина если пересекаются</returns>
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
                    if (mesh.BoundingSphere.Transform(GetResultingTransformation(Transforms[mesh.ParentBone.Index])).Intersects(
                        mesh1.BoundingSphere.Transform(unit.GetResultingTransformation(Transforms[mesh1.ParentBone.Index]))))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Нахождение расстояния до ближайшего части модели, пересеченной лучем
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
                float? dist = ray.Intersects(mesh.BoundingSphere.Transform(GetResultingTransformation(Transforms[mesh.ParentBone.Index])));

                if (dist != null && (ret == null || dist > ret))
                    ret = dist;
            }

            return ret;
        }
        #endregion
        #endregion
    }
}
