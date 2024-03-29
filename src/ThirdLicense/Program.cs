﻿// Copyright (c) 2020 Benito Palacios Sánchez

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
namespace ThirdLicense
{
    using System;
    using System.CommandLine;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using NuGet.Packaging;
    using ThirdLicense.LicenseGenerator;
    using ThirdLicense.ProjectAnalyzer;

    /// <summary>
    /// Application entry-point.
    /// </summary>
    internal static class Program
    {
        const string DefaultOutputName = "THIRD-PARTY-NOTICES.TXT";

        /// <summary>
        /// Runs the application.
        /// </summary>
        /// <param name="args">The application arguments.</param>
        /// <returns>The return code of the application.</returns>
        internal static Task<int> Main(string[] args)
        {
            var projectArg = new Option<string>("--project") {
                Description = "Project file to analyze third-parties",
                IsRequired = true,
            };
            var endpointArg = new Option<string>("--endpoint") {
                Description = "Additional NuGet repository endpoint",
                IsRequired = false,
            };
            var outputArg = new Option<FileInfo>("--output", () => new FileInfo(DefaultOutputName)) {
                Description = "Path to the output file",
                IsRequired = true,
            };

            var rootCommand = new RootCommand("Generates transitive third-party license notice") {
                projectArg,
                endpointArg,
                outputArg,
            };
            rootCommand.SetHandler(Generate, projectArg, endpointArg, outputArg);

            return rootCommand.InvokeAsync(args);
        }

        static async Task<int> Generate(string project, string endpoint, FileInfo output)
        {
            Console.WriteLine("Project: {0}", project);
            Console.WriteLine("Output file: {0}", output.FullName);
            if (!string.IsNullOrEmpty(endpoint)) {
                Console.WriteLine("Endpoint: {0}", endpoint);
            } else {
                Console.WriteLine("Default endpoints");
            }

            if (!File.Exists(project) && !Directory.Exists(project)) {
                Console.WriteLine("Cannot find project file or folder: {0}", project);
                return -1;
            }

            if (!output.Directory.Exists) {
                output.Directory.Create();
            }

            Stopwatch watch = Stopwatch.StartNew();
            var dependencies = DotnetListStdoutAnalyzer.Analyze(project);

            using var nugetInspector = new NuGetProtocolInspector();
            if (!string.IsNullOrEmpty(endpoint)) {
                await nugetInspector.AddEndpointAsync(endpoint).ConfigureAwait(false);
            }

            await nugetInspector.AddDefaultEndpointsAsync().ConfigureAwait(false);

            var packages = dependencies
                .SelectAwait(d => new ValueTask<NuspecReader>(nugetInspector.InspectAsync(d)))
                .Where(x => x != null);

            using var outputStream = new FileStream(output.FullName, FileMode.Create);
            await LicenseTextGenerator.Generate(outputStream, packages).ConfigureAwait(false);

            watch.Stop();
            Console.WriteLine($"Done in {watch.Elapsed}!");
            return 0;
        }
    }
}
