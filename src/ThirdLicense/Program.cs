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
namespace ThirdLicense
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public static class Program
    {
        const string NugetEndpointV2 = "https://www.nuget.org/api/v2/";
        const string DefaultOutputName = "THIRD-PARTY-NOTICES.TXT";

        static Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Generates transitive third-party license notice") {
                new Option<string>("--project") {
                    Description = "Project file to analyze third-parties",
                    Required = true,
                },
                new Option<string>("--endpoint", () => NugetEndpointV2) {

                    Description = "NuGet repository endpoint (v2 only)",
                    Required = false,
                },
                new Option<string>("--output", () => DefaultOutputName) {
                    Description = "Path to the output file",
                    Required = true,
                },
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string>(Generate);

            return rootCommand.InvokeAsync(args);
        }

        static async Task<int> Generate(string project, string endpoint, string output)
        {
            Stopwatch watch = Stopwatch.StartNew();
            var analyzer = new DotnetListStdoutAnalyzer();
            var dependencies = analyzer.Analyze(project);

            var nugetInspector = new NuGetV2Inspector(endpoint);
            var packages = dependencies
                .SelectAwait(d => nugetInspector.InspectAsync(d))
                .Where(x => x != null);

            using var outputStream = new FileStream(output, FileMode.Create);
            var licenseGenerator = new LicenseTextGenerator();
            await licenseGenerator.Generate(outputStream, packages);

            watch.Stop();
            Console.WriteLine($"Done in {watch.Elapsed}!");
            return 0;
        }
    }
}
