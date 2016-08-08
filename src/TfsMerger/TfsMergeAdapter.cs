using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TfsMerger.Core.Vcs;
using TfsMerger.Core.Vcs.Tfs;

namespace TfsMerger
{
    public class TfsMergeAdapter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        IMerger merger;
        IVcsContext context;
        public TfsMergeAdapter(string uri)
        {
            merger = new TfsVcsMerger();
            context = TfsVcsContext.Create(uri);
        }
        public void DumpCandidates(IDictionary<string, string> directions, string output)
        {
            DumpMergeCandidates(context, merger, directions, output);
        }

        public void Merge(string[] cs, string mergeIssue, IDictionary<string, string> directions, Regex[] acceptTheirs, Regex[] acceptYours, string[] bubblePath)
        {
            Logger.Info("Obtaining changesets");
            var changesets = merger.GetCandidates(context, directions)
                .Where(x => cs.Contains(x.Changeset.Id))
                .OrderBy(x => x.Changeset.CreationDate);

            Action<IEnumerable<TfsMerger.Core.Vcs.MergeCandidate>> merge = (chs) =>
            {
                var startFromPath = chs.First().Source;
                var changesetsToMerge = new List<IChangeset>(chs.Select(x => x.Changeset));

                Logger.Info("Merging batch {0} -> {1}: {2}", chs.First().Source, chs.First().Destination, String.Join(",", changesetsToMerge.Select(x=>x.Id)));
                
                var startFromIdx = bubblePath.Select((x, i) => new { x, i }).Where(x => x.x == startFromPath).Select(x => x.i).FirstOrDefault();
                for (var i = startFromIdx; i < bubblePath.Length - 1; i++)
                {
                    IChangeset[] merged = null;
                    Logger.Debug("Merging changesets {0} -> {1}: {2}", bubblePath[i], bubblePath[i + 1], String.Join(",", changesetsToMerge.Select(x => x.Id)));
                    if (MergeChangesets(context, merger, bubblePath[i], bubblePath[i + 1], changesetsToMerge.ToArray(), acceptTheirs, null, mergeIssue, out merged))
                    {
                        if (merged != null && merged.Length > 0)
                        {
                            changesetsToMerge.Clear();
                            changesetsToMerge.AddRange(merged);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            };
            Logger.Info("Processing changesets");

            string lastSource = null, lastDest = null;
            var batch = new List<TfsMerger.Core.Vcs.MergeCandidate>();
            foreach (var mergeCandidate in changesets)
            {
                if (lastSource != null && lastDest != null && (mergeCandidate.Source != lastSource || mergeCandidate.Destination != lastDest))
                {
                    if(batch.Any())
                    {
                        merge(batch);
                    }
                    batch.Clear();
                }

                batch.Add(mergeCandidate);
                lastSource = mergeCandidate.Source;
                lastDest = mergeCandidate.Destination;
            }
            if(batch.Any())
            {
                merge(batch);
            }
        }

        private static bool MergeChangesets(IVcsContext ctx, IMerger merger, string src, string dst, string[] changesets, Regex[] accepthTheirs, Regex[] acceptYours, string mergeWi, out string[] mergeChangesets)
        {
            var result = new List<string>();
            mergeChangesets = null;


            var history = merger.GetCandidates(ctx, new Dictionary<string, string>() { { src, dst } })
                .Select(x => x.Changeset)
                .Where(x => changesets.Contains(x.Id))
                .OrderBy(x => x.CreationDate);

            Logger.Info("Merging {0} changesets", history.Count());
            var cq = new ConsoleQuestionary();

            foreach (var changeset in history)
            {
                var cn = merger.MergeChangeset(ctx, src, dst, changeset, accepthTheirs, acceptYours, cq, mergeWi);
                result.Add(cn.Id);
            }
            mergeChangesets = result.ToArray();
            return true;
        }
        private static bool MergeChangesets(IVcsContext ctx, IMerger merger, string src, string dst, IChangeset[] changesets, Regex[] accepthTheirs, Regex[] acceptYours, string mergeWi, out IChangeset[] mergeChangesets)
        {
            var result = new List<IChangeset>();
            mergeChangesets = null;



            Logger.Info("Merging {0} changesets", changesets.Count());
            var cq = new ConsoleQuestionary();

            foreach (var changeset in changesets)
            {
                var cn = merger.MergeChangeset(ctx, src, dst, changeset, accepthTheirs, acceptYours, cq, mergeWi);
                result.Add(cn);
            }
            mergeChangesets = result.ToArray();
            return true;
        }

        private static IEnumerable<TfsMerger.Core.Vcs.MergeCandidate> DumpMergeCandidates(IVcsContext ctx, IMerger merger, IDictionary<string, string> directions, string dumpFile = null)
        {

            var overallHistory = new List<TfsMerger.Core.Vcs.MergeCandidate>();

            foreach (var direction in directions)
            {
                overallHistory.AddRange(merger.GetCandidates(ctx, new Dictionary<string, string>() { { direction.Key, direction.Value } }));
            }
            if (!String.IsNullOrEmpty(dumpFile))
            {
                using (var f = File.OpenWrite(dumpFile))
                {
                    using (var sw = new StreamWriter(f))
                    {
                        foreach (var item in overallHistory
                            .OrderBy(x => x.Changeset.CreationDate))
                        {
                            var line = String.Format("{0} {1}\t->\t{2}\t{3}\t{4}\t{5}", item.Changeset.CreationDate, Path.GetFileName(item.Source), Path.GetFileName(item.Destination), item.Changeset.Id, String.Join(",", item.Changeset.RelatedIssues), item.Changeset.Comment).Replace(Environment.NewLine, " ");
                            sw.WriteLine(line);
                        }
                    }
                }
            }
            return overallHistory
                .OrderBy(x => x.Changeset.CreationDate);
        }
    }
}
