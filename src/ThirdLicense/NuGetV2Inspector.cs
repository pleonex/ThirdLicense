namespace ThirdLicense
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// Analyzer of NuGet package metadata via the HTTP REST API v2.
    /// </summary>
    /// <remarks>
    /// The version 2 does not support the new license expressions or file.
    /// It did only support the now deprecated license URL. For this reason
    /// this analyzer needs to download the nupkg file to read the nuspec.
    /// </remarks>
    public class NuGetV2Inspector
    {
        const string MetadataEndpoint = "Packages(Id='{0}',Version='{1}')";

        static readonly HttpClient client;
        readonly Uri endpoint;

        static NuGetV2Inspector()
        {
            client = new HttpClient();
        }

        public NuGetV2Inspector(string v2Endpoint)
        {
            endpoint = new Uri(v2Endpoint);
        }

        public async ValueTask<PackageInfo> InspectAsync(PackageId packageId)
        {
            using HttpResponseMessage response = await GetPackageAsync(packageId);
            if (response == null) {
                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var nupkg = new ZipArchive(stream);

            XDocument nuspec = GetNuSpecFromPackage(nupkg, packageId);
            string nuspecNamespace = nuspec.Root.Attribute("xmlns").Value;
            XElement metadata = nuspec.Root.Element(XName.Get("metadata", nuspecNamespace));
            if (metadata == null) {
                throw new FormatException($"Missing metadata element for {packageId}");
            }

            if (metadata.Element(XName.Get("developmentDependency", nuspecNamespace))?.Value == "true") {
                Console.WriteLine($"Skipping dev dependency: {packageId}");
                return null;
            }

            var info = new PackageInfo {
                Id = packageId,
                Copyright = metadata.Element(XName.Get("copyright", nuspecNamespace))?.Value,
                ProjectUrl = metadata.Element(XName.Get("projectUrl", nuspecNamespace))?.Value,
                LicenseUrl = metadata.Element(XName.Get("licenseUrl", nuspecNamespace))?.Value,
            };

            XElement repository = metadata.Element(XName.Get("repository", nuspecNamespace));
            if (repository != null) {
                info.RepositoryUrl = repository.Attribute("url")?.Value;
                info.RepositoryCommit = repository.Attribute("commit")?.Value;
            }

            string licenseFile = nupkg.Entries
                .FirstOrDefault(e => e.Name.Equals("LICENSE.TXT", StringComparison.OrdinalIgnoreCase))
                ?.FullName;

            XElement license = metadata.Element(XName.Get("license", nuspecNamespace));
            if (license != null) {
                string type = license.Attribute("type")?.Value;
                if (type == "expression") {
                    info.LicenseExpression = license.Value;
                } else if (type == "file") {
                    licenseFile = license.Value;
                }
            }

            if (licenseFile != null) {
                using var licenseStream = nupkg.GetEntry(licenseFile)?.Open();
                if (licenseStream == null) {
                    throw new FormatException("Missing license file");
                }

                info.LicenseContent = await new StreamReader(licenseStream).ReadToEndAsync();
            }

            return info;
        }

        private XDocument GetNuSpecFromPackage(ZipArchive nupkg, PackageId packageId)
        {
            using var nuspecStream = nupkg.GetEntry($"{packageId.Name}.nuspec")?.Open();
            if (nuspecStream == null) {
                throw new FormatException("Missing NuSpec file");
            }

            return XDocument.Load(nuspecStream);
        }

        private async Task<HttpResponseMessage> GetPackageAsync(PackageId packageId)
        {
            // First get the metadata of the package.
            // The v2 doesn't include license information but it has the
            // download URL.
            string downloadUri = await GetPackageDownloadUriAsync(packageId);
            if (downloadUri == null) {
                return null;
            }

            var response = await client.GetAsync(downloadUri);
            response.EnsureSuccessStatusCode();

            return response;
        }

        private async Task<String> GetPackageDownloadUriAsync(PackageId packageId)
        {
            var uri = new Uri(endpoint, string.Format(MetadataEndpoint, packageId.Name, packageId.Version));
            using var response = await client.GetAsync(uri);
            if (!response.IsSuccessStatusCode) {
                Console.WriteLine($"Cannot get info for: {packageId}");
                return null;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            XDocument metadata = XDocument.Load(stream);
            XElement content = metadata.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "content");
            if (content == null) {
                throw new FormatException("Missing content metadata info");
            }

            if (content.Attribute("type")?.Value != "application/zip") {
                throw new FormatException("Only nupkg in zip format supported");
            }

            var srcAttribute = content.Attribute("src");
            if (srcAttribute == null) {
                throw new FormatException("Missing download URL");
            }

            return srcAttribute.Value;
        }
    }
}
