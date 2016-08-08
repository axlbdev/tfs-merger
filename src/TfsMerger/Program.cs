using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

using Microsoft.VisualStudio.Services.Client;

using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;

using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using TfsMerger.Core.Vcs.Tfs;
using TfsMerger.Core.Vcs;
using System.Text.RegularExpressions;


namespace TfsMerger
{
    class Program
    {
        static int Main(string[] args)
        {
            string invokedVerb = null;
            object invokedVerbInstance = null;

            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options,
              (verb, subOptions) =>
              {
                  invokedVerb = verb;
                  invokedVerbInstance = subOptions;
              }))
            {
                return 1;
            }

            var co = invokedVerbInstance as ConfigFileOptions;
            var cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<Config.Configuration>(File.ReadAllText(co.Config));

            var m = new TfsMergeAdapter(cfg.RepoUri);

            try
            {
                if (invokedVerb == "dump")
                {
                    Console.WriteLine("Dumping");
                    var directions = cfg.MergeDirections
                        .Select(x => new { Src = cfg.GetBranch(x.Key), Dst = cfg.GetBranch(x.Value) })
                        .ToDictionary(x => x.Src, x => x.Dst);
                    m.DumpCandidates(directions, cfg.DumpFile);
                }
                if (invokedVerb == "merge")
                {
                    Console.WriteLine("Merging");
                    var directions = cfg.MergeDirections
                        .Select(x => new { Src = cfg.GetBranch(x.Key), Dst = cfg.GetBranch(x.Value) })
                        .ToDictionary(x => x.Src, x => x.Dst);
                    var cs = File.ReadLines(cfg.ChangesetsFile)
                        .Where(x => x != null)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToArray();
                    m.Merge(cs, cfg.MergeIssue, directions,
                        cfg.AutoResolve.AcceptTheirs == null
                            ? null
                            : cfg.AutoResolve.AcceptTheirs.Select(x => new Regex(x)).ToArray(),
                        cfg.AutoResolve.AcceptYours == null
                            ? null
                            : cfg.AutoResolve.AcceptYours.Select(x => new Regex(x)).ToArray(),
                        cfg.BubblePath.Select(x=>cfg.GetBranch(x)).ToArray());
                }
            } catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
                return 1;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All done");
            Console.ResetColor();
            return 0;
        }
    }
}

