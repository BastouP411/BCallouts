using System;
using System.Collections.Generic;
using System.Text;
using Rage;
using Rage.Native;

namespace BCallouts
{
    public static class Natives
    {
        public static bool GetClosestVehicleSpawnPoint(this Vector3 SearchPoint, out Vector3 Point, out float Heading)
        {
            Vector3 TempPoint;
            float TempHeading;
            bool Tmp = NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING<bool>(SearchPoint.X, SearchPoint.Y, SearchPoint.Z, out TempPoint, out TempHeading, 1, 0x40400000, 0);
            Point = TempPoint;
            Heading = TempHeading;
            return Tmp;
        }

        public static bool GetSafeCoordForPed(this Vector3 searchPoint, bool onGround, out Vector3 position)
        {
            return NativeFunction.Natives.GET_SAFE_COORD_FOR_PED<bool>(searchPoint.X, searchPoint.Y, searchPoint.Z, onGround, out position, 16);
        }
    }
}
