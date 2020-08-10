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
        /// Defines the <see cref="CelestialBody" />.
        /// </summary>
        class CelestialBody
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

        class DetectedEntity : CelestialBody
        {
            public DateTime Detected;
            
            public DetectedEntity() : base()
            {
                Detected = DateTime.Now;
            }
        }
    }
}
