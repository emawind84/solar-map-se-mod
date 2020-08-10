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
        T FindFirstBlockOfType<T>(Func<T, bool> collect) where T : class, IMyTerminalBlock
        {
            var blocks = new List<T>();
            GridTerminalSystem.GetBlocksOfType(blocks, blk => collect(blk));
            return blocks.Count() > 0 ? blocks[0] : null;
        }

        void RetrieveCustomSetting()
        {
            // init settings
            _ini.TryParse(Me.CustomData);

            string _gpsBroadcastTag = _ini.Get(ScriptPrefixTag, "GPSBroadcastTag").ToString();
            if (_gpsBroadcastTag != "")
            {
                GPSBroadcastTag = _gpsBroadcastTag;
            }
            
        }

        /// <summary>
        /// Defines the CelestialType.
        /// </summary>
        enum CelestialType
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
            Moon,
            /// <summary>
            /// GPS Coordinates
            /// </summary>
            GPS
        }

        /// <summary>
        /// Defines the Oxygen.
        /// </summary>
        enum Oxygen
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
        /// Thrown when we detect that we have taken up too much processing time
        /// and need to put off the rest of the exection until the next call.
        /// </summary>
        class PutOffExecutionException : Exception { }

        /// <summary>
        /// The SetTerminalCycle.
        /// </summary>
        /// <returns>The <see cref="IEnumerator{bool}"/>.</returns>
        IEnumerator<bool> SetTerminalCycle()
        {
            while (true)
            {
                yield return shipController.Run();
                yield return textPanel.Run();
            }
        }

        /// <summary>
        /// Defines the <see cref="ProgrammableBlock" />.
        /// </summary>
        class ProgrammableBlock
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
            public ProgrammableBlock(Program program)
            {
                this.program = program;
                short display = program._ini.Get(program.ScriptPrefixTag, "DebugDisplay").ToInt16();

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

                textSurface.ContentType = ContentType.TEXT_AND_IMAGE;
                textSurface.WriteText(program.echoOutput);
            }
        }

        struct GPSInfo
        {
            public long ID { get; set; }
            public string Name { get; set; }
            public Vector3D Position { get; set; }
            public DateTime Created { get; set; }

            public bool IsRecent => DateTime.Now - this.Created < new TimeSpan(0, 0, 30);

            public override string ToString()
            {
                return $"{this.Name}" + Environment.NewLine +
                    $"Position: {this.Position}" + Environment.NewLine;
            }
        }
    }
}
