# DefaultAudioDeviceSwitcher — Linux port

This project contains a minimal .NET 10 tray application that uses Ayatana AppIndicator3 to show a tray icon and `pactl` to switch the default PulseAudio/PipeWire sink (output).

Important notes
- This is a starting implementation. It expects native GTK and Ayatana AppIndicator3 libraries and C# bindings to be installed on the system.
- You may need to adjust the package names and versions in the `.csproj` to match the available NuGet packages.

Prerequisites (Ubuntu/Debian examples)

Install system deps:

```bash
sudo apt update
sudo apt install -y libayatana-appindicator3-1 gir1.2-ayatanaappindicator3-0.1 libgtk-3-0
# On systems using pipewire, ensure pipewire-pulse provides pactl compatibility
sudo apt install -y pulseaudio-utils
```

Add NuGet packages (from repo root):

```bash
cd DefaultAudioDeviceSwitcher.Linux
dotnet add package GtkSharp
dotnet add package Ayatana.AppIndicator3
dotnet restore
```

Run the app:

```bash
dotnet run --project DefaultAudioDeviceSwitcher.Linux
```

Behavior
- The app enumerates sinks using `pactl list sinks` and shows them in the tray menu.
- Selecting an item sets it as the default sink using `pactl set-default-sink` and attempts to move existing sink inputs.

If the Ayatana/Gtk bindings are missing at runtime the app prints guidance to the console.

Next steps / improvements
- Replace the dynamic/reflection approach with direct GTK/Ayatana bindings once the exact NuGet package names/versions are determined.
- Add icons, visual default-checkmark, and better error handling.
- Support sources (input) similarly if needed.
