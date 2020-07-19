namespace IngameScript
{
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

    /// <summary>
    /// Defines the <see cref="Program" />.
    /// </summary>
    internal partial class Program : MyGridProgram
    {
        /// <summary>
        /// Defines the FREQUENCY.
        /// </summary>
        private const UpdateFrequency FREQUENCY = UpdateFrequency.Update1;

        /// <summary>
        /// Defines the terminalCycle.
        /// </summary>
        private readonly IEnumerator<bool> terminalCycle;

        /// <summary>
        /// Defines the programmableBlock.
        /// </summary>
        private readonly ProgrammableBlock programmableBlock;

        /// <summary>
        /// Defines the shipController.
        /// </summary>
        private readonly ShipController shipController;

        /// <summary>
        /// Defines the textPanel.
        /// </summary>
        private readonly TextPanel textPanel;

        private MyIni _ini = new MyIni();

        private readonly string IniSectionKey = "SolarMap";

        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        public Program()
        {

            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
                throw new Exception(result.ToString());

            // ---------------------------------------------------------------
            // Celestial bodies - Start.
            // ---------------------------------------------------------------
            List<CelestialBody> celestialBodies = new List<CelestialBody>()
            {
                new CelestialBody
                {
                    Name = "Helion I",
                    Radius = 60000,
                    Gravity = 1,
                    HasAtmosphere = true,
                    Oxygen = Oxygen.High,
                    Type = CelestialType.Planet,
                    Position = new Vector3(0.5f, 0, 0.5f),
                    Resources = "All"
                },

                new CelestialBody
                {
                    Name = "Xindus",
                    Radius = 9500,
                    Gravity = 0.25f,
                    HasAtmosphere = false,
                    Oxygen = Oxygen.None,
                    Type = CelestialType.Moon,
                    Position = new Vector3(16384.5f, 0f, -113615.5f),
                    Resources = "All"
                },

                new CelestialBody
                {
                    Name = "Helion II",
                    Radius = 60000,
                    Gravity = 0.9f,
                    HasAtmosphere = true,
                    Oxygen = Oxygen.None,
                    Type = CelestialType.Planet,
                    Position = new Vector3(1031072.5f, 0f, 1631072.5f),
                    Resources = "All"
                },

                new CelestialBody
                {
                    Name = "Hiigara",
                    Radius = 9500,
                    Gravity = 0.25f,
                    HasAtmosphere = true,
                    Oxygen = Oxygen.None,
                    Type = CelestialType.Moon,
                    Position = new Vector3(916384.5f, 0f, 1616384.5f),
                    Resources = "All"
                },

                new CelestialBody
                {
                    Name = "Helion III",
                    Radius = 60000,
                    Gravity = 1.1f,
                    HasAtmosphere = true,
                    Oxygen = Oxygen.Low,
                    Type = CelestialType.Planet,
                    Position = new Vector3(131072.5f, 0f, 5731072.5f),
                    Resources = "All"
                },

                new CelestialBody
                {
                    Name = "Miranda",
                    Radius = 9500,
                    Gravity = 0.25f,
                    HasAtmosphere = true,
                    Oxygen = Oxygen.None,
                    Type = CelestialType.Moon,
                    Position = new Vector3(36384.5f, 0f, 5796384.5f),
                    Resources = "All"
                }
                /*,

                new CelestialBody
                {
                    Name = "Unknown",
                    Radius = 120000,
                    Gravity = 1,
                    HasAtmosphere = false,
                    Oxygen = Oxygen.None,
                    Type = CelestialType.Planet,
                    Position = new Vector3(3199494, 0, 8121258),
                    Resources = "All"
                }
                */
            };
            // ---------------------------------------------------------------
            // Celestial bodies - End.
            // ---------------------------------------------------------------

            programmableBlock = new ProgrammableBlock(this, FREQUENCY);
            shipController = new ShipController(this);
            textPanel = new TextPanel(this, new Map(this, celestialBodies));

            terminalCycle = SetTerminalCycle();

        }

        /// <summary>
        /// Defines the CelestialType.
        /// </summary>
        public enum CelestialType
        {
            /// <summary>
            /// Defines the Asteroid.
            /// </summary>
            Asteroid,
            /// <summary>
            /// Defines the Planet.
            /// </summary>
            Planet,
            /// <summary>
            /// Defines the Moon.
            /// </summary>
            Moon
        }

        /// <summary>
        /// Defines the Oxygen.
        /// </summary>
        public enum Oxygen
        {
            /// <summary>
            /// Defines the None.
            /// </summary>
            None,
            /// <summary>
            /// Defines the Low.
            /// </summary>
            Low,
            /// <summary>
            /// Defines the High.
            /// </summary>
            High
        }

        /// <summary>
        /// The SetTerminalCycle.
        /// </summary>
        /// <returns>The <see cref="IEnumerator{bool}"/>.</returns>
        private IEnumerator<bool> SetTerminalCycle()
        {
            while (true)
            {
                yield return shipController.Run();
                yield return textPanel.Run();
            }
        }

        /// <summary>
        /// The Main.
        /// </summary>
        /// <param name="arg">The arg<see cref="string"/>.</param>
        /// <param name="updateType">The updateType<see cref="UpdateType"/>.</param>
        public void Main(string arg, UpdateType updateType)
        {
            // The update type is binary. Must look it up on Malware's wikia to figure out how to manipulate it.
            if ((updateType & Update[FREQUENCY]) != 0)
            {

                if (!terminalCycle.MoveNext())
                    terminalCycle.Dispose();

                programmableBlock.Draw();

            }
        }

        /// <summary>
        /// Common behavior to all terminals.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        abstract public class Terminal<T> where T : class, IMyTerminalBlock
        {
            /// <summary>
            /// Defines the program.
            /// </summary>
            protected Program program;

            /// <summary>
            /// Defines the list.
            /// </summary>
            private readonly List<T> list = new List<T>();

            /// <summary>
            /// Defines the listIndex, listUpdate...
            /// </summary>
            private int listIndex, listUpdate;

            /// <summary>
            /// Initializes a new instance of the <see cref="Terminal{T}"/> class.
            /// </summary>
            /// <param name="program">The program<see cref="Program"/>.</param>
            public Terminal(Program program)
            {
                this.program = program;
            }

            /// <summary>
            /// Gets a value indicating whether IsListEmpty.
            /// </summary>
            public bool IsListEmpty => list.Count == 0;

            /// <summary>
            /// The Run.
            /// </summary>
            /// <returns>The <see cref="bool"/>.</returns>
            public bool Run()
            {

                if (listIndex < list.Count)
                {

                    if (list.Count == 0)
                        return true;

                    if (!IsCorrupt(list[listIndex]))
                    {
                        OnCycle(list[listIndex]);
                    }
                    else
                    {
                        list.Remove(list[listIndex]);
                    }

                    listIndex++;

                }
                else
                {

                    // Update terminals of this type roughly every ~20 seconds when update frequency is 10.
                    if (listUpdate % (60 / (list.Count + 1)) == 0)
                    {
                        program.GridTerminalSystem.GetBlocksOfType(list, Collect);
                    }

                    listUpdate++;
                    listIndex = 0;

                }

                return true;
            }

            /// <summary>
            /// Specificies which terminals to be collected. Use override.
            /// </summary>
            /// <param name="terminal">The terminal<see cref="T"/>.</param>
            /// <returns>The <see cref="bool"/>.</returns>
            public virtual bool Collect(T terminal)
            {
                return true;
            }

            /// <summary>
            /// The OnCycle.
            /// </summary>
            /// <param name="terminal">The terminal<see cref="T"/>.</param>
            public abstract void OnCycle(T terminal);

            /// <summary>
            /// Checks if the terminal is null, gone from world, or broken off from grid.
            /// </summary>
            /// <param name="block">The block<see cref="T"/>.</param>
            /// <returns>The <see cref="bool"/>.</returns>
            public bool IsCorrupt(T block)
            {
                if (block == null || block.WorldMatrix == MatrixD.Identity) return true;
                return !(program.GridTerminalSystem.GetBlockWithId(block.EntityId) == block);
            }
        }

        /// <summary>
        /// Defines the <see cref="CelestialBody" />.
        /// </summary>
        public class CelestialBody
        {
            /// <summary>
            /// Defines the Type.
            /// </summary>
            public CelestialType Type;

            /// <summary>
            /// Defines the Oxygen.
            /// </summary>
            public Oxygen Oxygen;

            /// <summary>
            /// Defines the Position.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Defines the HasAtmosphere.
            /// </summary>
            public bool HasAtmosphere;

            /// <summary>
            /// Defines the Radius.
            /// </summary>
            public float Radius;

            /// <summary>
            /// Defines the Gravity.
            /// </summary>
            public float Gravity;

            /// <summary>
            /// Defines the Name.
            /// </summary>
            public string Name;

            /// <summary>
            /// Defines the Resources.
            /// </summary>
            public string Resources;
            
            /// <summary>
            /// Defines the PlanetPosition.
            /// </summary>
            public Vector2 PlanetPosition;

            /// <summary>
            /// Defines the OrbitSize.
            /// </summary>
            public Vector2 OrbitSize;

            /// <summary>
            /// Defines the PlanetSize.
            /// </summary>
            public Vector2 PlanetSize;

            /// <summary>
            /// Defines the LblTitlePos.
            /// </summary>
            public Vector2 LblTitlePos;

            /// <summary>
            /// Defines the LblDistancePos.
            /// </summary>
            public Vector2 LblDistancePos;
        }

        /// <summary>
        /// Defines the <see cref="Map" />.
        /// </summary>
        private class Map
        {

            private readonly float minX;

            private readonly float maxX;

            private readonly float minZ;

            private readonly float maxZ;

            private readonly Program program;

            /// <summary>
            /// Initializes a new instance of the <see cref="Map"/> class.
            /// </summary>
            /// <param name="celestialBodies">The celestialBodies<see cref="List{CelestialBody}"/>.</param>
            public Map(Program program, List<CelestialBody> celestialBodies)
            {
                this.program = program;
                Inverted = program._ini.Get(program.IniSectionKey, "Inverted").ToBoolean(false);
                StarRadius = program._ini.Get(program.IniSectionKey, "StarRadius").ToInt32(100000);
                string starGpsPos = program._ini.Get(program.IniSectionKey, "StarPosition").ToString();
                MyWaypointInfo starGps = new MyWaypointInfo();
                if (MyWaypointInfo.TryParse(starGpsPos, out starGps))
                {
                    StarPosition = starGps.Coords;
                }
                
                CelestialBodies = celestialBodies;
                CelestialInfo = new List<CelestialBody>(celestialBodies);

                minX = StarPosition.X;
                maxX = StarPosition.X;
                minZ = StarPosition.Z;
                maxZ = StarPosition.Z;

                foreach (CelestialBody planet in CelestialBodies)
                {

                    // Sets the maximum distance of the solar system.
                    maxX = planet.Position.X > maxX ? planet.Position.X : maxX;
                    maxZ = planet.Position.Z > maxZ ? planet.Position.Z : maxZ;

                    minX = planet.Position.X < minX ? planet.Position.X : minX;
                    minZ = planet.Position.Z < minZ ? planet.Position.Z : minZ;
                    
                }

                foreach (CelestialBody celestialBody in CelestialBodies)
                {

                    // Set planet position relative to a 2D surface.
                    //celestialBody.MapPosition = GetMapPosition(celestialBody.Position);

                    // Find nearest planet.
                    if (celestialBody.Type == CelestialType.Moon)
                    {
                        float distance = 0;
                        foreach (CelestialBody planet in CelestialBodies)
                        {
                            if (planet.Type == CelestialType.Planet)
                            {
                                float dist = Vector3.Distance(celestialBody.Position, planet.Position);
                                if (dist < distance)
                                {
                                    distance = dist;
                                }
                            }
                        }
                    }
                }

                CelestialBodies.Sort(SortByDistance);
                //CelestialBodies.Sort(SortByType);
            }

            public Vector3 StarPosition { get; } = new Vector3(0, 0, -100000);

            public int StarRadius { get; } = 100000;

            public Boolean Inverted { get; } = false;

            /// <summary>
            /// Gets the CelestialBodies.
            /// </summary>
            public List<CelestialBody> CelestialBodies { get; }

            /// <summary>
            /// Gets the CelestialInfo.
            /// </summary>
            public List<CelestialBody> CelestialInfo { get; }

            /// <summary>
            /// The GetMapPosition.
            /// </summary>
            /// <param name="position">The position<see cref="Vector3"/>.</param>
            /// <returns>The <see cref="Vector2"/>.</returns>
            public Vector2 GetMapPosition(Vector3 position)
            {
                Vector2 mapCoordinates = Vector2.Zero;
                mapCoordinates.X = position.X - minX;
                mapCoordinates.Y = position.Z - minZ;
                float rangeX = maxX - minX;
                float rangeZ = maxZ - minZ;

                mapCoordinates = new Vector2(mapCoordinates.X / rangeX, mapCoordinates.Y / rangeZ);
                if (Inverted) mapCoordinates = Vector2.One - mapCoordinates;
                return mapCoordinates;
            }

            /// <summary>
            /// The SortByDistance.
            /// </summary>
            /// <param name="a">The a<see cref="CelestialBody"/>.</param>
            /// <param name="b">The b<see cref="CelestialBody"/>.</param>
            /// <returns>The <see cref="int"/>.</returns>
            private int SortByDistance(CelestialBody a, CelestialBody b)
            {
                float distanceA = Vector2.Distance(new Vector2(a.Position.X, a.Position.Z), new Vector2(StarPosition.X, StarPosition.Z));
                float distanceB = Vector2.Distance(new Vector2(b.Position.X, b.Position.Z), new Vector2(StarPosition.X, StarPosition.Z));
                if (distanceA > distanceB)
                    return -1;
                else if (distanceB > distanceA)
                    return 1;
                return 0;
            }

            /// <summary>
            /// The SortByType.
            /// </summary>
            /// <param name="a">The a<see cref="CelestialBody"/>.</param>
            /// <param name="b">The b<see cref="CelestialBody"/>.</param>
            /// <returns>The <see cref="int"/>.</returns>
            private int SortByType(CelestialBody a, CelestialBody b)
            {
                if (a.Type == CelestialType.Moon && b.Type == CelestialType.Planet)
                    return -1;
                else if (b.Type == CelestialType.Moon && a.Type == CelestialType.Planet)
                    return 1;
                return 0;
            }
        }

        /// <summary>
        /// Defines the <see cref="ShipController" />.
        /// </summary>
        private class ShipController : Terminal<IMyShipController>
        {
            /// <summary>
            /// Defines the main.
            /// </summary>
            private IMyShipController main;

            /// <summary>
            /// Initializes a new instance of the <see cref="ShipController"/> class.
            /// </summary>
            /// <param name="program">The program<see cref="Program"/>.</param>
            public ShipController(Program program) : base(program)
            {
            }

            /// <summary>
            /// Gets or sets the Main.
            /// </summary>
            public IMyShipController Main
            {
                get
                {
                    return IsListEmpty || (main != null && (main.WorldMatrix == MatrixD.Identity || !main.IsWorking)) ? null : main;
                }
                set
                {
                    main = value;
                }
            }

            /// <summary>
            /// The OnCycle.
            /// </summary>
            /// <param name="terminal">The terminal<see cref="IMyShipController"/>.</param>
            public override void OnCycle(IMyShipController terminal)
            {
                if (terminal.IsWorking)
                {
                    if (Main == null)
                    {
                        Main = terminal;
                    }
                    if (terminal.IsMainCockpit)
                    {
                        Main = terminal;
                    }
                }
            }

            public override bool Collect(IMyShipController terminal)
            {
                return base.Collect(terminal);
            }
        }

        /// <summary>
        /// Defines the <see cref="TextPanel" />.
        /// </summary>
        private class TextPanel : Terminal<IMyTerminalBlock>
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
            /// Initializes a new instance of the <see cref="TextPanel"/> class.
            /// </summary>
            /// <param name="program">The program<see cref="Program"/>.</param>
            /// <param name="map">The map<see cref="Map"/>.</param>
            public TextPanel(Program program, Map map) : base(program)
            {
                this.map = map;
            }

            /// <summary>
            /// The OnCycle.
            /// </summary>
            /// <param name="lcd">The lcd<see cref="IMyTextPanel"/>.</param>
            public override void OnCycle(IMyTerminalBlock block)
            {
                // Call the TryParse method on the custom data. This method will
                // return false if the source wasn't compatible with the parser.
                MyIniParseResult resultIniParse;
                MyIni _ini = new MyIni();
                if (!_ini.TryParse(block.CustomData, out resultIniParse))
                    throw new Exception(resultIniParse.ToString());
                short display = _ini.Get(program.IniSectionKey, "Display").ToInt16();
                bool displayGridName = _ini.Get(program.IniSectionKey, "DisplayGridName").ToBoolean(true);
                bool displayInfoPanel = _ini.Get(program.IniSectionKey, "DisplayInfoPanel").ToBoolean(true);
                bool displaySun = _ini.Get(program.IniSectionKey, "DisplaySun").ToBoolean(true);
                short stretchFactor = _ini.Get(program.IniSectionKey, "StretchFactor").ToInt16(1);

                gridWorldPosition = program.Me.GetPosition();

                IMyTextSurface lcd;
                if (block is IMyTextSurfaceProvider)
                {
                    lcd = (block as IMyTextSurfaceProvider).GetSurface(display);
                }
                else
                {
                    lcd = block as IMyTextPanel;
                }

                RectangleF _viewport = new RectangleF((lcd.TextureSize - lcd.SurfaceSize) / 2f, lcd.SurfaceSize);

                using (MySpriteDrawFrame frame = lcd.DrawFrame())
                {

                    Vector2 positionMult = new Vector2(0.8f / stretchFactor, 0.8f);
                    Vector2 infoPanelOffset = Vector2.Zero;
                    if (displayInfoPanel)
                    {
                        positionMult = new Vector2(0.6f / stretchFactor, 0.8f);
                        infoPanelOffset = new Vector2(180, 0);
                    }
                    Vector2 lcdSize = lcd.SurfaceSize - infoPanelOffset;
                    Vector2 positionOffset = (lcdSize - lcdSize * positionMult) / 2f + infoPanelOffset + _viewport.Position;
                    Vector2 starPosition = map.GetMapPosition(map.StarPosition) * lcdSize * positionMult + positionOffset;
                    
                    // Celestial orbits.
                    foreach (CelestialBody celestialBody in map.CelestialBodies.Where(cb => cb.Type == CelestialType.Planet))
                    {
                        celestialBody.PlanetPosition = map.GetMapPosition(celestialBody.Position) * lcdSize * positionMult + positionOffset;
                        celestialBody.OrbitSize = new Vector2(Vector2.Distance(celestialBody.PlanetPosition, starPosition)) * 2;

                        // Border and fill.
                        frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", starPosition, celestialBody.OrbitSize + 3, lcd.ScriptForegroundColor));
                        frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", starPosition, celestialBody.OrbitSize, lcd.ScriptBackgroundColor));
                    }

                    // Celestial bodies.
                    foreach (CelestialBody celestialBody in map.CelestialBodies.Where(cb => cb.Type == CelestialType.Planet))
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
                    if (program.shipController != null)
                    {
                        Vector2 position = lcdSize * map.GetMapPosition(gridWorldPosition) * positionMult + positionOffset;
                        if (program.shipController.Main != null && !program.Me.CubeGrid.IsStatic)
                        {
                            // float rotation = -(float)(Math.Acos(program.shipController.Main.WorldMatrix.Forward.Z) + (Math.PI / 2f));
                            float azimuth, elevation;
                            Vector3.GetAzimuthAndElevation(program.shipController.Main.WorldMatrix.Forward, out azimuth, out elevation);
                            frame.Add(new MySprite(SpriteType.TEXTURE, "AH_BoreSight", position, new Vector2(lcdSize.Y * 0.05f + 3), Color.Red, null, rotation: -azimuth + (float)(Math.PI / 2f)));
                        }
                        else
                        {
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", position, new Vector2(lcdSize.Y * 0.01f), Color.Red));
                        }
                        if (displayGridName)
                        {
                            frame.Add(new MySprite(SpriteType.TEXT, program.Me.CubeGrid.DisplayName, position - 10, null, Color.Red, null, TextAlignment.RIGHT, 0.5f));
                        }
                    }

                    if (displayInfoPanel)
                    {
                        int maxColumns = 2;
                        if (lcd.SurfaceSize.X <= 512)
                        {
                            maxColumns = 1;
                        }
                        // Information panel background.
                        int infoPanelPerColumn = 6;
                        if (lcd.SurfaceSize.Y < 512)
                        {
                            infoPanelPerColumn = 3;
                        }
                        if (lcd.SurfaceSize.Y > 512)
                        {
                            infoPanelPerColumn = 12;
                        }

                        int xOffsetIncrement = 190;
                        for (int i = 0; i < Math.Min(Math.Ceiling(map.CelestialInfo.Count * 1f / infoPanelPerColumn), maxColumns); i++)
                        {
                            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(95 + i * xOffsetIncrement, 256) + _viewport.Position, new Vector2(183, 505), new Color(0, 0, 0, 50)));
                        }

                        // Information panel content.
                        for (int i = 0; i < map.CelestialInfo.Count; i++)
                        {
                            if (i >= infoPanelPerColumn * maxColumns) break;
                            CelestialBody cb = map.CelestialInfo[i];

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

                    // SHOW OMG
                    //float azimuth, elevation;
                    //Vector3.GetAzimuthAndElevation(program.shipController.Main.WorldMatrix.Forward, out azimuth, out elevation);
                    //frame.Add(new MySprite(SpriteType.TEXT, (azimuth).ToString(), new Vector2(250, 250), null, null, null, rotation: 0.7f));

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
                bool isSolarmap = terminal.IsSameConstructAs(program.Me) && terminal.CustomData.Contains("[SolarMap]") && (terminal is IMyTextPanel || terminal is IMyTextSurfaceProvider);
                isSolarmap &= terminal.IsWorking && terminal != program.Me;
                
                // Set the content type of this terminal to script.
                if (isSolarmap)
                {
                    if (terminal is IMyTextSurfaceProvider)
                    {
                        (terminal as IMyTextSurfaceProvider).GetSurface(0).ContentType = ContentType.SCRIPT;
                    }
                    else if (terminal is IMyTextPanel)
                    {
                        (terminal as IMyTextPanel).ContentType = ContentType.SCRIPT;
                    }
                }

                // Return.
                return isSolarmap;
            }

            
        }

        /// <summary>
        /// Defines the <see cref="ProgrammableBlock" />.
        /// </summary>
        private class ProgrammableBlock
        {
            /// <summary>
            /// Defines the program.
            /// </summary>
            protected Program program;

            /// <summary>
            /// Defines the textSurface.
            /// </summary>
            private readonly IMyTextSurface textSurface;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProgrammableBlock"/> class.
            /// </summary>
            /// <param name="program">The program<see cref="Program"/>.</param>
            /// <param name="updateFrequency">The updateFrequency<see cref="UpdateFrequency"/>.</param>
            public ProgrammableBlock(Program program, UpdateFrequency updateFrequency = UpdateFrequency.Update1)
            {
                this.program = program;
                program.Runtime.UpdateFrequency = updateFrequency;

                MyIniParseResult resultIniParse;
                MyIni _ini = new MyIni();
                if (!_ini.TryParse(program.Me.CustomData, out resultIniParse))
                    throw new Exception(resultIniParse.ToString());
                short display = _ini.Get(program.IniSectionKey, "Display").ToInt16();

                textSurface = program.Me.GetSurface(display);
                textSurface.ContentType = ContentType.SCRIPT;
            }

            /// <summary>
            /// The Draw.
            /// </summary>
            public void Draw()
            {

                if (program.Me.CubeGrid.GridSizeEnum == MyCubeSize.Small)
                    return;

                // Calculate the viewport offset by centering the surface size onto the texture size
                RectangleF _viewport = new RectangleF(
                    (textSurface.TextureSize - textSurface.SurfaceSize) / 2f,
                    textSurface.SurfaceSize
                );

                using (MySpriteDrawFrame frame = textSurface.DrawFrame())
                {

                    frame.Add(new MySprite(SpriteType.TEXT, "SolarMap v0.9", new Vector2(10, 10) + _viewport.Position, null, Color.White, null, TextAlignment.LEFT, 0.7f));
                    frame.Add(new MySprite(SpriteType.TEXT, "Instructions:\nCall chain:\nExecution time:", new Vector2(10, 35) + _viewport.Position, null, Color.White, null, TextAlignment.LEFT, 0.6f));

                    frame.Add(new MySprite(SpriteType.TEXT,
                        program.Runtime.CurrentInstructionCount + "/" + program.Runtime.MaxInstructionCount + "\n" +
                        program.Runtime.CurrentCallChainDepth + "/" + program.Runtime.MaxCallChainDepth + "\n" +
                        program.Runtime.LastRunTimeMs.ToString("F2") + " ms\n", new Vector2(200, 35) + _viewport.Position, null, Color.White, null, TextAlignment.LEFT, 0.6f));

                    if (program.shipController.Main == null)
                    {
                        frame.Add(new MySprite(SpriteType.TEXT, "Status", new Vector2(10, 110) + _viewport.Position, null, Color.White, null, TextAlignment.LEFT, 0.7f));
                        frame.Add(new MySprite(SpriteType.TEXT, "- Controller does not exist.", new Vector2(10, 135) + _viewport.Position, null, Color.White, null, TextAlignment.LEFT, 0.6f));
                    }

                }
            }
        }

        /// <summary>
        /// Defines the Update.
        /// </summary>
        private readonly Dictionary<UpdateFrequency, UpdateType> Update = new Dictionary<UpdateFrequency, UpdateType>
        {
            { UpdateFrequency.Update1, UpdateType.Update1 },
            { UpdateFrequency.Update10, UpdateType.Update10 },
            { UpdateFrequency.Update100, UpdateType.Update100 }
        };

        
    }

}
