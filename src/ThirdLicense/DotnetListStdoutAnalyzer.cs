namespace ThirdLicense
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;

    public class DotnetListStdoutAnalyzer
    {
        public IAsyncEnumerable<PackageId> Analyze(string projectPath)
        {
            var stdout = GetDotnetListOutput(projectPath);
            return AnalyzeOutput(stdout);
        }

        private static async IAsyncEnumerable<PackageId> AnalyzeOutput(IAsyncEnumerable<string> lines)
        {
            List<PackageId> uniquePackages = new List<PackageId>();

            await foreach (var line in lines) {
                if (line.Length < 6 || line[3] != '>') {
                    continue;
                }

                var content = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (content.Length < 3) {
                    continue;
                }

                string name = content[1];
                string version = content[^1];
                if (uniquePackages.Any(p => p.Name == name && p.Version == version)) {
                    continue;
                }

                var package = new PackageId();
                package.Name = name;
                package.Version = version;

                uniquePackages.Add(package);
                yield return package;
            }
        }

        private static async IAsyncEnumerable<string> GetDotnetListOutput(string projectPath)
        {
            var startInfo = new ProcessStartInfo {
                FileName = "dotnet",
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            startInfo.ArgumentList.Add("list");
            startInfo.ArgumentList.Add(projectPath);
            startInfo.ArgumentList.Add("package");
            startInfo.ArgumentList.Add("--include-transitive");

            using var process = Process.Start(startInfo);

            while (!process.StandardOutput.EndOfStream) {
                string line = await process.StandardOutput.ReadLineAsync();
                yield return line;
            }

            bool success = process.WaitForExit(30_000);

            if (!success || process.ExitCode != 0) {
                throw new Exception("Cannot get dependencies info");
            }
        }
    }
}
