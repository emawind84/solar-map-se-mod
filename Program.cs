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

        #region CelestialBodies

        private List<CelestialBody> celestialBodies = new List<CelestialBody>()
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

        #endregion

        /// <summary>
        /// Defines the FREQUENCY.
        /// </summary>
        private const UpdateFrequency FREQUENCY = UpdateFrequency.Update100;

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
        /// A wrapper for the <see cref="Echo"/> function that adds the log to the stored log.
        /// This allows the log to be remembered and re-outputted without extra work.
        /// </summary>
        public Action<string> EchoR;
        /// <summary>
        /// Stores the output of Echo so we can effectively ignore some calls
        /// without overwriting it.
        /// </summary>
        public StringBuilder echoOutput = new StringBuilder();

        #region Properties

        /// <summary>
        /// The length of time we have been executing for.
        /// Measured in milliseconds.
        /// </summary>
        int ExecutionTime
        {
            get { return (int)((DateTime.Now - currentCycleStartTime).TotalMilliseconds + 0.5); }
        }

        /// <summary>
        /// The current percent load of the call.
        /// </summary>
        double ExecutionLoad
        {
            get { return Runtime.CurrentInstructionCount / Runtime.MaxInstructionCount; }
        }

        #endregion

        #region Version
        
        const string SCRIPT_NAME = "ED's SolarMap";
        // current script version
        const int VERSION_MAJOR = 1, VERSION_MINOR = 1, VERSION_REVISION = 0;
        /// <summary>
        /// Current script update time.
        /// </summary>
        const string VERSION_UPDATE = "2020-07-24";
        /// <summary>
        /// A formatted string of the script version.
        /// </summary>
        readonly string VERSION_NICE_TEXT = string.Format("v{0}.{1}.{2} ({3})", VERSION_MAJOR, VERSION_MINOR, VERSION_REVISION, VERSION_UPDATE);

        #endregion

        #region Format Strings

        /// <summary>
        /// The format for the text to echo at the start of each call.
        /// </summary>
        const string FORMAT_UPDATE_TEXT = "{0}\n{1}\nLast run: #{{0}} at {{1}}";

        #endregion

        #region Script state & storage

        /// The time we started the last cycle at.
        /// If <see cref="USE_REAL_TIME"/> is <c>true</c>, then it is also used to track
        /// when the script should next update
        /// </summary>
        DateTime currentCycleStartTime;
        /// The total number of calls this script has had since compilation.
        /// </summary>
        long totalCallCount;
        /// <summary>
        /// The text to echo at the start of each call.
        /// </summary>
        string scriptUpdateText;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        public Program()
        {

            // init echo wrapper
            EchoR = log =>
            {
                echoOutput.AppendLine(log);
                Echo(log);
            };

            // init settings
            _ini.TryParse(Me.CustomData);
            
            Runtime.UpdateFrequency = FREQUENCY;

            programmableBlock = new ProgrammableBlock(this);
            shipController = new ShipController(this);
            textPanel = new TextPanel(this, new Map(this, celestialBodies));

            terminalCycle = SetTerminalCycle();

            EchoR(string.Format("Compiled {0} {1}", SCRIPT_NAME, VERSION_NICE_TEXT));

            // format terminal info text
            scriptUpdateText = string.Format(FORMAT_UPDATE_TEXT, SCRIPT_NAME, VERSION_NICE_TEXT);
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
            currentCycleStartTime = DateTime.Now;

            echoOutput.Clear();

            // output terminal info
            EchoR(string.Format(scriptUpdateText, ++totalCallCount, currentCycleStartTime.ToString("h:mm:ss tt")));

            if (!terminalCycle.MoveNext())
                terminalCycle.Dispose();

            EchoR(string.Format("Instructions: {0}", Runtime.CurrentInstructionCount + "/" + Runtime.MaxInstructionCount));
            EchoR(string.Format("Call chain: {0}", Runtime.CurrentCallChainDepth + "/" + Runtime.MaxCallChainDepth));
            EchoR(string.Format("Execution time: {0}", Runtime.LastRunTimeMs.ToString("F2") + " ms"));

            if (shipController.Main == null)
            {
                EchoR("Status\n- Controller does not exist.");
            }

            int exTime = ExecutionTime;
            double exLoad = Math.Round(100.0f * ExecutionLoad, 1);
            string stepText = "all steps";
            EchoR(string.Format("Completed {0} in {1}ms\n{2}% load ({3} instructions)",
                stepText, exTime, exLoad, Runtime.CurrentInstructionCount));

            programmableBlock.Draw();
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
            public ProgrammableBlock(Program program)
            {
                this.program = program;
                short display = program._ini.Get(program.IniSectionKey, "DebugDisplay").ToInt16();

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
        
    }

}
