using System.Diagnostics;
using System.Text.Json;

namespace DefaultAudioDeviceSwitcher.Linux;

internal class ProfileInfo
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    public override string ToString() => string.IsNullOrEmpty(Description) ? Name : Description;
}

internal class CardInfo
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public List<ProfileInfo> Profiles = [];
    
    public override string ToString() => string.IsNullOrEmpty(Description) ? Name : Description;
}

internal record DefaultDeviceInfo(string CardName, string ProfileName);

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

    public static List<CardInfo>? GetCards()
    {
        var cards = new List<CardInfo>();

        var soundCards = PactlListCards();
        
        if (soundCards is null)
            return null;

        foreach (var soundCard in soundCards)
        {
            var card = new CardInfo
            {
                Name = soundCard.Name,
                Description = soundCard.Properties.GetValueOrDefault("device.nick")
                              ?? soundCard.Properties.GetValueOrDefault("api.alsa.card.name")
            };

            foreach (var (profileName, profile) in soundCard.Profiles)
            {
                if (profile is { Available: true })
                {
                    card.Profiles.Add(new ()
                    {
                        Name = profileName,
                        Description = profile.Description
                    });
                }
            }

            cards.Add(card);
        }

        return cards;
    }
    
    public static List<(string Sink, string Card, string Profile)>? GetSelectedProfiles()
    {
        var audioSinks = PactlListSinks();
        var cards = PactlListCards();
        if (cards == null)
            return null;
        
        var cardProfiles = cards.ToDictionary(c => c.Name, c => c.ActiveProfile);
        
        return audioSinks?.Where(s => cardProfiles
            .ContainsKey(s.Properties["device.name"]))
            .Select(s => (s.Name, s.Properties["device.name"], cardProfiles[s.Properties["device.name"]]))
            .ToList();
    }

    public static DefaultDeviceInfo? GetDefaultCardProfile()
    {
        var info = PactlInfo();
        var sinkName = info?.GetValueOrDefault("default_sink_name")?.ToString();
        
        if (sinkName == null) return null;

        var profiles = GetSelectedProfiles();

        return profiles?
            .Where(x => x.Sink == sinkName)
            .Select(x => new DefaultDeviceInfo(x.Card, x.Profile))
            .FirstOrDefault();
    }

    public static bool SetDefaultCardProfile(string cardName, string? profileName)
    {
        if (profileName != null)
        {
            PactlDoAction("set-card-profile", cardName, profileName);
        }
        
        var profiles = GetSelectedProfiles();
        var sinkName = profiles?.Where(x => x.Card == cardName).FirstOrDefault().Sink;

        if (sinkName == null)
        {
            Console.Error.WriteLine($"Could not find sink for {cardName} - {profileName}");
            return false;
        }

        return SetDefaultSink(sinkName);
    }

    public static bool SetDefaultSink(string sink)
    {
        if (!PactlDoAction("set-default-sink", sink)) return false;

        var sinkInputs  = PactlShortSinkInputs();
        if (sinkInputs == null) return false;

        foreach (var sinkInput in sinkInputs)
        {
            PactlDoAction("move-sink-input", sinkInput.Index.ToString(), sink);
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

    private static Dictionary<string, object>? PactlInfo() => CallPactlAndParse<Dictionary<string, object>>("info");

    private static List<AudioSink>? PactlListSinks() => CallPactlAndParse<List<AudioSink>>("list sinks");

    private static List<ShortSinkInput>? PactlShortSinkInputs() => CallPactlAndParse<List<ShortSinkInput>>("list short sink-inputs");
    
    private static List<SoundCard>? PactlListCards() => CallPactlAndParse<List<SoundCard>>("list cards");

    private static T? CallPactlAndParse<T>(string command)
        where T : class
    {
        var cmdOutput = Run("pactl", $"--format=json {command}");
        if (cmdOutput == null)
        {
            return null;
        }

        var parsedOutput = JsonSerializer.Deserialize<T>(cmdOutput);
        if (parsedOutput == null)
        {
            Console.WriteLine($"Could not parse pactl output for params '{command}'");
        }

        return parsedOutput;
    }

    private static bool PactlDoAction(string action, params string[] args)
    {
        return Run("pactl", $"{action} {string.Join(" ", args)}", useSpawn: true) != null;
    }
}