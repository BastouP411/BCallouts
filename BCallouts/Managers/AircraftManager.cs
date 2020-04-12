using BCallouts.Beans;
using BCallouts.Common;
using BCallouts.Managers;
using BCallouts.Menus;
using Rage;
using RAGENativeUI;
using System.Collections.Generic;

namespace BCallouts
{
    public static class AircraftManager
    {
        private static readonly Vector3 AIRPORT_BLIP_POS = new Vector3(-893.53f, -2401.76f, 15f);
        private static readonly float AIRPORT_BLIP_HEADING = 160f;
        private static readonly Vector3 CARRIER_BLIP_POS = new Vector3(3081.11f, -4705.11f, 16f);
        private static readonly float CARRIER_BLIP_HEADING = 109f;
        private static readonly Vector3 CARRIER_PLANE_SPAWN_POS = new Vector3(3098.74f, -4808.59f, 16f);
        private static readonly float CARRIER_PLANE_SPAWN_HEADING = 24.82f;

        public static Blip AirportBlip { get; private set; }
        public static Blip CarrierBlip { get; private set; }
        public static bool IsActive { get; private set; }
        public static List<IDisplayItem> AircraftModels { get; private set; }

        private static GameFiber ProcessFiber;
        private static bool IsPlayerInZone;
        private static AircraftSelectorMenu AircraftSelectorMenu;
        private static CarrierMenu CarrierMenu;

        public static void Initialize() {
            IsActive = true;

            AirportBlip = new Blip(AIRPORT_BLIP_POS) {

                Sprite = (BlipSprite)583,
                Name = "Aircraft Selection"
            };

            CarrierBlip = new Blip(CARRIER_BLIP_POS) {

                Sprite = (BlipSprite)455,
                Name = "Aircraft Carrier"

            };

            AircraftModels = new List<IDisplayItem>();
            foreach (AircraftModel am in ConfigManager.Config.AircraftModels)
            {
                AircraftModels.Add(new DisplayItem(new Model(am.Model), am.Name));
            }

            AircraftSelectorMenu = new AircraftSelectorMenu();
            CarrierMenu = new CarrierMenu();
            Process();
        }

        public static void Finally() {
            if (CarrierBlip.Exists()) { CarrierBlip.Delete(); }
            if (AirportBlip.Exists()) { AirportBlip.Delete(); }
            if (ProcessFiber.IsAlive) { ProcessFiber.Abort(); }
            AircraftSelectorMenu = null;
            CarrierMenu = null;
            IsActive = false;
        }

        private static void Process() {

            ProcessFiber = GameFiber.StartNew(delegate { 
                while(true) {

                    if (AirportBlip.Exists() && ZoneActivationCheck(AirportBlip, 5f, "Hit ~INPUT_CONTEXT~ to open the Plane Manager menu.")) {
                        AircraftSelectorMenu.OpenMenu();
                    }

                    if (CarrierBlip.Exists() && ZoneActivationCheck(CarrierBlip, 5f, "Hit ~INPUT_CONTEXT~ to open the Carrier menu.")) {
                        CarrierMenu.OpenMenu();
                    }

                    AircraftSelectorMenu.Process();
                    CarrierMenu.Process();
                    GameFiber.Yield();
                }
            });

        }

        private static bool ZoneActivationCheck(Blip Blip, float Distance, string Message) {
            if (Blip.Exists() && Blip.Position.DistanceTo(Game.LocalPlayer.Character.Position) < Distance)
            {
                if (!IsPlayerInZone)
                {
                    Game.DisplayHelp(Message, false);
                    IsPlayerInZone = true;
                }

                return Game.IsControlJustPressed(0, GameControl.Context);
            }
            else if (Blip.Exists() && Blip.Position.DistanceTo(Game.LocalPlayer.Character.Position) >= Distance)
            {
                if (IsPlayerInZone)
                {
                    IsPlayerInZone = false;
                }
            }
            return false;
        }

        public static void TravelToCarrier() {
            GameFiber.StartNew(delegate {
                Game.FadeScreenOut(3000);
                while (!Game.IsScreenFadedOut) { GameFiber.Yield(); }
                Game.LocalPlayer.Character.Position = CARRIER_BLIP_POS;
                Game.LocalPlayer.Character.Heading = CARRIER_BLIP_HEADING;
                GameFiber.Sleep(1000);
                Game.FadeScreenIn(3000);
            });
        }

        public static void TravelToGround() {
            GameFiber.StartNew(delegate {
                Game.FadeScreenOut(3000);
                while (!Game.IsScreenFadedOut) { GameFiber.Yield(); }
                Game.LocalPlayer.Character.Position = AIRPORT_BLIP_POS;
                Game.LocalPlayer.Character.Heading = AIRPORT_BLIP_HEADING;
                GameFiber.Sleep(1000);
                Game.FadeScreenIn(3000);
            });
        }

        public static void TakePlane(Model Model) {
            GameFiber.StartNew(delegate {
                Game.FadeScreenOut(3000);
                while(!Game.IsScreenFadedOut) { GameFiber.Yield(); }
                Vehicle Plane = new Vehicle(Model, CARRIER_PLANE_SPAWN_POS, CARRIER_PLANE_SPAWN_HEADING);
                Plane.SetLockedForPlayer(Game.LocalPlayer, false);
                Game.LocalPlayer.Character.WarpIntoVehicle(Plane, -1);
                GameFiber.Sleep(1000);
                Game.FadeScreenIn(3000);
            });
        }
    }
}
