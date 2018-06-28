using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCommunityCenter
{
    public class BundleInfo
    {
        public string Name { get; set; }
        public int RewardItemType { get; set; }
        public int RewardItemId { get; set; }
        public int RewardStack { get; set; }
        public List<BundleItemInfo> Requirements { get; set; }

        public bool Completed
        {
            get { return Requirements.All(x => x.Completed); }
        }
    }
}
