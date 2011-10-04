using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Game3;
using Microsoft.Xna.Framework;

namespace CreateGameResources
{
    class Program
    {
        private const string OutPath = @"..\..\..\Game3\Game3\bin\x86\Debug\";
        static void Main(string[] args)
        {
            Settings settings = CreateSettings();
            Workarea workarea = CreateWorkarea(settings);
            CreateMapCity(workarea);
            CreateMapCemetery(workarea);
        }

        private static Settings CreateSettings()
        {
            Settings settings = new Settings()
            {
                ScreenWidth = 800,
                ScreenHeight = 600,
                FullScreen = false,
                IsFixedTimeStep = true,
                SynchronizeWithVerticalRetrace = true,
                MouseSpeedX = 4,
                MouseSpeedY = 4,
                DebugMode = true
            };
            settings.Save(Path.Combine(OutPath, "Settings.xml"));
            return settings;
        }

        private static Workarea CreateWorkarea(Settings settings)
        {
            Workarea workarea = new Workarea {Settings = settings};
            Workarea.Current = workarea;

            workarea.UnitTypes.Add(new UnitType
            {
                Name = "Игрок",
                Code = "PLAYER",
                HealthMax = 100f,
                DamageMin = 20f,
                DamageMax = 30f,
                Speed = 3f,
                VisibilityRange = 10f,
                AttackRange = 10f,
                AttackDelay = 1f,
            });
            workarea.UnitTypes.Add(new UnitType
            {
                Name = "Крепостной",
                Code = "PEASANT",
                HealthMax = 100f,
                DamageMin = 1f,
                DamageMax = 30f,
                Speed = 0.1f,
                VisibilityRange = 10f,
                AttackRange = 1f,
                AttackDelay = 1f
            });
            workarea.UnitTypes.Add(new UnitType
            {
                Name = "Воин",
                Code = "WARRIOR",
                HealthMax = 300f,
                DamageMin = 10f,
                DamageMax = 20f,
                Speed = 1f,
                VisibilityRange = 10f,
                AttackRange = 1f,
                AttackDelay = 1f
            });
            workarea.UnitTypes.Add(new UnitType
            {
                Name = "Самолет",
                Code = "PLANE1",
                HealthMax = 100f,
                DamageMin = 0f,
                DamageMax = 10f,
                Speed = 1f,
                VisibilityRange = 5f,
                AttackRange = 2f,
                AttackDelay = 1f,
                World = Matrix.CreateScale(0.001f)/*Matrix.CreateRotationY((float)(-Math.PI/2))*/* Matrix.CreateTranslation(0f, 1f, 0f),
                IsFlyable = true
            });
            workarea.UnitTypes.Add(new UnitType
            {
                Name = "Призрак1",
                Code = "GHOST1",
                HealthMax = 70f,
                DamageMin = 0f,
                DamageMax = 15f,
                Speed = 1f,
                VisibilityRange = 5f,
                AttackRange = 2f,
                AttackDelay = 3f,
                //World = Matrix.CreateScale(0.001f)/*Matrix.CreateRotationY((float)(-Math.PI/2))*/* Matrix.CreateTranslation(0f, 1f, 0f),
                IsFlyable = true
            });
            workarea.UnitTypes.Add(new UnitType
            {
                Name = "Призрак2",
                Code = "GHOST2",
                HealthMax = 100f,
                DamageMin = 0f,
                DamageMax = 30f,
                Speed = 2f,
                VisibilityRange = 7f,
                AttackRange = 3f,
                AttackDelay = 3f,
                //World = Matrix.CreateScale(0.001f)/*Matrix.CreateRotationY((float)(-Math.PI/2))*/* Matrix.CreateTranslation(0f, 1f, 0f),
                IsFlyable = true
            });
            workarea.UnitTypes.Add(new UnitType
            {
                Name = "Дом",
                Code = "HOUSE1",
                HealthMax = 300f,
                IsBreakable = false,
                BoundingBox = new BoundingBox(new Vector3(0f, 0f, -3f), new Vector3(7f, 4.35f, 0f))
            });
            workarea.UnitTypes.Add(new UnitType
            {
                Name = "Могила",
                Code = "GRAVE1",
                HealthMax = 300f,
                BoundingBox = new BoundingBox(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.9f, 1f, 0.3f))
            });
            workarea.Save(Path.Combine(OutPath, "Workarea.xml"));
            return workarea;
        }
        
        private static void CreateMapCity(Workarea workarea)
        {
            Map map = new Map(workarea)
            {
                Name = "Город",
                Sizes = new Vector3(50f,50f,50f),
                Gravity = new Vector3(0f, -9.8f, 0f),

                FogEnabled = true,
                ForStart = 5f,
                FogEnd = 50f,
                FogColor = new Vector3(0.5f, 0.5f, 0.5f),
                              
                EnableDefaultLighting = true
            };

            map.Heightmap = new float[(int)(map.Sizes.X + 1) * (int)(map.Sizes.Y + 1)];
            Random r = new Random();
            for (int i = 0; i < (map.Sizes.X + 1) * (map.Sizes.Y + 1); i++)
            {
                map.Heightmap[i] = r.Next(3) / 10.0f;
            }
            map.PickUpLandscape(map.Sizes/2, 2f);
            
            map.Units.Add(new Unit("PLANE1", map)
            {
                Name = "Самолет1",
                Fraction = 2,
                Position = new Vector3(7f, 2.0f, 9f),
                Angles = new Vector3(0f, 0f, 0f),
            });
            map.Units.Add(new Unit("HOUSE1", map)
            {
                Name = "Дом1",
                Fraction = 0,
                Position = new Vector3(7f, 0f, 5f),
                Angles = Vector3.Zero
            });
            map.Save(Path.Combine(OutPath, "Maps\\City.xml"));
        }

        private static void CreateMapCemetery(Workarea workarea)
        {
            Map map = new Map(workarea)
            {
                Name = "Кладбище",
                Sizes = new Vector3(50f,50f,50f),
                Gravity = new Vector3(0f, -5f, 0f),

                FogEnabled = true,
                ForStart = 1f,
                FogEnd = 50f,
                FogColor = Color.Black.ToVector3(),
                //FogColor = Color.CornflowerBlue.ToVector3(),

                //EnableDefaultLighting = true
                LightingEnabled = true,
                DirectionalLight0 = new MapLight { DiffuseColor = Color.Red.ToVector3(), Direction = Vector3.One, Enabled = true, SpecularColor = Color.Red.ToVector3() }
            };

            map.Heightmap = new float[(int)(map.Sizes.X + 1) * (int)(map.Sizes.Y + 1)];
            Random r = new Random();
            for (int i = 0; i < (map.Sizes.X + 1) * (map.Sizes.Y + 1); i++)
            {
                map.Heightmap[i] = r.Next(3) / 10.0f;
            }
            //map.PickUpLandscape((int)map.Width / 2, (int)map.Height / 2, 5f);

            //map.Units.Add(new Unit("PLANE1", map)
            //{
            //    Name = "Самолет1",
            //    Fraction = 2,
            //    Position = new Vector3(7f, 2.0f, 9f),
            //    Angles = new Vector3(0f, 0f, 0f),
            //    //Scales = new Vector3(10f,10f,10f)
            //});
            map.Units.Add(new Unit("HOUSE1", map)
            {
                Name = "Дом1",
                Fraction = 0,
                Position = new Vector3(7f, 0f, 5f),
                Angles = Vector3.Zero
            });

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    map.Units.Add(new Unit("GRAVE1", map)
                    {
                        Name = "Могила" + (i * 5 + j).ToString("G"),
                        Fraction = 0,
                        Position = new Vector3(15f + i * 5, 0f, 15f + j * 5),
                        Angles = new Vector3((float)r.NextDouble() / 5 - 0.1f, (float)r.NextDouble() / 5 - 0.1f - (float)Math.PI / 2f, (float)r.NextDouble() / 5 - 0.1f),
                        Scales = new Vector3((float)r.NextDouble() + 0.5f, (float)r.NextDouble() + 0.5f, (float)r.NextDouble() + 0.5f)
                    });
                }
            }

            for (int i = 0; i < 10; i++)
            {
                map.Units.Add(new Unit("GHOST"+r.Next(1,3), map)
                {
                    Name = "Призрак"+i,
                    Fraction = 2,
                    Position = new Vector3((float)r.NextDouble() * map.Sizes.X, (float)r.NextDouble() * 5f, (float)r.NextDouble() * map.Sizes.Y)
                });
            }
            
            map.Save(Path.Combine(OutPath, "Maps\\Cemetery.xml"));
        }
    }
}
