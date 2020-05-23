namespace ThirdLicense
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            string project = "";
            string endpoint = "https://www.nuget.org/api/v2/";
            string output = "THIRD-PARTY-NOTICES.TXT";

            var analyzer = new DotnetListStdoutAnalyzer();
            var dependencies = analyzer.Analyze(project);

            var nugetInspector = new NuGetV2Inspector(endpoint);
            var packages = dependencies
                .SelectAwait(d => nugetInspector.InspectAsync(d))
                .Where(x => x != null);

            using var outputStream = new FileStream(output, FileMode.Create);
            var licenseGenerator = new LicenseTextGenerator();
            await licenseGenerator.Generate(outputStream, packages);
        }
    }
}
