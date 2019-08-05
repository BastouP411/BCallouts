using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using LSPD_First_Response.Engine.Scripting.Entities;


namespace BCallouts.Callouts
{
    [CalloutInfo("CriminalEscort", CalloutProbability.High)]
    class CriminalEscort : Callout
    {
        private Ped Criminal;
        private Ped FVDriver;
        private Ped CVDriver;
        private Vehicle FollowVeh;
        private Vehicle CriminalVeh;
        private Blip CriminalBlip;
        private Vector3 SpawnPoint;


        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(200f));
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(20f, SpawnPoint);
            CalloutMessage = "Criminal Escort";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GRAND_THEFT_AUTO IN_OR_ON_POSITION", SpawnPoint);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {

            CriminalVeh = new Vehicle("SCHAFTER5", SpawnPoint)
            {
                IsPersistent = true
            };

            FollowVeh = new Vehicle("GAUNTLET", CriminalVeh.Position.Around(15f))
            {
                IsPersistent = true
            };

            FVDriver = FollowVeh.CreateRandomDriver();
            FVDriver.IsPersistent = true;
            FVDriver.BlockPermanentEvents = true;
            CVDriver = CriminalVeh.CreateRandomDriver();
            CVDriver.IsPersistent = true;
            CVDriver.BlockPermanentEvents = true;

            CriminalBlip = CVDriver.AttachBlip();
            CriminalBlip.IsFriendly = false;

            CVDriver.Tasks.CruiseWithVehicle(15f, VehicleDrivingFlags.Normal);
            NativeFunction.CallByName<uint>("TASK_VEHICLE_ESCORT", FVDriver, FollowVeh, CriminalVeh, -1, 25f, 2883620, 10, 0, 25f); // Native: _TASK_VEHICLE_FOLLOW
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            if(!CVDriver.Exists() || CVDriver.IsDead)
            {
                End();
            }
        }

        public override void End()
        {
            base.End();
            if (CriminalBlip.Exists()) { CriminalBlip.Delete(); }
            if (FVDriver.Exists()) { FVDriver.Dismiss(); }
            if (CVDriver.Exists()) { CVDriver.Dismiss(); }
            if (CriminalVeh.Exists()) { CriminalVeh.Dismiss(); }
        }
    }
}
