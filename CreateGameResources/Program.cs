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
            #region Workarea
            Workarea workarea = new Workarea();
            workarea.UnitTypes.Add(new UnitType
                                       {
                                           Name = "Игрок",
                                           Code = "PLAYER",
                                           HealthMax = 100f,
                                           DamageMin = 1f,
                                           DamageMax = 30f,
                                           Speed = 3f,
                                           VisibilityRange = 100f,
                                           AttackRange = 1f,
                                           AttackDelay = 1f
                                       });
            workarea.UnitTypes.Add(new UnitType
                                       {
                                           Name = "Крепостной",
                                           Code = "PEASANT",
                                           HealthMax = 100f,
                                           DamageMin = 1f,
                                           DamageMax = 30f,
                                           Speed = 0.1f,
                                           VisibilityRange = 100f,
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
                                           HealthMax = 300f,
                                           DamageMin = 0f,
                                           DamageMax = 0f,
                                           Speed = 1f,
                                           VisibilityRange = 5f,
                                           AttackRange = 2f,
                                           AttackDelay = 0f,
                                           Scale = 0.001f
                                       });
            workarea.UnitTypes.Add(new UnitType
                                       {
                                           Name = "Дом",
                                           Code = "HOUSE1",
                                           HealthMax = 300f,
                                           Scale = 1f
                                       });
            workarea.Save(Path.Combine(OutPath, "Workarea.xml"));
            #endregion

            #region Map
            Map map = new Map(workarea) { Name = "Город", Width = 50, Height = 50, Depth = 10, FogEnabled = true, FogColor = new Vector3(0.5f, 0.5f, 0.5f) };

            map.Heightmap = new float[(int)(map.Width + 1) * (int)(map.Height + 1)];
            Random r = new Random();
            for (int i = 0; i < (map.Width + 1) * (map.Height + 1); i++)
            {
                map.Heightmap[i] = r.Next(3) / 10.0f;
            }

            map.Units.Add(new Unit("PLANE1", map)
                              {
                                  Name = "Самолет1",
                                  Fraction = 2,
                                  Position = new Vector3(7f, 0.5f, 9f),
                                  Angles = new Vector3(0f, 0f, 0f),
                              });
            map.Units.Add(new Unit("HOUSE1", map)
                              {
                                  Name = "Дом1",
                                  Fraction = 0,
                                  Position = new Vector3(7f, 0f, 5f),
                                  Angles = Vector3.Zero
                              });
            map.Save(Path.Combine(OutPath, "Map1.xml"));
            #endregion

            #region Settings

            Settings settings = new Settings()
            {
                ScreenWidth = 800,
                ScreenHeight = 600,
                FullScreen = false,
                ForStart = 5f,
                FogEnd = 50f,
                EnableDefaultLighting = true,
                IsFixedTimeStep = true,
                SynchronizeWithVerticalRetrace = true,
                MouseSpeedX = 4,
                MouseSpeedY = 4
            };
            settings.Save(Path.Combine(OutPath, "Settings.xml"));
            #endregion
        }
    }
}
