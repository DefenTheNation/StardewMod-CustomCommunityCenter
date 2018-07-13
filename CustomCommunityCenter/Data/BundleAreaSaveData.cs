using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCommunityCenter.Data
{
    public class BundleAreaSaveData
    {
        public string Name { get; set; }
        public List<BundleSaveData> Bundles { get; set; }
    }
}
