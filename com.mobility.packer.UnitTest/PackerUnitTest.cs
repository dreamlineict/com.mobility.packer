using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace com.mobility.packer.UnitTest
{
    [TestClass]
    public class PackerUnitTest
    {
        readonly Packer packer = new Packer();
        string filePath;

        public PackerUnitTest()
        {
            filePath = @"C:\Angular\TDD\PackagingChallenge.txt";
        }

        [TestMethod]
        public void TestFileExists()
        {
            try
            {
                //test/validate file exists
                packer.FileExists(filePath);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void Testpack()
        {            
            Assert.AreEqual("4\n-\n2,7\n8,9", packer.pack(filePath));
        }

        [TestMethod]
        public void TestValidatePackageContraints()
        {
            try
            {
                //mock test object to test constraints
                var package = new List<Package>
                {
                    new Package { PackageLimit = 10, PackageItems = new List<PackageItem>{ new PackageItem { IndexNumber = 1, Weight = 70, Cost = 12 } } }
                };
                packer.ValidatePackageContraints(package);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void TestValidateFileContents()
        {
            try
            {
                //test/validate file contents
                string[] fileContents = File.ReadAllLines(filePath);
                packer.ValidateFileContents(fileContents);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }
    }
}
