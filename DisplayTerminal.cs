using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        /// <summary>
        /// Defines the <see cref="DisplayTerminal" />.
        /// </summary>
        class DisplayTerminal : Terminal<IMyTerminalBlock>
        {
            /// <summary>
            /// Defines the infoSize.
            /// </summary>
            private Vector2 infoSize = new Vector2(175, 20);

            /// <summary>
            /// Defines the infoPosition.
            /// </summary>
            private Vector2 infoPosition = new Vector2(7, 7);

            /// <summary>
            /// Defines the fiveThirdCenter.
            /// </summary>
            private Vector2 fiveThirdCenter = new Vector2(512 / 2, 307.2f / 2 * 1.6f);

            /// <summary>
            /// Defines the gridWorldPosition.
            /// </summary>
            private Vector3 gridWorldPosition;

            /// <summary>
            /// Defines the map.
            /// </summary>
            private readonly Map map;

            /// <summary>
            /// Initializes a new instance of the <see cref="DisplayTerminal"/> class.
            /// </summary>
            /// <param name="program">The program<see cref="Program"/>.</param>
            /// <param name="map">The map<see cref="Map"/>.</param>
            public DisplayTerminal(Program program, Map map) : base(program)
            {
                this.map = map;
            }

            /// <summary>
            /// The OnCycle.
            /// </summary>
            /// <param name="lcd">The lcd<see cref="IMyTextPanel"/>.</param>
            public override void OnCycle(IMyTerminalBlock block)
            {
                base.OnCycle(block);
                // Call the TryParse method on the custom data. This method will
                // return false if the source wasn't compatible with the parser.
                MyIni _ini = new MyIni();
                _ini.TryParse(block.CustomData);
                short display = _ini.Get(Program.ScriptPrefixTag, "Display").ToInt16();
                bool displayGridName = _ini.Get(Program.ScriptPrefixTag, "DisplayGridName").ToBoolean(true);
                bool displayInfoPanel = _ini.Get(Program.ScriptPrefixTag, "DisplayInfoPanel").ToBoolean(true);
                bool displaySun = _ini.Get(Program.ScriptPrefixTag, "DisplaySun").ToBoolean(true);
                bool displayOrbit = _ini.Get(Program.ScriptPrefixTag, "DisplayOrbit").ToBoolean(true);
                bool displayGPS = _ini.Get(Program.ScriptPrefixTag, "DisplayGPS").ToBoolean(false);
                float stretchFactor = _ini.Get(Program.ScriptPrefixTag, "StretchFactor").ToSingle(1);
                float stretchFactorV = _ini.Get(Program.ScriptPrefixTag, "StretchFactorV").ToSingle(1);
                float stretchFactorH = _ini.Get(Program.ScriptPrefixTag, "StretchFactorH").ToSingle(stretchFactor);
                float mapRadius = _ini.Get(Program.ScriptPrefixTag, "MapRadius").ToSingle();
                bool followGrid = _ini.Get(Program.ScriptPrefixTag, "FollowGrid").ToBoolean();
                Vector3 mapCenterPosition = Vector3.Zero;
                MyWaypointInfo centerPosition;
                if (MyWaypointInfo.TryParse(_ini.Get(Program.ScriptPrefixTag, "CenterPosition").ToString(), out centerPosition))
                {
                    mapCenterPosition = centerPosition.Coords;
                }

                gridWorldPosition = Program.Me.GetPosition();
                if (followGrid) mapCenterPosition = gridWorldPosition;

                IMyTextSurface lcd;
                if (block is IMyTextSurfaceProvider)
                {
                    lcd = (block as IMyTextSurfaceProvider).GetSurface(display);
                }
                else
                {
                    lcd = block as IMyTextPanel;
                }

                lcd.ContentType = ContentType.SCRIPT;
                lcd.Script = "";

                RectangleF _viewport = new RectangleF((lcd.TextureSize - lcd.SurfaceSize) / 2f, lcd.SurfaceSize);

                using (MySpriteDrawFrame frame = lcd.DrawFrame())
                {
                    if (GetRefreshCount(block) % 2 == 0) frame.Add(new MySprite());

                    Vector2 positionMult = new Vector2(0.8f / stretchFactorH, 0.8f / stretchFactorV);
                    Vector2 infoPanelOffset = Vector2.Zero;
                    if (displayInfoPanel)
                    {
                        positionMult = new Vector2(0.6f / stretchFactorH, 0.8f / stretchFactorV);
                        infoPanelOffset = new Vector2(180, 0);
                    }
                    Vector2 lcdSize = lcd.SurfaceSize - infoPanelOffset;
                    Vector2 positionOffset = (lcdSize - lcdSize * positionMult) / 2f + infoPanelOffset + _viewport.Position;
                    Vector2 starPosition = map.GetMapPosition(map.StarPosition, mapCenterPosition, mapRadius) * lcdSize * positionMult + positionOffset;

                    {  // display radar style
                        //for (var i = 5; i > 0; i--)
                        //{
                        //    var position = lcdSize * map.GetMapPosition(mapCenterPosition, mapCenterPosition, mapRadius) * positionMult + positionOffset;
                        //    var radarSize = new Vector2(lcd.SurfaceSize.Y * 0.18f * i);
                        //    frame.Add(new MySprite(SpriteType.TEXTURE, "CircleHollow", position, radarSize + 2, new Color(lcd.ScriptForegroundColor, 0.1f)));
                        //    frame.Add(new MySprite(SpriteType.TEXTURE, "CircleHollow", position, radarSize, lcd.ScriptBackgroundColor));
                        //}
                        //frame.Add(new MySprite(SpriteType.TEXTURE, "Grid", starPosition, new Vector2(20), lcd.ScriptForegroundColor));
                    }

                    foreach (CelestialBody celestialBody in map.Planets)
                    {
                        celestialBody.PlanetPosition = map.GetMapPosition(celestialBody.Position, mapCenterPosition, mapRadius) * lcdSize * positionMult + positionOffset;
                        celestialBody.OrbitSize = new Vector2(Vector2.Distance(celestialBody.PlanetPosition, starPosition)) * 2;

                        // Celestial orbits.
                        if (displayOrbit)
                        {
                            // Border and fill.
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", starPosition, celestialBody.OrbitSize + 3, lcd.ScriptForegroundColor));
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", starPosition, celestialBody.OrbitSize, lcd.ScriptBackgroundColor));
                        }
                    }

                    // Celestial bodies.
                    foreach (CelestialBody celestialBody in map.Planets)
                    {
                        celestialBody.PlanetSize = new Vector2(lcd.SurfaceSize.Y * celestialBody.Radius * 0.000001f);
                        celestialBody.LblTitlePos = new Vector2(celestialBody.PlanetPosition.X, celestialBody.PlanetPosition.Y - 40 - celestialBody.PlanetSize.Y * 0.5f);
                        celestialBody.LblDistancePos = new Vector2(celestialBody.PlanetPosition.X, celestialBody.PlanetPosition.Y - 20 - celestialBody.PlanetSize.Y * 0.5f);

                        // Border and fill.
                        frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", celestialBody.PlanetPosition, celestialBody.PlanetSize + 3, lcd.ScriptForegroundColor));
                        frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", celestialBody.PlanetPosition, celestialBody.PlanetSize, lcd.ScriptBackgroundColor));

                        // Text.
                        frame.Add(new MySprite(SpriteType.TEXT, celestialBody.Name, celestialBody.LblTitlePos, null, lcd.ScriptForegroundColor, null, rotation: 0.7f));
                        frame.Add(new MySprite(SpriteType.TEXT, (Vector3.Distance(celestialBody.Position, gridWorldPosition) / 1000).ToString("F1") + " km", celestialBody.LblDistancePos, null, lcd.ScriptForegroundColor, null, rotation: 0.55f));
                    }

                    // Grid dot or arrow.
                    {
                        var position = lcdSize * map.GetMapPosition(gridWorldPosition, mapCenterPosition, mapRadius) * positionMult + positionOffset;
                        if (Program.shipController?.Main != null && !Program.Me.CubeGrid.IsStatic)
                        {
                            // float rotation = -(float)(Math.Acos(program.shipController.Main.WorldMatrix.Forward.Z) + (Math.PI / 2f));
                            float azimuth, elevation;
                            Vector3.GetAzimuthAndElevation(Program.shipController.Main.WorldMatrix.Forward, out azimuth, out elevation);
                            frame.Add(new MySprite(SpriteType.TEXTURE, "AH_BoreSight", position, new Vector2(lcdSize.Y * 0.05f + 3), Color.Red, null, rotation: -azimuth + (float)(Math.PI / 2f)));
                        }
                        else
                        {
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", position, new Vector2(lcdSize.Y * 0.01f), Color.Red));
                        }
                        if (displayGridName)
                        {
                            frame.Add(new MySprite(SpriteType.TEXT, Program.Me.CubeGrid.DisplayName, position - 10, null, Color.Red, null, TextAlignment.RIGHT, 0.55f));
                        }
                    }

                    if (displayGPS)
                    {
                        foreach (var gps in map.CelestialBodies.FindAll(body => body.Type == CelestialType.GPS))
                        {
                            var _gpsPos = map.GetMapPosition(gps.Position, mapCenterPosition, mapRadius) * lcdSize * positionMult + positionOffset;
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", _gpsPos, new Vector2(lcdSize.Y * 0.01f), lcd.ScriptForegroundColor));
                            frame.Add(new MySprite(SpriteType.TEXT, gps.Name, _gpsPos - 10, null, lcd.ScriptForegroundColor, null, TextAlignment.RIGHT, 0.55f));
                        }
                    }

                    if (displayInfoPanel)
                    {
                        int maxColumns = 2;
                        if (lcd.SurfaceSize.X <= 512)
                        {
                            maxColumns = 1;
                        }
                        int infoPanelPerColumn = 6;
                        if (lcd.SurfaceSize.Y < 512)
                        {
                            infoPanelPerColumn = 3;
                        }
                        if (lcd.SurfaceSize.Y > 512)
                        {
                            infoPanelPerColumn = 12;
                        }

                        // Information panel background.
                        int xOffsetIncrement = 190;
                        var planetsAndMoons = map.CelestialBodies.FindAll(obj => obj.Type == CelestialType.Planet || obj.Type == CelestialType.Moon);
                        for (int i = 0; i < Math.Min(Math.Ceiling(planetsAndMoons.Count * 1f / infoPanelPerColumn), maxColumns); i++)
                        {
                            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(95 + i * xOffsetIncrement, lcd.SurfaceSize.Y / 2) + _viewport.Position, new Vector2(183, lcd.SurfaceSize.Y - 5), new Color(0, 0, 0, 50)));
                        }

                        // Information panel content.
                        for (int i = 0; i < planetsAndMoons.Count; i++)
                        {
                            if (i >= infoPanelPerColumn * maxColumns) break;
                            CelestialBody cb = planetsAndMoons[i];

                            int yOffset = 83 * (i % infoPanelPerColumn);
                            int xOffset = i / infoPanelPerColumn * xOffsetIncrement;

                            // Title: Background and text.
                            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(infoPosition.X + (infoSize.X / 2) + xOffset, (infoSize.Y / 2) + infoPosition.Y + yOffset) + _viewport.Position, infoSize, new Color(0, 0, 0, 150)));
                            frame.Add(new MySprite(SpriteType.TEXT, cb.Name, new Vector2(3 + infoPosition.X + xOffset, infoPosition.Y + yOffset) + _viewport.Position, null, lcd.ScriptForegroundColor, null, TextAlignment.LEFT, 0.6f));
                            frame.Add(new MySprite(SpriteType.TEXT, (Vector3.Distance(cb.Position, gridWorldPosition) / 1000).ToString("F1") + " km", new Vector2(172 + infoPosition.X + xOffset, 2 + infoPosition.Y + yOffset) + _viewport.Position, null, lcd.ScriptForegroundColor, null, TextAlignment.RIGHT, 0.5f));

                            // Body: Content.
                            string txt = "Radius: " + (cb.Radius / 1000).ToString("F1") + " km\n" +
                                        "Gravity: " + cb.Gravity.ToString("F1") + " G\n" +
                                        "Atmosphere: " + cb.HasAtmosphere + "\n" +
                                        "Oxygen: " + cb.Oxygen + "\n" +
                                        "Resources: " + cb.Resources + "\n";
                            frame.Add(new MySprite(SpriteType.TEXT, txt, new Vector2(3 + infoPosition.X + xOffset, infoPosition.Y + infoSize.Y + yOffset) + _viewport.Position, null, lcd.ScriptForegroundColor, null, TextAlignment.LEFT, 0.4f));

                        }
                    }

                    if (displaySun)
                    {
                        // Drawing the Sun
                        frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", starPosition, new Vector2(lcd.SurfaceSize.Y * map.StarRadius * 0.000001f), Color.Yellow));
                    }

                }
            }

            /// <summary>
            /// The Collect.
            /// </summary>
            /// <param name="terminal">The terminal<see cref="IMyTextPanel"/>.</param>
            /// <returns>The <see cref="bool"/>.</returns>
            public override bool Collect(IMyTerminalBlock terminal)
            {
                // Collect this.
                bool isSolarmap = terminal.IsSameConstructAs(Program.Me)
                    && MyIni.HasSection(terminal.CustomData, Program.ScriptPrefixTag)
                    && (terminal is IMyTextPanel || terminal is IMyTextSurfaceProvider)
                    && terminal.IsWorking
                    && terminal != Program.Me;

                //program.EchoR(string.Format("Collecting {0} {1}", terminal.CustomName, isSolarmap));

                // Return.
                return isSolarmap;
            }
        }
    }
}
