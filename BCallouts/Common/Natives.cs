using Rage;
using Rage.Native;

namespace BCallouts.Common
{
    public static class Natives
    {
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

        public static bool IsScreenFadedOut() {
            return NativeFunction.Natives.IS_SCREEN_FADED_OUT<bool>();
        }

        public static void DoScreenFadeOut(int Duration) {
            NativeFunction.Natives.DO_SCREEN_FADE_OUT(Duration);
        }

        public static void DoScreenFadeIn(int Duration)
        {
            NativeFunction.Natives.DO_SCREEN_FADE_IN(Duration);
        }
    }
}
