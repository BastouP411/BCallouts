using Rage;
using System.Reflection;

namespace BCallouts.Common
{
    public static class Util
    {
        // Thanks LMS for this code snippet
        public static Ped GetInstancePed(this TaskInvoker taskInvoker)
        {
            PropertyInfo p = taskInvoker.GetType().GetProperty("Ped", BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null)
            {
                Ped instancePed = (Ped)p.GetMethod.Invoke(taskInvoker, null);
                return instancePed;
            }

            return null;
        }
    }
}
