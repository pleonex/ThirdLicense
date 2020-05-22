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

            var analyzer = new DotnetListStdoutAnalyzer();
            var dependencies = analyzer.Analyze(project);

            var nugetInspector = new NuGetInspector();
            var packages = dependencies.Select(d => nugetInspector.Inspect(d));

            using var outputStream = new FileStream("", FileMode.Open);
            var licenseGenerator = new LicenseTextGenerator();
            await licenseGenerator.Generate(packages);
        }
    }
}
