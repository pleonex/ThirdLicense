namespace ThirdLicense
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public class LicenseTextGenerator
    {
        public async Task Generate(Stream output, IAsyncEnumerable<PackageInfo> packages)
        {
            var writer = new StreamWriter(output);
            await foreach (var package in packages) {
                await WritePackageNotice(writer, package);
            }
        }

        private async Task WritePackageNotice(StreamWriter writer, PackageInfo package)
        {
            await writer.WriteAsync("License notice for ");
            await writer.WriteAsync(package.Id.Name);
            await writer.WriteAsync(" (v");
            await writer.WriteAsync(package.Id.Version);
            await writer.WriteLineAsync(")");

            int dotLength = "License notice for ".Length + package.Id.Name.Length;
            await writer.WriteLineAsync(new string('-', dotLength));

            if (!string.IsNullOrEmpty(package.RepositoryUrl)) {
                await writer.WriteLineAsync();
                await writer.WriteAsync(package.RepositoryUrl);
                if (!string.IsNullOrEmpty(package.RepositoryCommit)) {
                    await writer.WriteAsync(" at ");
                    await writer.WriteAsync(package.RepositoryCommit);
                }

                await writer.WriteLineAsync();
            }

            if (!string.IsNullOrEmpty(package.ProjectUrl) && package.RepositoryUrl != package.ProjectUrl) {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync(package.ProjectUrl);
            }

            if (!string.IsNullOrEmpty(package.Copyright)) {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync(package.Copyright);
            }

            if (!string.IsNullOrEmpty(package.LicenseExpression)) {
                await writer.WriteLineAsync();
                await writer.WriteAsync("Licensed under ");
                await writer.WriteLineAsync(package.LicenseExpression);
            }

            if (!string.IsNullOrEmpty(package.LicenseUrl)) {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync("Available at");
                await writer.WriteLineAsync(package.LicenseUrl);
            } else if (!string.IsNullOrEmpty(package.LicenseContent)) {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync(package.LicenseContent);
            }

            await writer.WriteLineAsync();
            await writer.WriteLineAsync();
        }
    }
}
