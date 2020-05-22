namespace ThirdLicense
{
    using System.IO;
    using System.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            var analyzer = new DotnetInspectStdoutAnalyzer();
            var dependencies = analyzer.Analyze("");

            var nugetInspector = new NuGetInspector();
            var packages = dependencies.Select(d => nugetInspector.Inspect(d));

            using var outputStream = new FileStream("", FileMode.Open);
            var licenseGenerator = new LicenseTextGenerator();
            licenseGenerator.Generate(packages);
        }
    }
}
