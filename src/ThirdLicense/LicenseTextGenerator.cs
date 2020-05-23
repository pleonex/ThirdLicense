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
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class LicenseTextGenerator
    {
        public async Task Generate(Stream output, IAsyncEnumerable<PackageInfo> packages)
        {
            using var writer = new StreamWriter(output, Encoding.UTF8, 1024, true);
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
