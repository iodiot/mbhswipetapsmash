﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.GameObject;

namespace BumpSetSpike.Behaviour.FSM
{
    /// <summary>
    /// Pause menu.
    /// </summary>
    class FSMPauseScreen : MBHEngine.Behaviour.FiniteStateMachine
    {
        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="parentGOH"></param>
        /// <param name="fileName"></param>
        public FSMPauseScreen(GameObject parentGOH, String fileName)
            : base(parentGOH, fileName)
        {
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="fileName"></param>
        public override void LoadContent(string fileName)
        {
            base.LoadContent(fileName);

            AddState(new StatePauseRoot(), "StatePauseRoot");
        }
    }
}
