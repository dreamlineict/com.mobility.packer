using com.mobility.packer.Exceptions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace com.mobility.packer
{
    public class Packer : IPacker
    {
        #region pack logic
        public string pack(string filePath)
        {
            StringBuilder Output = new StringBuilder { };
            try
            {
                //check if file exists and if not, throw 'file not found' exception, otherwise proceed.
                FileExists(filePath);

                //Read file
                string[] fileContents = File.ReadAllLines(filePath);

                //minimum File Content(s) validation
                ValidateFileContents(fileContents);

                var Packages = new List<Package>();
                for (int i = 0; i < fileContents.Length; i++)
                {
                    string PackageLine = fileContents[i];
                    int ColonIndex = PackageLine.IndexOf(':');
                    //get packageLimit before the colon
                    string packageLimit = PackageLine.Substring(0, ColonIndex - 1).Trim();
                    string[] items = PackageLine.Substring(ColonIndex + 1, (PackageLine.Length - (ColonIndex + 1))).TrimStart().TrimEnd().Split(' ');

                    var package = new Package
                    {
                        PackageLimit = int.Parse(packageLimit),
                        PackageItems = new List<PackageItem> { }
                    };
                    for (int x = 0; x < items.Length; x++)
                    {
                        //replace the parentheses and the get the items into a list by spliting them with the ',' character
                        string packageItem = items[x].Replace("(", "").Replace(")", "");
                        string[] packageItems = packageItem.Trim().Split(',');

                        try
                        {
                            package.PackageItems.Add(new PackageItem
                            {
                                IndexNumber = int.Parse(packageItems[0]), //get package item Index Number, then convert to integer
                                Weight = float.Parse(packageItems[1]), //get package item Weight, then convert to float
                                Cost = float.Parse(packageItems[2].Replace("€", ""))//get package item Cost, then convert to float...note: € removed
                            });
                        }
                        catch (APIException aex)
                        {
                            throw new APIException("Failed to convert package items data types", aex);
                        }
                    }

                    Packages.Add(package);
                }

                //Validate Package Contraints
                ValidatePackageContraints(Packages);

                for (int y = 0; y <= Packages.Count - 1; y++)
                {
                    //sort package items by cost in descending order, then by weight in ascending order
                    //this helps determine which things to put into the package so that the total weight is less than or equal
                    //to the package limit and the total cost is as large as possible.
                    var packageItems = Packages[y].PackageItems.OrderByDescending(x => x.Cost).ThenBy(x => x.Weight).ToList();
                    int limit = Packages[y].PackageLimit;

                    float weightSum = 0;
                    string outputValue = "";
                    for (int count = 0; count < packageItems.Count; count++)
                    {
                        if (packageItems[count].Weight <= limit)
                        {
                            //check if the current package items' cost matches the next items' cost and if so,
                            //take the one weighing less
                            if ((count + 1 < packageItems.Count) && (packageItems[count].Cost == packageItems[count + 1].Cost && packageItems[count].Weight < packageItems[count + 1].Weight))
                            {
                                PackageOutput(packageItems, limit, ref weightSum, ref outputValue, count);
                            }
                            else if (count == 0)
                            {
                                outputValue = "\n";
                                PackageOutput(packageItems, limit, ref weightSum, ref outputValue, count);
                            }
                            else if (count > 0 && packageItems[count - 1].Cost != packageItems[count].Cost || (packageItems[count - 1].Weight + packageItems[count].Weight < limit))
                            {
                                PackageOutput(packageItems, limit, ref weightSum, ref outputValue, count);
                            }

                            //if weight total exceeds package limit, break out of the item selection process.
                            if (weightSum > limit)
                            {
                                outputValue = string.Concat(outputValue.Replace("-", ""), "\n");
                                break;
                            }
                        }
                    }

                    //if no items within the package line items use the "-" character to show no match
                    if (outputValue.Trim().Length < 1)
                    {
                        outputValue = string.Concat(outputValue, "-\n");
                    }

                    //append output
                    Output.Append(ReplaceLastOccurrence(outputValue, ",", ""));
                }
            }
            catch (APIException aex)
            {
                throw new APIException("APIException", aex);
            }

            //Replace Last Occurrence of new line and then return output
            return ReplaceLastOccurrence(Output.ToString(), "\n", "");
        }

        public void FileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new APIException("File not Found", filePath);
            }
        }

        private static void PackageOutput(List<PackageItem> packageItems, int limit, ref float weightSum, ref string outputValue, int count)
        {
            weightSum += packageItems[count].Weight;
            if (weightSum <= limit)
            {
                outputValue += string.Concat(packageItems[count].IndexNumber.ToString(), ",");
            }
        }

        public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }

        public void ValidatePackageContraints(List<Package> packages)
        {
            //Max weight that a package can take is ≤ 100
            foreach (var package in packages)
            {
                if(package.PackageLimit > 100)
                {
                    throw new APIException
                    (
                        string.Format("APIException - Package Limit cannot exceed 100 - Package Limit = {0}", package.PackageLimit)
                    );
                }
            }

            //There might be up to 15 items you need to choose from
            foreach (var package in packages)
            {
                if (package.PackageItems.Count > 15)
                {
                    throw new APIException
                    (
                        string.Format("APIException - Max Package items cannot be higher than 15 - Package Limit = {0}, Items = {1}", 
                        package.PackageLimit, package.PackageItems.Count)
                    );
                }
            }

            //Max weight and cost of an item is ≤ 100
            foreach (var package in packages)
            {
                for(int i = 0; i <= package.PackageItems.Count - 1; i++)
                {
                    if(package.PackageItems[i].Weight > 100 || package.PackageItems[i].Cost > 100)
                    {
                        throw new APIException
                        (
                            string.Format("APIException - Max weight and cost of an item cannot be higher than 100 - Package Limit = {0}, Weight = {1}, Cost {2}",
                            package.PackageLimit, package.PackageItems[i].Weight, package.PackageItems[i].Cost)
                        );
                    }                    
                }
            }
        }

        public void ValidateFileContents(string[] fileContents)
        {
            for(int x = 0; x <= fileContents.Length - 1; x++)
            {
                if (fileContents[x].IndexOf(":") < 1)
                    throw new APIException("Invalid file content");
            }
        }
        #endregion
    }
}