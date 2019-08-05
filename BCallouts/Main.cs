using System;
using LSPD_First_Response.Mod.API;
using Rage;

namespace BCallouts
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("BCallouts " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " has been initialised.");
        }

        public override void Finally()
        {
            Game.LogTrivial("BCallouts has been cleaned up.");
        }

        private void OnOnDutyStateChangedHandler(bool onDuty)
        {
            if (onDuty)
            {
                RegisterCallouts();
            }
        }

        private void RegisterCallouts()
        {
            Functions.RegisterCallout(typeof(Callouts.CriminalEscort));
        }
    }
}
