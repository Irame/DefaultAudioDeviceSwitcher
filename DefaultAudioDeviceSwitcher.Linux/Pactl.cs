using System.Diagnostics;
using System.Text.Json;

namespace DefaultAudioDeviceSwitcher.Linux;

internal class SinkInfo
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public override string ToString() => string.IsNullOrEmpty(Description) ? Name : Description;
}

internal static class Pactl
{
    private static bool IsInsideFlatpak => File.Exists("/.flatpak-info");
    
    public static Task WatchForChanges(Action action, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var psi = new ProcessStartInfo("pactl", "subscribe")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            if (process == null) return;

            while (!cancellationToken.IsCancellationRequested)
            {
                string? line = process.StandardOutput.ReadLine();
                if (line == null) continue;

                // Anything relating to sink or server might matter
                if (line.Contains("on sink") || line.Contains("on server"))
                {
                    Gtk.Application.Invoke((_, _) => { action(); });
                }
            }

            process.Kill();
        }, cancellationToken);
    }
    
    public static List<SinkInfo>? GetSinks()
    {
        var sinks = new List<SinkInfo>();

        var cmdOutput = Run("pactl", "--format=json list cards");
        if (cmdOutput == null)
            return null;
            
        var soundCards = JsonSerializer.Deserialize<List<SoundCard>>(cmdOutput);

        if (soundCards == null)
        {
            Console.WriteLine("Could not parse pactl output");
            return null;
        }

        foreach (var soundCard in soundCards)
        {
            if (!soundCard.Name.StartsWith("alsa_card."))
                continue;

            var deviceSlug = soundCard.Name["alsa_card.".Length..];
            var deviceName = soundCard.Properties.GetValueOrDefault("device.nick")
                             ?? soundCard.Properties.GetValueOrDefault("api.alsa.card.name");

            HashSet<string> processedSinks = [];

            foreach (var (profileName, profile) in soundCard.Profiles.OrderBy(x => x.Key.Contains("input:")))
            {
                if (profile is { Available: true, Sinks: > 0 })
                {
                    sinks.AddRange(profileName.Split('+')
                        .Where(x => x.StartsWith("output:"))
                        .Select(x => x["output:".Length..])
                        .Select(output => $"alsa_output.{deviceSlug}.{output}")
                        .Where(x => processedSinks.Add(x))
                        .Select(sinkName => new SinkInfo
                        {
                            Name = sinkName,
                            Description = $"{deviceName} - {profile.Description}"
                        })) ;
                }
            }
        }

        return sinks;
    }

    public static string? GetDefaultSink()
    {
        var infoStr = Run("pactl", "--format=json info");
        if (infoStr == null) return null;

        var info  = JsonSerializer.Deserialize<Dictionary<string, object>>(infoStr);
        return info?.GetValueOrDefault("default_sink_name")?.ToString();
    }

    public static bool SetDefaultSink(string sink)
    {
        var result = Run("pactl", $"set-default-sink {sink}", useSpawn: true);
        if (result == null) return false;

        var inputs = Run("pactl", "list short sink-inputs");
        if (inputs == null) return true;

        foreach (var l in inputs.Split('\n'))
        {
            var parts = l.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            if (int.TryParse(parts[0], out var id))
                Run("pactl", $"move-sink-input {id} {sink}", useSpawn: true);
        }

        return true;
    }

    private static string? Run(string cmd, string args, bool useSpawn = false)
    {
        string actualCmd, actualArgs;

        if (IsInsideFlatpak && useSpawn)
        {
            actualCmd = "flatpak-spawn";
            actualArgs = $"--host {cmd} {args}";
        }
        else
        {
            actualCmd = cmd;
            actualArgs = args;
        }

        try
        {
            var psi = new ProcessStartInfo(actualCmd, actualArgs)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var p = Process.Start(psi);
            if (p == null) return "";

            string result = p.StandardOutput.ReadToEnd();
            p.WaitForExit(1000);

            if (p.ExitCode != 0)
            {
                string errorResult = p.StandardError.ReadToEnd();
                throw new Exception(errorResult);
            }
                
            return result;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error running {cmd} {args}: {ex.Message}");
            return null;
        }
    }
}