using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using LSPD_First_Response.Engine.Scripting.Entities;


namespace BCallouts.Callouts
{
    [CalloutInfo("Criminal Escort", CalloutProbability.Low)]
    class CriminalEscort : Callout
    {
        private Ped Criminal;
        private Ped CVDriver;
        private Ped FVDriver;
        private Ped FVBrute;
        private Vehicle FollowVeh;
        private Vehicle CriminalVeh;
        private Blip CriminalBlip;
        private Blip FVDBlip;
        private Blip FVBBlip;
        private LHandle Pursuit;
        private Vector3 SpawnPoint;
        private float SpawnHeading;
        private bool PursuitStarted;

        public override bool OnBeforeCalloutDisplayed()
        {

            int WaitCount = 0;
            while (!World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(500f, 5000f)).GetClosestVehicleSpawnPoint(out SpawnPoint, out SpawnHeading))
            {
                GameFiber.Yield();
                WaitCount++;
                if (WaitCount > 10) { return false; }
            }
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint.Around(0f, 25f), 50f);
            CalloutMessage = "Criminal Escort";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS WE_HAVE CRIME_SUSPECT_ON_THE_RUN IN_OR_ON_POSITION", SpawnPoint);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            PursuitStarted = false;

            CriminalVeh = new Vehicle("SCHAFTER5", SpawnPoint)
            {
                IsPersistent = true,
                Heading = SpawnHeading
            };

            Vector3 vectRear = CriminalVeh.RearPosition - CriminalVeh.Position;

            FollowVeh = new Vehicle("GAUNTLET", SpawnPoint + ((vectRear / vectRear.Length()) * 7))
            {
                IsPersistent = true,
                Heading = SpawnHeading
            };

            FVDriver = FollowVeh.CreateRandomDriver();
            FVDriver.IsPersistent = true;
            FVDriver.BlockPermanentEvents = true;
            FVDriver.RelationshipGroup = "CRIMINAL";
            FVDriver.Inventory.GiveNewWeapon(WeaponHash.CombatPistol, -1, true);
            Functions.SetVehicleOwnerName(FollowVeh, Functions.GetPersonaForPed(FVDriver).FullName);

            CVDriver = CriminalVeh.CreateRandomDriver();
            CVDriver.IsPersistent = true;
            CVDriver.BlockPermanentEvents = true;
            CVDriver.RelationshipGroup = "CRIMINAL";
            CVDriver.Inventory.GiveNewWeapon(WeaponHash.CombatPistol, -1, true);
            Functions.SetVehicleOwnerName(CriminalVeh, Functions.GetPersonaForPed(CVDriver).FullName);

            Criminal = new Ped
            {
                IsPersistent = true,
                BlockPermanentEvents = true,
                RelationshipGroup = "CRIMINAL"
            };
            Criminal.Inventory.GiveNewWeapon(WeaponHash.MicroSMG, -1, true);
            Criminal.WarpIntoVehicle(CriminalVeh, MathHelper.GetRandomInteger(1, 2));

            FVBrute = new Ped
            {
                IsPersistent = true,
                BlockPermanentEvents = true,
                RelationshipGroup = "CRIMINAL"
            };
            FVBrute.Inventory.GiveNewWeapon(WeaponHash.MicroSMG, -1, true);
            FVBrute.WarpIntoVehicle(FollowVeh, 0);

            Persona CrimPersona = Functions.GetPersonaForPed(Criminal);
            CrimPersona.Wanted = true;
            Functions.SetPersonaForPed(Criminal, CrimPersona);

            CVDriver.Tasks.CruiseWithVehicle(15f, VehicleDrivingFlags.Normal);
            NativeFunction.CallByName<uint>("TASK_VEHICLE_ESCORT", FVDriver, FollowVeh, CriminalVeh, -1, 50f, 1074528293, 5, 0, 25f); // Native: TASK_VEHICLE_ESCORT

            Blipper();

            return base.OnCalloutAccepted();
        }

        private void Blipper()
        {
            GameFiber.StartNew(delegate
            {
                while (!PursuitStarted && Criminal.Exists())
                {
                    if (CriminalBlip.Exists()) { CriminalBlip.Delete(); }
                    CriminalBlip = new Blip(Criminal.Position.Around(0f, 25f), 75f)
                    {
                        IsFriendly = false,
                        Color = Color.Gold,
                        IsRouteEnabled = true,
                        Alpha = 0.5f
                    };
                    GameFiber.Sleep(60000);
                }
            });
        }

        public override void Process()
        {
            base.Process();
            if ((!CVDriver.Exists() || CVDriver.IsDead || Functions.IsPedArrested(CVDriver)) && (!FVBrute.Exists() || FVBrute.IsDead || Functions.IsPedArrested(FVBrute)) && (!FVDriver.Exists() || FVDriver.IsDead || Functions.IsPedArrested(FVDriver)) && (!Criminal.Exists() || Criminal.IsDead || Functions.IsPedArrested(Criminal)))
            {
                End();
            }

            if ((!FVDriver.Exists() || FVDriver.IsDead || Functions.IsPedArrested(FVDriver)) && (!FVBrute.Exists() || FVBrute.IsDead || Functions.IsPedArrested(FVBrute)) && PursuitStarted && !Functions.IsPursuitStillRunning(Pursuit))
            {
                End();
            }

            if (PursuitStarted && !Functions.IsPursuitStillRunning(Pursuit) && ((FVBrute.Exists() && !FVBrute.IsDead && !Functions.IsPedArrested(FVBrute) && FVBrute.Position.DistanceTo(Game.LocalPlayer.Character.Position) > 400f) || !FVBrute.Exists() || FVBrute.IsDead || Functions.IsPedArrested(FVBrute)) && ((FVDriver.Exists() && !FVDriver.IsDead && !Functions.IsPedArrested(FVDriver) && FVDriver.Position.DistanceTo(Game.LocalPlayer.Character.Position) > 400f) || !FVDriver.Exists() || FVDriver.IsDead || Functions.IsPedArrested(FVDriver)))
            {
                End();
            }

            if (PursuitStarted && !Functions.IsPursuitStillRunning(Pursuit))
            {
                if (CriminalBlip.Exists()) { CriminalBlip.Delete(); }
                if (CVDriver.Exists()) { CVDriver.Dismiss(); }
                if (Criminal.Exists()) { Criminal.Dismiss(); }
                if (CriminalVeh.Exists()) { CriminalVeh.Dismiss(); }
            }

            if (!PursuitStarted && Game.LocalPlayer.Character.Position.DistanceTo(CVDriver.Position) < 15)
            {
                StartPursuit();
            }

            if ((!CVDriver.Exists() || CVDriver.IsDead || Functions.IsPedArrested(CVDriver)) && CriminalBlip.Exists())
            {
                CriminalBlip.Delete();
            }

            if ((!FVDriver.Exists() || FVDriver.IsDead || Functions.IsPedArrested(FVDriver)) && FVDBlip.Exists())
            {
                FVDBlip.Delete();
            }
            if ((!FVBrute.Exists() || FVBrute.IsDead || Functions.IsPedArrested(FVBrute)) && FVBBlip.Exists())
            {
                FVBBlip.Delete();
            }
        }

        public override void End()
        {
            base.End();
            if (CriminalBlip.Exists()) { CriminalBlip.Delete(); }
            if (FVDBlip.Exists()) { FVDBlip.Delete(); }
            if (FVDriver.Exists()) { FVDriver.Dismiss(); }
            if (FVBrute.Exists()) { FVBrute.Dismiss(); }
            if (CVDriver.Exists()) { CVDriver.Dismiss(); }
            if (Criminal.Exists()) { Criminal.Dismiss(); }
            if (CriminalVeh.Exists()) { CriminalVeh.Dismiss(); }
            if (FollowVeh.Exists()) { FollowVeh.Dismiss(); }
        }

        private void StartPursuit()
        {
            if (!PursuitStarted)
            {
                Game.SetRelationshipBetweenRelationshipGroups("COP", "CRIMINAL", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("CRIMINAL", "COP", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("CRIMINAL", "PLAYER", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("PLAYER", "CRIMINAL", Relationship.Hate);

                if (Functions.GetActivePursuit() != null) { Functions.ForceEndPursuit(Functions.GetActivePursuit()); }
                Pursuit = Functions.CreatePursuit();

                if (CVDriver.Exists() && !CVDriver.IsDead)
                {
                    CVDriver.Tasks.Clear();
                    Functions.AddPedToPursuit(Pursuit, CVDriver);
                }

                if (FVDriver.Exists() && !FVDriver.IsDead)
                {
                    FVDriver.Tasks.Clear();
                    FVDBlip = FVDriver.AttachBlip();
                    FVDBlip.IsFriendly = false;
                    FVDBlip.Scale = 0.75f;
                    FVDriver.Tasks.FightAgainst(Game.LocalPlayer.Character);
                }

                if (FVBrute.Exists() && !FVBrute.IsDead)
                {
                    FVBBlip = FVBrute.AttachBlip();
                    FVBBlip.IsFriendly = false;
                    FVBBlip.Scale = 0.75f;
                    FVBrute.Tasks.FightAgainst(Game.LocalPlayer.Character);
                }

                if (Criminal.Exists() && !Criminal.IsDead)
                {
                    Functions.AddPedToPursuit(Pursuit, Criminal);
                }

                if (CriminalBlip.Exists())
                {
                    CriminalBlip.Delete();
                }

                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                PursuitStarted = true;
            }
        }
    }
}
