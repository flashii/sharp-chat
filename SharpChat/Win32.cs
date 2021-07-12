using System.Runtime.InteropServices;

namespace SharpChat {
    public static class Win32 {
        public static bool RunningOnWindows
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static void IncreaseThreadPrecision() {
            if(RunningOnWindows)
                timeBeginPeriod(1);
        }

        public static void RestoreThreadPrecision() {
            if(RunningOnWindows)
                timeEndPeriod(1);
        }

        [DllImport(@"winmm")]
        public static extern uint timeBeginPeriod(uint period);

        [DllImport(@"winmm")]
        public static extern uint timeEndPeriod(uint period);
    }
}
