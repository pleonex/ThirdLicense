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
namespace ThirdLicense.NuGetInspect
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;

    public class NuGetProtocolInspector
    {
        readonly SourceCacheContext cache;
        readonly List<FindPackageByIdResource> repositories;
        readonly ILogger logger;

        public NuGetProtocolInspector()
        {
            logger = NullLogger.Instance;
            cache = new SourceCacheContext();
            repositories = new List<FindPackageByIdResource>();
        }

        public async Task AddDefaultEndpointsAsync()
        {
            var settings = Settings.LoadDefaultSettings(Environment.CurrentDirectory);
            foreach (var source in SettingsUtility.GetEnabledSources(settings)) {
                await AddRepositoryAsync(source);
            }
        }

        public async Task AddEndpointAsync(string endpoint)
        {
            var source = new PackageSource(endpoint);
            await AddRepositoryAsync(source);
        }

        public async Task<NuspecReader> InspectAsync(PackageIdentity packageId)
        {
            foreach (var repo in repositories) {
                var nuspec = await InpsectFromRepoAsync(packageId, repo);
                if (nuspec != null) {
                    return nuspec;
                }
            }

            Console.WriteLine($"- Cannot get info from: {packageId.Id} (v{packageId.Version.ToFullString()})");
            return null;
        }

        private async Task AddRepositoryAsync(PackageSource source)
        {
            SourceRepository repository;
            if (source.ProtocolVersion == 2) {
                repository = Repository.Factory.GetCoreV2(source);
            } else {
                repository = Repository.Factory.GetCoreV3(source.Source);
            }

            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            repositories.Add(resource);
        }

        private async Task<NuspecReader> InpsectFromRepoAsync(PackageIdentity packageId, FindPackageByIdResource repo)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            using var stream = new MemoryStream();
            bool success = await repo.CopyNupkgToStreamAsync(packageId.Id, packageId.Version, stream, cache, logger, cancellationToken);
            if (!success) {
                return null;
            }

            Console.WriteLine($"+ {packageId.Id} (v{packageId.Version.ToFullString()})");
            using var reader = new PackageArchiveReader(stream);
            return await reader.GetNuspecReaderAsync(cancellationToken);
        }
    }
}
