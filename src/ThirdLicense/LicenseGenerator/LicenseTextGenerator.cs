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
namespace ThirdLicense.LicenseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using NuGet.Packaging;

    public static class LicenseTextGenerator
    {
        public static Task Generate(Stream output, IAsyncEnumerable<NuspecReader> packages)
        {
            if (output == null) {
                throw new ArgumentNullException(nameof(output));
            }

            if (packages == null) {
                throw new ArgumentNullException(nameof(packages));
            }

            return GenerateInternalAsync(output, packages);
        }

        private static async Task GenerateInternalAsync(Stream output, IAsyncEnumerable<NuspecReader> packages)
        {
            using var writer = new StreamWriter(output, Encoding.UTF8, 1024, true);
            await foreach (var package in packages) {
                if (package.GetDevelopmentDependency()) {
                    Console.WriteLine($"- Dev dependency: {package.GetId()}");
                    continue;
                }

                await WritePackageNotice(writer, package).ConfigureAwait(false);
            }
        }

        private static async Task WritePackageNotice(StreamWriter writer, NuspecReader package)
        {
            await writer.WriteAsync("License notice for ").ConfigureAwait(false);
            await writer.WriteAsync(package.GetId()).ConfigureAwait(false);
            await writer.WriteAsync(" (v").ConfigureAwait(false);
            await writer.WriteAsync(package.GetVersion().ToFullString()).ConfigureAwait(false);
            await writer.WriteLineAsync(")").ConfigureAwait(false);
            await writer.WriteLineAsync("------------------------------------").ConfigureAwait(false);

            var repository = package.GetRepositoryMetadata();
            if (repository != null) {
                await writer.WriteLineAsync().ConfigureAwait(false);
                await writer.WriteAsync(repository.Url).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(repository.Commit)) {
                    await writer.WriteAsync(" at ").ConfigureAwait(false);
                    await writer.WriteAsync(repository.Commit).ConfigureAwait(false);
                }

                await writer.WriteLineAsync().ConfigureAwait(false);
            }

            await WriteIfNotEmpty(writer, string.Empty, package.GetProjectUrl()).ConfigureAwait(false);
            await WriteIfNotEmpty(writer, string.Empty, package.GetCopyright()).ConfigureAwait(false);

            var license = package.GetLicenseMetadata();
            if (license != null) {
                await WriteIfNotEmpty(writer, "Licensed under ", license.LicenseExpression.ToString()).ConfigureAwait(false);
                await WriteIfNotEmpty(writer, "Available at ", license.LicenseUrl.AbsolutePath).ConfigureAwait(false);
                await WriteIfNotEmpty(writer, string.Empty, license.License).ConfigureAwait(false);
            }

            await writer.WriteLineAsync().ConfigureAwait(false);
            await writer.WriteLineAsync().ConfigureAwait(false);
        }

        private static async Task WriteIfNotEmpty(StreamWriter writer, string prefix, string content)
        {
            if (!string.IsNullOrEmpty(content)) {
                await writer.WriteLineAsync().ConfigureAwait(false);
                await writer.WriteAsync(prefix).ConfigureAwait(false);
                await writer.WriteLineAsync(content).ConfigureAwait(false);
            }
        }
    }
}
