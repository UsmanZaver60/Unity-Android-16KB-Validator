namespace UZaver
{
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using System.Diagnostics;
    using System.IO;
    using Debug = UnityEngine.Debug;

    public class Android16KBPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1000;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            if (!EditorUserBuildSettings.buildAppBundle)
                return;
            
            string outputPath = report.summary.outputPath;
            
            Debug.Log("🔍 Running 16KB page-size validation on AAB...");

            string aabDir = Path.GetDirectoryName(outputPath);
            string aabName = Path.GetFileNameWithoutExtension(outputPath);
            string tempDir = Path.Combine(aabDir, aabName + "_Extracted");

            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            Directory.CreateDirectory(tempDir);

            // Copy AAB → ZIP
            string zipPath = Path.Combine(tempDir, aabName + ".zip");
            File.Copy(outputPath, zipPath, true);

            // Extract AAB
            ExtractZip(zipPath, tempDir);

            string libRoot = Path.Combine(tempDir, "base", "lib");
            if (!Directory.Exists(libRoot))
            {
                Debug.LogError("No native libraries found in AAB.");
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
                return;
            }

            string readlELFPath = GetReadELFPath();
            foreach (var so in Directory.GetFiles(libRoot, "*.so", SearchOption.AllDirectories))
            {
                if (!Is16KbCompatible(so, readlELFPath))
                {
                    Debug.LogError($"16KB PAGE CHECK FAILED {so}");
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                    return;
                }
            }

            Debug.Log("✅ 16KB validation PASSED: All native libraries are compatible.");
            
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        // ---------- Helpers ----------
        static void ExtractZip(string zipPath, string destination)
        {
            string command =
                $"Expand-Archive -LiteralPath '{zipPath}' -DestinationPath '{destination}' -Force";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = destination
                }
            };

            process.Start();

            string stderr = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Debug.LogError(
                    $"PowerShell Expand-Archive failed.\n{stderr}"
                );
                return;
            }
        }

        private string GetReadELFPath()
        {
            string ndkRoot = GetUnityBundledNdkPath();

            string readelf = Directory.GetFiles(
                ndkRoot,
                "*readelf*.exe",
                SearchOption.AllDirectories
            )[0];
            return readelf;
        }
        static bool Is16KbCompatible(string soPath, string readELFPath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = readELFPath,
                    Arguments = $"-l \"{soPath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return IsReadElfOutput16KBCompatible(output);
        }
        static string GetUnityBundledNdkPath()
        {
            // Example:
            // C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data
            string editorData = EditorApplication.applicationContentsPath;

            string ndkPath = Path.Combine(
                editorData,
                "PlaybackEngines",
                "AndroidPlayer",
                "NDK"
            );

            if (!Directory.Exists(ndkPath))
                Debug.LogError($"Android NDK not found at:\n{ndkPath}");

            return ndkPath;
        }
        static bool IsReadElfOutput16KBCompatible(string output)
        {
            foreach (var line in output.Split('\n'))
            {
                if (!line.Contains("LOAD"))
                    continue;

                // Last column is p_align
                string[] tokens = line.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                string align = tokens[^1];

                if (align == "0x1000") // 4KB → FAIL
                    return false;
            }
            return true;
        }
    }
}
