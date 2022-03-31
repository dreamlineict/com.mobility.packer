using com.mobility.packer;
using System.Web.Http;

namespace PackageChallenge.Controllers
{
    [RoutePrefix("api/Package")]
    public class PackageController : ApiController
    {
        readonly Packer packer = new Packer();

        [Route("Packer")]
        [HttpGet]
        public string Packer(string filePath)
        {
            return packer.pack(filePath);
        }
    }
}
