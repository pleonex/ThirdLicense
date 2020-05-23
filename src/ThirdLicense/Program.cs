namespace ThirdLicense
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    class Program
    {
        const string NugetEndpointV2 = "https://www.nuget.org/api/v2/";
        const string DefaultOutputName = "THIRD-PARTY-NOTICES.TXT";

        static Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Generate transitive third-party license notice") {
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
            var analyzer = new DotnetListStdoutAnalyzer();
            var dependencies = analyzer.Analyze(project);

            var nugetInspector = new NuGetV2Inspector(endpoint);
            var packages = dependencies
                .SelectAwait(d => nugetInspector.InspectAsync(d))
                .Where(x => x != null);

            using var outputStream = new FileStream(output, FileMode.Create);
            var licenseGenerator = new LicenseTextGenerator();
            await licenseGenerator.Generate(outputStream, packages);

            return 0;
        }
    }
}
