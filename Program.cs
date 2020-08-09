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

        string IniSectionKey = "SolarMap";

        const string GPSBroadcastTag = "SHUTTLE_STATE";

        /// <summary>
        /// whether to use real time (second between calls) or pure UpdateFrequency
        /// for update frequency
        /// </summary>
        readonly bool USE_REAL_TIME = false;
        /// <summary>
        /// Defines the FREQUENCY.
        /// </summary>
        private const UpdateFrequency FREQUENCY = UpdateFrequency.Update100;
        /// <summary>
        /// How often the script should update in milliseconds
        /// </summary>
        const int UPDATE_REAL_TIME = 1000;
        /// <summary>
        /// The maximum run time of the script per call.
        /// Measured in milliseconds.
        /// </summary>
        const double MAX_RUN_TIME = 35;
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
        private readonly DisplayTerminal textPanel;

        private MyIni _ini = new MyIni();

        /// <summary>
        /// A wrapper for the <see cref="Echo"/> function that adds the log to the stored log.
        /// This allows the log to be remembered and re-outputted without extra work.
        /// </summary>
        public Action<string> EchoR;
        
        Map CelestialMap { get; set; }

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
        const int VERSION_MAJOR = 1, VERSION_MINOR = 2, VERSION_REVISION = 0;
        /// <summary>
        /// Current script update time.
        /// </summary>
        const string VERSION_UPDATE = "2020-08-10";
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
        /// <summary>
        /// The time to wait before starting the next cycle.
        /// Only used if <see cref="USE_REAL_TIME"/> is <c>true</c>.
        /// </summary>
        TimeSpan cycleUpdateWaitTime = new TimeSpan(0, 0, 0, 0, UPDATE_REAL_TIME);
        /// The total number of calls this script has had since compilation.
        /// </summary>
        long totalCallCount;
        /// <summary>
        /// The text to echo at the start of each call.
        /// </summary>
        string scriptUpdateText;
        /// <summary>
        /// The current step in the TIM process cycle.
        /// </summary>
        int processStep;
        /// <summary>
        /// All of the process steps that TIM will need to take,
        /// </summary>
        readonly Action[] processSteps;
        /// <summary>
        /// Stores the output of Echo so we can effectively ignore some calls
        /// without overwriting it.
        /// </summary>
        public StringBuilder echoOutput = new StringBuilder();

        IMyBroadcastListener BroadcastListener { get; }

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

            // initialise the process steps we will need to do
            processSteps = new Action[]
            {
                ProcessStepCheckBroadcastMessages
            };

            Runtime.UpdateFrequency = FREQUENCY;

            CelestialMap = new Map(this, celestialBodies);
            programmableBlock = new ProgrammableBlock(this);
            shipController = new ShipController(this);
            textPanel = new DisplayTerminal(this, CelestialMap);

            this.BroadcastListener = this.IGC.RegisterBroadcastListener(GPSBroadcastTag);
            this.BroadcastListener.SetMessageCallback();

            terminalCycle = SetTerminalCycle();

            EchoR(string.Format("Compiled {0} {1}", SCRIPT_NAME, VERSION_NICE_TEXT));

            // format terminal info text
            scriptUpdateText = string.Format(FORMAT_UPDATE_TEXT, SCRIPT_NAME, VERSION_NICE_TEXT);
        }

        /// <summary>
        /// The Main.
        /// </summary>
        /// <param name="arg">The arg<see cref="string"/>.</param>
        /// <param name="updateType">The updateType<see cref="UpdateType"/>.</param>
        public void Main(string arg, UpdateType updateType)
        {
            if (USE_REAL_TIME)
            {
                DateTime n = DateTime.Now;
                if (n - currentCycleStartTime >= cycleUpdateWaitTime)
                    currentCycleStartTime = n;
                else
                {
                    Echo(echoOutput.ToString()); // ensure that output is not lost
                    return;
                }
            }
            else
            {
                currentCycleStartTime = DateTime.Now;
            }

            echoOutput.Clear();

            // output terminal info
            EchoR(string.Format(scriptUpdateText, ++totalCallCount, currentCycleStartTime.ToString("h:mm:ss tt")));

            if (processStep == processSteps.Length)
            {
                processStep = 0;
            }
            int processStepTmp = processStep;
            bool didAtLeastOneProcess = false;

            try
            {
                processSteps[processStep]();
                didAtLeastOneProcess = true;
            }
            catch (PutOffExecutionException) { }
            catch (Exception ex)
            {
                // if the process step threw an exception, make sure we print the info
                // we need to debug it
                string err = "An error occured,\n" +
                    "please give the following information to the developer:\n" +
                    string.Format("Current step on error: {0}\n{1}", processStep, ex.ToString().Replace("\r", ""));
                EchoR(err);
                throw ex;
            }

            if (!terminalCycle.MoveNext())
                terminalCycle.Dispose();

            //EchoR(string.Format("Instructions: {0}", Runtime.CurrentInstructionCount + "/" + Runtime.MaxInstructionCount));
            //EchoR(string.Format("Call chain: {0}", Runtime.CurrentCallChainDepth + "/" + Runtime.MaxCallChainDepth));
            //EchoR(string.Format("Execution time: {0}", Runtime.LastRunTimeMs.ToString("F2") + " ms"));

            if (shipController.Main == null)
            {
                EchoR("Status\n- Controller does not exist.");
            }

            string stepText;
            int theoryProcessStep = processStep == 0 ? processSteps.Count() : processStep;
            int exTime = ExecutionTime;
            double exLoad = Math.Round(100.0f * ExecutionLoad, 1);
            if (processStep == 0 && processStepTmp == 0 && didAtLeastOneProcess)
                stepText = "all steps";
            else if (processStep == processStepTmp)
                stepText = string.Format("step {0} partially", processStep);
            else if (theoryProcessStep - processStepTmp == 1)
                stepText = string.Format("step {0}", processStepTmp);
            else
                stepText = string.Format("steps {0} to {1}", processStepTmp, theoryProcessStep - 1);
            EchoR(string.Format("Completed {0} in {1}ms\n{2}% load ({3} instructions)",
                stepText, exTime, exLoad, Runtime.CurrentInstructionCount));

            programmableBlock.Draw();
        }
        
        void ProcessStepCheckBroadcastMessages()
        {
            if (this.BroadcastListener.HasPendingMessage)
            {
                EchoR("Received broadcast message");
                MyIGCMessage message = this.BroadcastListener.AcceptMessage();
                try
                {
                    var data = message.As<MyTuple<long, string, Vector3D, string>>();
                    GPSInfo gpsmessage = new GPSInfo()
                    {
                        ID = data.Item1,
                        Name = data.Item2,
                        Position = data.Item3,
                        Created = DateTime.Now
                    };
                    CelestialMap.AddGPSPosition(gpsmessage);
                }
                catch { }
            }
        }
        
    }

}
