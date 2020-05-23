namespace ThirdLicense
{
    public class PackageId
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public override string ToString()
        {
            return $"[{Name}:{Version}]";
        }
    }
}
