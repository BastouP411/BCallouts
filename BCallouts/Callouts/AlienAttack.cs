using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;
using System.Drawing;
using System;

namespace BCallouts.Callouts
{
    [CalloutInfo("Alien Attack", CalloutProbability.Low)]
    class AlienAttack : Callout
    {
        private Vector3 SpawnPoint;
        private Ped Alien;
        private Blip ZoneBlip;
        private Blip AlienBlip;

        private bool IsFake;
        private bool SpawnSequenceInitiated;
        private bool AlienSpawned;
        private bool TargetFlees;

        private Random Rdm;
        private LHandle Pursuit;

        public override bool OnBeforeCalloutDisplayed()
        {
            int WaitCount = 0;
            while (!World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(2000f, 5000f)).GetSafeCoordForPed(false, out SpawnPoint))
            {
                GameFiber.Yield();
                WaitCount++;
                if (WaitCount > 50) { return false; }
            }
            Rdm = new Random(DateTime.UtcNow.Millisecond);
            IsFake = Rdm.NextDouble() < 0.9f;
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint.Around(10f), 20f);
            CalloutMessage = "Possible Alien Attack";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS WE_HAVE CRIME_SUSPECT_ON_THE_RUN IN_OR_ON_POSITION", SpawnPoint);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            SpawnSequenceInitiated = false;
            
            Game.DisplayNotification("Citizens say they saw an ~r~alien~s~ at this ~y~position~w~.~n~This may be the first human-alien contact. Proceed with caution.");
            ZoneBlip = new Blip(SpawnPoint, 50f)
            {
                IsFriendly = false,
                IsRouteEnabled = true,
                Alpha = 0.5f,
                Color = Color.Gold
            };

            if(IsFake)
            {
                Alien = new Ped("s_m_m_movalien_01", SpawnPoint, 0f);
                Alien.SetVariation(0, 0, 0);
                Alien.SetVariation(3, 0, 0);
                Alien.SetVariation(4, 0, 0);
                Alien.SetVariation(5, 0, 0);
                Alien.SetVariation(6, 0, 0);
                Alien.DropsCurrentWeaponOnDeath = false;
                Alien.IsPersistent = true;
                Alien.BlockPermanentEvents = true;
                double wep = Rdm.NextDouble();
                Alien.Inventory.GiveNewWeapon(wep < 0.40 ? WeaponHash.Bat : (wep < 0.90 ? WeaponHash.CombatPistol : WeaponHash.AssaultRifle) , -1, true);
                Alien.RelationshipGroup = RelationshipGroup.Gang1;

                AlienSpawned = true;
            }
            else
            {
                AlienSpawned = false;
            }

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            if (AlienSpawned && (!Alien.Exists() || Alien.IsDead || Functions.IsPedArrested(Alien) || (TargetFlees && !Functions.IsPursuitStillRunning(Pursuit))))
            {
                End();
            }

            if (!IsFake && !SpawnSequenceInitiated && Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) < 25f)
            {
                AppearanceSequence();
            }

            if (IsFake && !SpawnSequenceInitiated && Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) < 50f)
            {
                PreparationSequence();
            }

            base.Process();
        }

        public override void End()
        {
            base.End();

            if (ZoneBlip.Exists()) { ZoneBlip.Delete(); }
            if (AlienBlip.Exists()) { AlienBlip.Delete(); }

            if (Alien.Exists()) { Alien.Dismiss(); }
        }

        private void PreparationSequence()
        {
            if (ZoneBlip.Exists()) { ZoneBlip.Delete(); }
            SpawnSequenceInitiated = true;
            Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Cop, RelationshipGroup.Gang1, Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Gang1, RelationshipGroup.Cop, Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Gang1, RelationshipGroup.Player, Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Player, RelationshipGroup.Gang1, Relationship.Hate);

            TargetFlees = Rdm.NextDouble() < 0.5f;
            if(!TargetFlees)
            {
                AlienBlip = Alien.AttachBlip();
                AlienBlip.IsFriendly = false;
                AlienBlip.Name = "Fake Alien";
                AlienBlip.Scale = 0.75f;
                Alien.Tasks.FightAgainst(Game.LocalPlayer.Character);
            }
            else
            {
                if (Functions.GetActivePursuit() != null) { Functions.ForceEndPursuit(Functions.GetActivePursuit()); }
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Alien);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
            }
        }

        private void AppearanceSequence()
        {
            SpawnSequenceInitiated = true;
            if (ZoneBlip.Exists()) { ZoneBlip.Delete(); }
            GameFiber.StartNew(delegate
            {
                for (double i = 0d; i < 8; i++)
                {
                    World.SpawnExplosion(SpawnPoint + new Vector3(Convert.ToSingle(5d * Math.Cos(Math.PI * i / 4d)), Convert.ToSingle(5d * Math.Sin(Math.PI * i / 4d)), 0f), 12, 1f, true, false, 0f);
                }
                GameFiber.Sleep(6000);

                for (double i = 0d; i < 8; i++)
                {
                    World.SpawnExplosion(SpawnPoint + new Vector3(Convert.ToSingle(5d * Math.Cos(Math.PI*i/4d)), Convert.ToSingle(5d * Math.Sin(Math.PI * i / 4d)), 0f), 8, 1f, true, false, 0f);
                }
                GameFiber.Sleep(500);
                Alien = new Ped("s_m_m_movalien_01", SpawnPoint, 0f);
                Alien.SetVariation(0, 0, 0);
                Alien.SetVariation(3, 0, 0);
                Alien.SetVariation(4, 0, 0);
                Alien.SetVariation(5, 0, 0);
                Alien.SetVariation(6, 0, 0);
                Alien.DropsCurrentWeaponOnDeath = false;
                Alien.IsPersistent = true;
                Alien.BlockPermanentEvents = true;
                Alien.Inventory.GiveNewWeapon(0x6D544C99, -1, true);
                Alien.RelationshipGroup = RelationshipGroup.Gang1;

                Persona AlienPersona = Functions.GetPersonaForPed(Alien);
                AlienPersona.ELicenseState = ELicenseState.None;
                AlienPersona.Surname = "Unknown";
                AlienPersona.Forename = "";
                AlienPersona.Wanted = true;
                Functions.SetPersonaForPed(Alien, AlienPersona);
                
                Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Cop, RelationshipGroup.Gang1, Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Gang1, RelationshipGroup.Cop, Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Gang1, RelationshipGroup.Player, Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Player, RelationshipGroup.Gang1, Relationship.Hate);

                
                AlienBlip = Alien.AttachBlip();
                AlienBlip.IsFriendly = false;
                AlienBlip.Name = "Alien";
                AlienBlip.Scale = 0.75f;
                Alien.Tasks.FightAgainst(Game.LocalPlayer.Character);
                AlienSpawned = true;
            });
        }
    }
}
