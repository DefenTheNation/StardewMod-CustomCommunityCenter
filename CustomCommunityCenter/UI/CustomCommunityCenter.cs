using StardewValley.Locations;

namespace CustomCommunityCenter
{
    public class CustomCommunityCenter : CommunityCenter
    {
        protected static string CommunityCenterName = "CommunityCenter";

        public CustomCommunityCenter() : base(CommunityCenterName)
        {
        }

        public CustomCommunityCenter(string name) : base(name)
        {

        }
    }
}
