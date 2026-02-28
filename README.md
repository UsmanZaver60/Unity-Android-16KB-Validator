# Unity Android 16KB Page Size Validator

Unity post-build validator that scans Android App Bundles (AAB) to ensure native libraries are 16KB page-size compliant for Google Play (Android 15+ requirement).

---

## 🧪 Tested Environment

- **Unity Version:** 2022.3.62f3 (LTS)
- **Platform:** Windows
- **Build Type:** Android App Bundle (AAB)
- **NDK:** Unity bundled NDK

---

## 🔍 What It Does

After building an Android App Bundle, the script:

- Extracts the generated `.aab`
- Locates all native `.so` libraries under `base/lib`
- Runs `readelf -l`
- Checks ELF `LOAD` segment alignment
- Logs an error if 4KB (`0x1000`) alignment is detected

---

## 🎯 Why This Matters

Google Play requires apps targeting Android 15+ to support 16KB page sizes.

Older SDKs, native plugins, or AAR dependencies may still use 4KB alignment, which can result in Play Console submission rejection.

This validator helps catch the issue immediately after build.

---

## 🛠 How To Use

1. Place `Android16KBPostProcessor.cs` inside an `Editor` folder.
2. Enable **Build App Bundle (AAB)** in Unity Android settings.
3. Build the project.
4. Validation runs automatically after build.

---

## 🖥 Platform Support

⚠ **Windows Only**

This implementation:

- Uses PowerShell `Expand-Archive` for AAB extraction
- Searches for `readelf.exe` inside Unity’s bundled NDK

macOS and Linux environments are not currently supported.

---

## 📌 Requirements

- Unity 2022.3.62f3 (tested)
- Android module installed
- Unity bundled NDK
- Windows environment
- AAB build enabled

---

## ⚠ Known Limitations

- Windows only
- Logs error instead of failing the Unity build
- Assumes `readelf` exists in Unity’s bundled NDK
- Validates final AAB only (not intermediate Gradle outputs)

---

## 📄 License

MIT License
