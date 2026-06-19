using System.Collections.Generic;

namespace HomeGenie.Data
{
    public class VirtualModule : Module
    {
        public string ParentId { get; set; } = string.Empty;

        public bool IsActive = true;

        public readonly List<string> ImplementFeatures = new List<string>();
    }
}
