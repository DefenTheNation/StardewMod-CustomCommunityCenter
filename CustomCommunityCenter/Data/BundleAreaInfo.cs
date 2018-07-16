using System.Collections.Generic;
using System.Linq;

namespace CustomCommunityCenter.Data
{
    public class BundleAreaInfo
    {
        public BundleAreaInfo()
        {
            Bundles = new List<BundleInfo>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string RewardName { get; set; }
        public bool Collected { get; set; }
        public List<BundleInfo> Bundles { get; set; }
        
        public bool Completed
        {
            get { return Bundles.All(x => x.Completed); }
        }        

        public int BundlesCompleted
        {
            get { return Bundles.Where(x => x.Completed).ToList().Count; }
        }
    }
}
