using System.Collections.Generic;

namespace Builder
{
    internal class BuildInfo
    {
        public List<Build> Builds { get; set; }

        public class Build
        {
            public string ProjectParentPath { get; set; }
            public string ProjectName { get; set; }
            public string ModFolderName { get; set; }
            public string DestinationFolderName { get; set; }
            public string[] ExtraFiles { get; set; }
            public string DDSContent { get; set; }
            public bool Package { get; set; }
        }
    }
}
