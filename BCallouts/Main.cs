using BCallouts.Common;
using BCallouts.Managers;
using LSPD_First_Response.Mod.API;
using Rage;


namespace BCallouts
{
    public class Main : Plugin
    {

        public override void Initialize()
        {
            ConfigManager.UpdateConfig();
            LoadIPLs();
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            DataCrackManager.Initialize();
            Game.LogTrivial("[BCallouts] BCallouts " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " has been initialised.");

        }

        public override void Finally()
        {
            RemoveIPLs();
            DataCrackManager.Finally();
            AircraftManager.Finally();
            Game.LogTrivial("BCallouts has been cleaned up.");
        }

        private void OnOnDutyStateChangedHandler(bool onDuty)
        {

            if (onDuty)
            {
                AircraftManager.Initialize();
                RegisterCallouts();
            } 
            else
            {
                AircraftManager.Finally();
            }
        }

        private void RegisterCallouts()
        {
            Functions.RegisterCallout(typeof(Callouts.CriminalEscort));
            Functions.RegisterCallout(typeof(Callouts.AlienAttack));
            Functions.RegisterCallout(typeof(Callouts.StolenAircraft));
        }

        private void LoadIPLs() {
            Natives.RequestIPL("hei_carrier");
            Natives.RequestIPL("hei_carrier_DistantLights");
            Natives.RequestIPL("hei_Carrier_int1");
            Natives.RequestIPL("hei_Carrier_int2");
            Natives.RequestIPL("hei_Carrier_int3");
            Natives.RequestIPL("hei_Carrier_int4");
            Natives.RequestIPL("hei_Carrier_int5");
            Natives.RequestIPL("hei_Carrier_int6");
            Natives.RequestIPL("hei_carrier_LODLights");
        }

        private void RemoveIPLs()
        {
            Natives.RemoveIPL("hei_carrier");
            Natives.RemoveIPL("hei_carrier_DistantLights");
            Natives.RemoveIPL("hei_Carrier_int1");
            Natives.RemoveIPL("hei_Carrier_int2");
            Natives.RemoveIPL("hei_Carrier_int3");
            Natives.RemoveIPL("hei_Carrier_int4");
            Natives.RemoveIPL("hei_Carrier_int5");
            Natives.RemoveIPL("hei_Carrier_int6");
            Natives.RemoveIPL("hei_carrier_LODLights");
        }
    }
}
