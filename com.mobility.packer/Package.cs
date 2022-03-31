using System.Collections.Generic;

namespace com.mobility.packer
{
    public class Package
    {
        public int PackageLimit { get; set; }
        public List<PackageItem> PackageItems { get; set; }
    }

    public class PackageItem
    {
        public int IndexNumber { get; set; }
        public float Weight { get; set; }
        public float Cost { get; set; }
    }
}