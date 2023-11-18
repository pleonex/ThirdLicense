// Copyright (c) 2020 Benito Palacios Sánchez

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace ThirdLicense.ProjectAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using NuGet.Packaging.Core;
    using NuGet.Versioning;

    /// <summary>
    /// Represents a .NET project analyzer that provides with the transitive dependencies.
    /// </summary>
    public static class DotnetListStdoutAnalyzer
    {
        /// <summary>
        /// Analyzes a .NET projects and retrieves its transitive dependencies.
        /// </summary>
        /// <param name="projectPath">The path to the project file.</param>
        /// <returns>An enumeration with the transitive dependencies.</returns>
        public static IAsyncEnumerable<PackageIdentity> Analyze(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath)) {
                throw new ArgumentNullException(nameof(projectPath));
            }

            // .NET 8 may throw errors if it's a relative path
            // https://github.com/NuGet/Home/issues/12954
            projectPath = Path.GetFullPath(projectPath);

            var stdout = GetDotnetListOutput(projectPath);
            return AnalyzeOutput(stdout);
        }

        private static async IAsyncEnumerable<PackageIdentity> AnalyzeOutput(IAsyncEnumerable<string> lines)
        {
            var uniquePackages = new List<PackageIdentity>();

            await foreach (var line in lines) {
                if (line.Length < 6 || line[3] != '>') {
                    continue;
                }

                var content = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (content.Length < 3) {
                    continue;
                }

                bool success = NuGetVersion.TryParse(content[^1], out NuGetVersion version);
                if (!success) {
                    throw new FormatException("Cannot parse: " + content[^1]);
                }

                var packageId = new PackageIdentity(content[1], version);
                if (uniquePackages.Contains(packageId)) {
                    continue;
                }

                uniquePackages.Add(packageId);
                yield return packageId;
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
                string line = await process.StandardOutput.ReadLineAsync().ConfigureAwait(false);
                yield return line;
            }

            bool success = process.WaitForExit(30_000);

            if (!success || process.ExitCode != 0) {
                throw new InvalidOperationException("Cannot get dependencies info");
            }
        }
    }
}
