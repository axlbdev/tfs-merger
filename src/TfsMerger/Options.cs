using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsMerger
{
    public class Options
    {
        public Options()
        {
            DumpVerb = new DumpSubOptions();
            MergeVerb = new MergeSubOptions();
        }

        [VerbOption("dump", HelpText = "Dump candidates for merge")]
        public DumpSubOptions DumpVerb { get; set; }
        [VerbOption("merge", HelpText = "Merge changesets")]
        public MergeSubOptions MergeVerb { get; set; }

        [HelpOption(HelpText = "Show help")]
        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            var sb = new StringBuilder(HelpText.AutoBuild(this, verb));
            return sb.ToString();
        }
    }
    public class DumpSubOptions
        : ConfigFileOptions
    {
    }
    public class MergeSubOptions
        : ConfigFileOptions
    {
    }
    public class ConfigFileOptions
    {
        [Option('c', "config", Required=true, HelpText = @"
Path to configuration file for merge|dump operations. Example of file contents:
{
  ""BranchAliases"": {
    ""branchMain"": ""$\\Demo\\Main"",
    ""branchProd"": ""$\\Demo\\Prod"",
    ""branchFuture"": ""$\\Demo\\Future"",
    ""branchTest"": ""$\\Demo\\Test""
  },
  ""MergeDirections"": {
    ""branchFuture"": ""branchMain"",
    ""branchMain"": ""branchTest""
  },
  ""BubblePath"": [
    ""branchFuture"",
    ""branchMain"",
    ""branchTest""
  ],
  ""RepoUri"": ""http://corporatetfs.local/tfs"",
  ""AutoResolve"": {
    ""AcceptTheirs"": [
      ""scripts\\.js"",
      ""scripts\\.min\\.js"",
      ""scripts\\.min\\.js\\.map""
    ],
    ""AcceptYours"": null
  },
  ""MergeIssue"": ""1234"",
  ""ChangesetsFile"": ""changesets.txt""
}   
")]
        public string Config { get; set; }
    }
}
