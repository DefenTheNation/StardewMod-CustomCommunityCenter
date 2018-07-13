using CustomCommunityCenter.Data;

namespace CustomCommunityCenter.API
{
    public interface ICommunityCenterHelper
    {
        void AddBundle(string bundleAreaName, BundleInfo bundle);
        void RemoveBundle(string bundleAreaName, string bundleName);
    }
}
