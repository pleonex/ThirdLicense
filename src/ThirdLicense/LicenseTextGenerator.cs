namespace ThirdLicense
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public class LicenseTextGenerator
    {
        public Task<Stream> Generate(IAsyncEnumerable<PackageInfo> packages)
        {
            return Task.FromException<Stream>(new NotImplementedException());
        }
    }
}
