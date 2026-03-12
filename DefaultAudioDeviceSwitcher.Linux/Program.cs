namespace DefaultAudioDeviceSwitcher.Linux;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            RunTrayApp();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error starting tray app: " + ex);
            return 1;
        }
    }

    private static void RunTrayApp()
    {
        try
        {
            RunGui().Wait();
        }
        catch (AggregateException ae)
        {
            throw ae.Flatten();
        }
    }

    private static async Task RunGui()
    {
        Gtk.Application.Init();

        var configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DefaultAudioDeviceSwitcher"
        );

        Directory.CreateDirectory(configPath);

        var configFile = Path.Combine(configPath, "config.json");
        var settings = Settings.Load(configFile);

        var _ = new TrayAppContext(settings);

        Gtk.Application.Run();

        await Task.CompletedTask;
    }
}