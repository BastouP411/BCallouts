using Rage;
using Rage.Native;

namespace BCallouts.Common
{
    public static class Natives
    {
        public static Task VehicleEscort(this TaskInvoker taskInvoker, Vehicle vehicle, Vehicle targetVehicle, EscortMode mode, float speed, VehicleDrivingFlags style, float minimumDistance, float noRoadsDistance)
        {
            Ped ped = taskInvoker.GetInstancePed();
            if (ped != null)
            {
                NativeFunction.CallByName<uint>("TASK_VEHICLE_ESCORT", ped, vehicle, targetVehicle, (int)mode, speed, (int)style, minimumDistance, 0, noRoadsDistance);
                return Task.GetTask(ped, "TASK_VEHICLE_ESCORT");
            }
            return null;
        }

        public static bool GetClosestVehicleSpawnPoint(this Vector3 SearchPoint, out Vector3 Point, out float Heading)
        {
            return NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING<bool>(SearchPoint.X, SearchPoint.Y, SearchPoint.Z, out Point, out Heading, 1, 0x40400000, 0);
        }

        public static bool GetSafeCoordForPed(this Vector3 SearchPoint, bool OnGround, out Vector3 Position)
        {
            return NativeFunction.Natives.GET_SAFE_COORD_FOR_PED<bool>(SearchPoint.X, SearchPoint.Y, SearchPoint.Z, OnGround, out Position, 16);
        }

        public static void RequestIPL(string IPL)
        {
            NativeFunction.Natives.REQUEST_IPL(IPL);
        }

        public static void RemoveIPL(string IPL)
        {
            NativeFunction.Natives.REMOVE_IPL(IPL);
        }

        public static bool IsIPLActive(string IPL)
        {
            return NativeFunction.Natives.IS_IPL_ACTIVE<bool>(IPL);
        }

        public static int GetSoundId()
        {
            return NativeFunction.Natives.GET_SOUND_ID<int>();
        }

        public static void StopSound(int id)
        {
            NativeFunction.Natives.STOP_SOUND(id);
        }

        public static void PlaySoundFrontend(int soundId, string audioName, string audioRef)
        {
            NativeFunction.Natives.PLAY_SOUND_FRONTEND(soundId, audioName, audioRef, true);
        }
    }
}
