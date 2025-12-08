#!/usr/bin/env bash
cd "$(dirname "$0")"

python3 flatpak-dotnet-generator.py --dotnet 10 --freedesktop 24.08 \
    nuget-sources.json \
    ../DefaultAudioDeviceSwitcher.Linux/DefaultAudioDeviceSwitcher.Linux.csproj

flatpak-builder build-dir --user --force-clean --install --repo=repo --default-branch=dev com.github.irame.DefaultAudioDeviceSwitcher.yaml

