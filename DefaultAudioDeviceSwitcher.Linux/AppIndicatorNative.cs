using System.Runtime.InteropServices;

namespace DefaultAudioDeviceSwitcher.Linux
{
    public enum AppIndicatorCategory { ApplicationStatus, Communications, SystemServices, Hardware, Other }
    public enum AppIndicatorStatus { Passive, Active, Attention }

    internal static class AppIndicatorNative
    {
        #if FLATPAK
            private const string Lib = "libappindicator3.so.1";
        #else
            private const string Lib = "libayatana-appindicator3.so.1";
        #endif

        [DllImport(Lib, CharSet = CharSet.Ansi)]
        public static extern IntPtr app_indicator_new(string id, string iconName, AppIndicatorCategory category);
        
        [DllImport(Lib, CharSet = CharSet.Ansi)]
        public static extern IntPtr app_indicator_new_with_path(string id, string iconName, AppIndicatorCategory category, string iconThemePath);

        [DllImport(Lib)]
        public static extern void app_indicator_set_status(IntPtr indicator, AppIndicatorStatus status);

        [DllImport(Lib, CharSet = CharSet.Ansi)]
        public static extern void app_indicator_set_icon(IntPtr indicator, string iconName);

        [DllImport(Lib, CharSet = CharSet.Ansi)]
        public static extern void app_indicator_set_icon_theme_path(IntPtr indicator, string iconThemePath);

        [DllImport(Lib, CharSet = CharSet.Ansi)]
        public static extern void app_indicator_set_title(IntPtr indicator, string title);

        [DllImport(Lib)]
        public static extern void app_indicator_set_menu(IntPtr indicator, IntPtr menu);

        [DllImport(Lib)]
        public static extern void app_indicator_set_secondary_activate_target(IntPtr indicator, IntPtr menuItem);
    }
}