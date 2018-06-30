using System.Collections.Generic;
using System.Linq;

namespace CustomCommunityCenter
{
    public class BundleAreaInfo
    {
        public int Id { get; set; }
        public bool Collected { get; set; }
        public List<BundleInfo> Bundles { get; set; }
        
        public bool Completed
        {
            get { return Bundles.All(x => x.Completed); }
        }

        
    }
}
