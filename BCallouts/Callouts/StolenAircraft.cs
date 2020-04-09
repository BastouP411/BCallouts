using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;
using System;

namespace BCallouts.Callouts
{
    [CalloutInfo("Stolen Aircraft", CalloutProbability.Medium)]
    class StolenAircraft : Callout {

        private readonly string[] ModelList = new string[] { "BUZZARD2", "MAVERICK", "FROGGER"};

        private Ped Criminal;
        private Vehicle Aircraft;
        private Vector3 SpawnPoint;
        private LHandle Pursuit;

        public override bool OnBeforeCalloutDisplayed() {
            if(!Game.LocalPlayer.Character.IsInAirVehicle) {
                return false;
            }

            SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(500f, 600f)).ZOffset(150f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint.Around(0f, 25f), 50f);
            CalloutMessage = "Stolen Aircraft";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS WE_HAVE CRIME_STOLEN_AIRCRAFT IN_OR_ON_POSITION", SpawnPoint);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted() {
            Random rdm = new Random();
            Aircraft = new Vehicle(ModelList[rdm.Next(0, ModelList.Length)], SpawnPoint);
            Aircraft.MakePersistent();
            Criminal = Aircraft.CreateRandomDriver();
            Criminal.MakePersistent();
            Criminal.BlockPermanentEvents = true;
            if(Functions.GetActivePursuit() != null) {
                Functions.ForceEndPursuit(Functions.GetActivePursuit());
            }

            Pursuit = Functions.CreatePursuit();
            Functions.AddPedToPursuit(Pursuit, Criminal);
            Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
            return base.OnCalloutAccepted();
        }

        public override void Process() {
            if(!Functions.IsPursuitStillRunning(Pursuit)) {
                End();
            }
            base.Process();
        }

        public override void End() {
            if (Criminal.Exists()) { Criminal.Dismiss(); }
            if (Aircraft.Exists()) { Aircraft.Dismiss(); }
            base.End();
        }
    }
}
