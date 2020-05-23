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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IO;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;

    /// <summary>
    /// Represents a NuGet package inspector by providing with its metadata.
    /// </summary>
    public sealed class NuGetProtocolInspector : IDisposable
    {
        readonly RecyclableMemoryStreamManager memoryStreamManager;
        readonly SourceCacheContext cache;
        readonly List<FindPackageByIdResource> repositories;
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetProtocolInspector" /> class.
        /// </summary>
        public NuGetProtocolInspector()
        {
            memoryStreamManager = new RecyclableMemoryStreamManager();
            logger = NullLogger.Instance;
            cache = new SourceCacheContext();
            repositories = new List<FindPackageByIdResource>();
        }

        /// <summary>
        /// Gets a value indicating whether the resources of the object has been released.
        /// </summary>
        public bool Disposed {
            get;
            private set;
        }

        /// <summary>
        /// Releases the resources of the object.
        /// </summary>
        public void Dispose()
        {
            if (Disposed) {
                return;
            }

            Disposed = true;
            cache.Dispose();
        }

        /// <summary>
        /// Adds all the NuGet enabled endpoints configured in the system.
        /// </summary>
        /// <remarks>
        /// This methods reads the system configuration as well as the NuGet
        /// configuration files from the current working directory.
        /// </remarks>
        /// <returns>The asynchronous task.</returns>
        public async Task AddDefaultEndpointsAsync()
        {
            var settings = Settings.LoadDefaultSettings(Environment.CurrentDirectory);
            foreach (var source in SettingsUtility.GetEnabledSources(settings)) {
                await AddRepositoryAsync(source).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds an additional NuGet endpoint.
        /// </summary>
        /// <param name="endpoint">The NuGet endpoint.</param>
        /// <returns>The asynchronous task.</returns>
        public Task AddEndpointAsync(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) {
                throw new ArgumentNullException(nameof(endpoint));
            }

            var source = new PackageSource(endpoint);
            return AddRepositoryAsync(source);
        }

        /// <summary>
        /// Inspects a NuGet package to provide with its metadata.
        /// </summary>
        /// <param name="packageId">The package identifier to inspect.</param>
        /// <returns>The asynchronous task with the NuGet metadata.</returns>
        public Task<NuspecReader> InspectAsync(PackageIdentity packageId)
        {
            if (packageId == null) {
                throw new ArgumentNullException(nameof(packageId));
            }

            return InspectPackageAsync(packageId);
        }

        private async Task AddRepositoryAsync(PackageSource source)
        {
            SourceRepository repository;
            if (source.ProtocolVersion == 2) {
                repository = Repository.Factory.GetCoreV2(source);
            } else {
                repository = Repository.Factory.GetCoreV3(source.Source);
            }

            var resource = await repository.GetResourceAsync<FindPackageByIdResource>().ConfigureAwait(false);
            repositories.Add(resource);
        }

        private async Task<NuspecReader> InspectPackageAsync(PackageIdentity packageId)
        {
            foreach (var repo in repositories) {
                var nuspec = await InpsectFromRepoAsync(packageId, repo).ConfigureAwait(false);
                if (nuspec != null) {
                    return nuspec;
                }
            }

            Console.WriteLine($"- Cannot get info from: {packageId.Id} (v{packageId.Version.ToFullString()})");
            return null;
        }

        private async Task<NuspecReader> InpsectFromRepoAsync(PackageIdentity packageId, FindPackageByIdResource repo)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            using var stream = memoryStreamManager.GetStream();
            bool success = await repo.CopyNupkgToStreamAsync(
                packageId.Id,
                packageId.Version,
                stream,
                cache,
                logger,
                cancellationToken)
                .ConfigureAwait(false);

            if (!success) {
                return null;
            }

            Console.WriteLine($"+ {packageId.Id} (v{packageId.Version.ToFullString()})");
            using var reader = new PackageArchiveReader(stream);
            return await reader.GetNuspecReaderAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
