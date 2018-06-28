using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
