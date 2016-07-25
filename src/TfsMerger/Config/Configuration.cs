using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsMerger.Config
{
    public class Configuration
    {
        public Configuration()
        {
            BranchAliases = new Dictionary<string, string>();
            AutoResolve = new AutoResolveConfiguration();
        }
        public Dictionary<string, string> BranchAliases { get; set; }
        public Dictionary<string, string> MergeDirections { get; set; }
        public string[] BubblePath { get; set; }
        public string RepoUri { get; set; }
        public AutoResolveConfiguration AutoResolve { get; set; }
        public string MergeIssue { get; set; }
        public string ChangesetsFile { get; set; }
        public string DumpFile { get; set; }

        public string GetBranch(string alias)
        {
            return BranchAliases.ContainsKey(alias) ? BranchAliases[alias] : alias;
        }
    }
}
