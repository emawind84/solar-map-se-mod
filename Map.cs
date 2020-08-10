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
        /// Defines the <see cref="Map" />.
        /// </summary>
        class Map
        {

            private readonly float minX;

            private readonly float maxX;

            private readonly float minZ;

            private readonly float maxZ;

            private readonly Program program;

            public Vector3 StarPosition { get; } = new Vector3(0, 0, -2000000);

            public int StarRadius { get; } = 100000;

            public Boolean Inverted { get; } = false;

            /// <summary>
            /// Gets the CelestialBodies.
            /// </summary>
            public List<CelestialBody> CelestialBodies { get; }
            /// <summary>
            /// Only the planets ordered by distance to render on LCD
            /// </summary>
            public List<CelestialBody> Planets { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Map"/> class.
            /// </summary>
            /// <param name="celestialBodies">The celestialBodies<see cref="List{CelestialBody}"/>.</param>
            public Map(Program program, List<CelestialBody> celestialBodies)
            {
                this.program = program;
                Inverted = program._ini.Get(program.ScriptPrefixTag, "Inverted").ToBoolean(false);
                StarRadius = program._ini.Get(program.ScriptPrefixTag, "StarRadius").ToInt32(100000);
                string starGpsPos = program._ini.Get(program.ScriptPrefixTag, "StarPosition").ToString();
                MyWaypointInfo starGps = new MyWaypointInfo();
                if (MyWaypointInfo.TryParse(starGpsPos, out starGps))
                {
                    StarPosition = starGps.Coords;
                }

                CelestialBodies = celestialBodies;
                Planets = celestialBodies.FindAll(cb => cb.Type == CelestialType.Planet);
                Planets.Sort(SortByDistance);

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
            }

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

            public Vector2 GetMapPosition(Vector3 position, Vector3 centerPosition, int radius = 1)
            {
                if (centerPosition == Vector3.Zero)
                {
                    return GetMapPosition(position);
                }

                var radiusInMeters = radius * 1000;  // in meters
                var minX = centerPosition.X - radiusInMeters;
                var minZ = centerPosition.Z - radiusInMeters;

                Vector2 mapCoordinates = Vector2.Zero;
                mapCoordinates.X = position.X - minX;
                mapCoordinates.Y = position.Z - minZ;
                var range = radiusInMeters * 2;

                mapCoordinates = new Vector2(mapCoordinates.X / range, mapCoordinates.Y / range);
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

            public void AddGPSPosition(GPSInfo gpsinfo)
            {
                var celestialBody = CelestialBodies.Find(obj => obj.Name == gpsinfo.Name);
                if (celestialBody == null)
                {
                    celestialBody = new CelestialBody();
                    CelestialBodies.Add(celestialBody);
                }
                celestialBody.Name = gpsinfo.Name;
                celestialBody.Position = gpsinfo.Position;
                celestialBody.Type = CelestialType.GPS;
            }
        }
    }
}
