using System.Runtime.InteropServices;
using System.Text;

namespace Leayal.PowerCfg_QUI.Classes
{
    static class PowerCfg
    {
        [DllImport("PowrProf.dll")]
        public static extern UInt32 PowerEnumerate(IntPtr RootPowerKey, IntPtr SchemeGuid, IntPtr SubGroupOfPowerSettingGuid, UInt32 AcessFlags, UInt32 Index, ref Guid Buffer, ref UInt32 BufferSize);

        [DllImport("PowrProf.dll")]
        public static extern UInt32 PowerReadFriendlyName(IntPtr RootPowerKey, ref Guid SchemeGuid, IntPtr SubGroupOfPowerSettingGuid, IntPtr PowerSettingGuid, IntPtr Buffer, ref UInt32 BufferSize);

        public enum AccessFlags : uint
        {
            ACCESS_SCHEME = 16,
            ACCESS_SUBGROUP = 17,
            ACCESS_INDIVIDUAL_SETTING = 18
        }

        public static string? GetPowerSchemeName(Guid schemeGuid)
        {
            uint sizeName = 1024;
            IntPtr pSizeName = Marshal.AllocHGlobal((int)sizeName);
            string? friendlyName;
            try
            {
                PowerReadFriendlyName(IntPtr.Zero, ref schemeGuid, IntPtr.Zero, IntPtr.Zero, pSizeName, ref sizeName);
                friendlyName = Marshal.PtrToStringUni(pSizeName);
            }
            finally
            {
                Marshal.FreeHGlobal(pSizeName);
            }

            return friendlyName;
        }

        public static IEnumerable<Guid> GetAllPowerSchemes()
        {
            var schemeGuid = Guid.Empty;

            uint sizeSchemeGuid = (uint)Marshal.SizeOf(typeof(Guid));
            uint schemeIndex = 0;

            while (PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, (uint)AccessFlags.ACCESS_SCHEME, schemeIndex, ref schemeGuid, ref sizeSchemeGuid) == 0)
            {
                yield return schemeGuid;
                schemeIndex++;
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("powrprof.dll")]
        private static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, ref IntPtr ActivePolicyGuid);

        public static bool GetCurrentPowerScheme(out Guid activeId, out string activeName)
        {
            // uint sizeSchemeGuid = (uint)Marshal.SizeOf(typeof(Guid));
            IntPtr ptr = IntPtr.Zero;
            uint res = PowerGetActiveScheme(IntPtr.Zero, ref ptr);
            if (res == 0)
            {
                activeId = Marshal.PtrToStructure<Guid>(ptr);
                LocalFree(ptr);
                activeName = GetPowerSchemeName(activeId) ?? String.Empty;
                return true;
            }
            activeId = Guid.Empty;
            activeName = string.Empty;
            return false;
        }

        [DllImport("powrprof.dll")]
        private static extern uint PowerSetActiveScheme(IntPtr UserRootPowerKey, Guid ActivePolicyGuid);

        public static bool SetCurrentPowerScheme(Guid activeId)
        {
            return (PowerSetActiveScheme(IntPtr.Zero, activeId) == 0);
        }
    }
}
