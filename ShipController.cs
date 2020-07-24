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
    /// <summary>
    /// Defines the <see cref="ShipController" />.
    /// </summary>
    class ShipController : Terminal<IMyShipController>
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
            base.OnCycle(terminal);
            Main = terminal;
        }

        public override bool Collect(IMyShipController terminal)
        {
            if (IsListEmpty)
            {
                return base.Collect(terminal);
            }
            return false;
        }
    }
}
