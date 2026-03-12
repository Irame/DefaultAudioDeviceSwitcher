using System.Runtime.InteropServices;

namespace DefaultAudioDeviceSwitcher.Linux;

// Needs GLib main loop to fire
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
delegate void NotifyActionCallback(IntPtr notification, string action, IntPtr userData);

public static class Notify
{
    const string Lib = "libnotify.so.4";

    private const string AppName = "Default Audio Device Switcher";

    [DllImport(Lib)] static extern bool notify_init(string appName);
    [DllImport(Lib)] static extern IntPtr notify_notification_new(string summary, string body, string? icon);
    [DllImport(Lib)] static extern bool notify_notification_show(IntPtr notification, IntPtr error);
    [DllImport(Lib)] static extern void g_object_unref(IntPtr obj);
    [DllImport(Lib)] static extern void notify_notification_add_action(
        IntPtr notification,
        string action,       // action ID (e.g. "clicked")
        string label,        // button label shown to user
        NotifyActionCallback callback,
        IntPtr userData,
        IntPtr freeFunc);

    public static void Send(string summary, string body, string? icon = "dialog-information")
    {
        notify_init(AppName);
        var n = notify_notification_new(summary, body, icon);
        notify_notification_show(n, IntPtr.Zero);
        g_object_unref(n);
    }
    
    public static void SendWithAction(
        string summary,
        string body,
        string actionLabel,
        Action onAction,
        string? icon = "dialog-information")
    {
        notify_init(AppName);
        var n = notify_notification_new(summary, body, icon);

        // Hold a strong reference so the GC doesn't collect it before the callback fires
        NotifyActionCallback callback = null!;
        callback = (notif, action, data) =>
        {
            onAction();
            g_object_unref(notif);
        };
        GCHandle.Alloc(callback);

        notify_notification_add_action(n, "default", actionLabel, callback, IntPtr.Zero, IntPtr.Zero);
        notify_notification_show(n, IntPtr.Zero);
    }
}