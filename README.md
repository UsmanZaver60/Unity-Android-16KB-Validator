# Unity Android 16KB Page Size Validator

Google Play requires apps targeting Android 15 (API 35)+ to support 16KB memory page sizes.

This repository provides a Unity Post-Build Processor that automatically validates whether all native .so libraries inside a generated Android App Bundle (AAB) are aligned for 16KB page support.

## What It Does

After building an Android App Bundle, the script:

Extracts the generated .aab

Locates all native .so libraries under base/lib

Uses readelf -l from Unity’s bundled NDK

Scans ELF LOAD segments

Detects 4KB (0x1000) alignment

Logs errors if non-compliant libraries are found

This helps prevent Google Play submission failures caused by legacy or third-party native plugins.

## 🛠 How To Use

Place Android16KBPostProcessor.cs inside an Editor folder in your Unity project.

Enable Build App Bundle (AAB) in Android Build Settings.

Build your project.

The script automatically runs post-build validation.

If any native library is 4KB-aligned, the console will log an error.

## 🖥 Platform Support

⚠ Windows Only

This implementation currently:

Uses PowerShell Expand-Archive for AAB extraction

Searches for readelf.exe inside Unity’s bundled NDK

It has not been adapted for macOS or Linux environments.

## 📌 Requirements

Unity with Android module installed

Unity bundled Android NDK

Windows environment

Android App Bundle build enabled

## ⚠ Known Limitations

Windows-only (due to PowerShell extraction)

Does not currently fail the Unity build (logs error instead)

Assumes readelf exists inside Unity’s bundled NDK

Validates final AAB only (not intermediate Gradle outputs)
