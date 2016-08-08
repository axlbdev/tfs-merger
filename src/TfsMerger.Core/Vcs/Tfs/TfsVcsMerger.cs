using Microsoft.TeamFoundation.VersionControl.Client;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TfsMerger.Core.UI;

namespace TfsMerger.Core.Vcs.Tfs
{
    public class TfsVcsMerger
        : IMerger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public IEnumerable<MergeCandidate> GetCandidates(IVcsContext context, IDictionary<string, string> directions)
        {
            var ctx = (TfsVcsContext)context;
            var allBranches = new List<string>();
            foreach (var branch in directions)
            {
                allBranches.Add(branch.Key);
                allBranches.Add(branch.Value);
            }
            allBranches = allBranches.Distinct().ToList();

            foreach (var branch in allBranches)
            {
                ctx.Workspace.Get(new GetRequest(Path.Combine(branch, "*"), RecursionType.Full, VersionSpec.Latest), GetOptions.None);
            }

            var overallHistory = new List<MergeCandidate>();

            foreach (var direction in directions)
            {
                overallHistory.AddRange(ctx.VCS.GetMergeCandidates(direction.Key, direction.Value, RecursionType.Full)
                    .Select(x => new MergeCandidate { Candidate = (TfsVcsMergeCandidate)x, Changeset = new TfsVcsChangeset(x.Changeset), Source = direction.Key, Destination = direction.Value }));
            }
            return overallHistory;
        }


        public IChangeset MergeChangeset(IVcsContext context, string src, string dst, IChangeset changeset, Regex[] acceptTheirs, Regex[] acceptYours, IQuestionary questionary, string mergeIssue = null)
        {
            var ctx = (TfsVcsContext)context;

            var id = changeset.Id;

            Logger.Trace("Merging changeset {0}", id);

            var status = ctx.Workspace.Merge(src, dst, new ChangesetVersionSpec(id), new ChangesetVersionSpec(id), LockLevel.Unchanged, RecursionType.Full, MergeOptions.None);
            Logger.Trace("Merge status: conflicts - {0}, failures - {1}", status.NumConflicts, status.NumFailures);

            
            var mergeWi = int.Parse(mergeIssue);
            var cs = ((TfsVcsChangeset)changeset).Changeset;
            var comment = String.Format("Merge {0} -> {1} ({2}:{3}) {4}", Path.GetFileName(src), Path.GetFileName(dst), cs.ChangesetId, String.Join(",", cs.WorkItems.Select(x => x.Id).Where(x => mergeWi <= 0 || x != mergeWi)), changeset.Comment);
            Logger.Info("Merge comment: {0}", comment);

            Conflict[] conflicts = null;
            if (status.NumConflicts > 0)
            {
                conflicts = ctx.Workspace.QueryConflicts(new string[] { dst }, true);

                foreach (Conflict conflict in conflicts)
                {

                    if (acceptTheirs != null && acceptTheirs.Any(x=>x.IsMatch(conflict.FileName)))
                    {
                        Logger.Trace("Conflict on {0} autoresolved with AcceptTheirs", conflict.FileName);
                        conflict.Resolution = Resolution.AcceptTheirs;
                        ctx.Workspace.ResolveConflict(conflict);
                    }
                    else if (acceptYours != null && acceptYours.Any(x => x.IsMatch(conflict.FileName)))
                    {
                        Logger.Trace("Conflict on {0} autoresolved with AcceptYours", conflict.FileName);
                        conflict.Resolution = Resolution.AcceptYours;
                        ctx.Workspace.ResolveConflict(conflict);
                    }
                    else if (MergeConflict(ctx, conflict))
                    {
                        Logger.Trace("Conflict on {0} automerged", conflict.FileName);
                        conflict.Resolution = Resolution.AcceptMerge;
                    }
                    if (conflict.IsResolved && !String.IsNullOrEmpty(conflict.MergedFileName) && File.Exists(conflict.MergedFileName))
                    {
                        ctx.Workspace.PendEdit(conflict.TargetLocalItem);
                        File.Copy(conflict.MergedFileName, conflict.TargetLocalItem,
                            true);
                    }
                }
                ctx.Workspace.AutoResolveValidConflicts(conflicts, AutoResolveOptions.All);
            }
            conflicts = ctx.Workspace.QueryConflicts(new string[] { dst }, true);
            if (conflicts.Any(x => !x.IsResolved))
            {
                if(questionary == null)
                {
                    throw new UnresolvedConflictException(id);
                }
                if(!questionary.ConflictsResolved(comment))
                {
                    throw new UnresolvedConflictException(id);
                }
            }
            conflicts = ctx.Workspace.QueryConflicts(new string[] { dst }, true);
            if (conflicts.Any(x => !x.IsResolved))
            {
                throw new UnresolvedConflictException(id);
            }
            var pc = ctx.Workspace.GetPendingChanges(ctx.MapPath(dst), RecursionType.Full);
            if (pc.Length > 0)
            {
                var wiInfo = cs.WorkItems
                    .Union(mergeWi >= 0 ? new[] { ctx.WiStore.GetWorkItem(mergeWi) } : new Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem[0])
                    .Where(x => x.Type.Name == "Задача")
                    .Select(x => new WorkItemCheckinInfo(x, WorkItemCheckinAction.Associate))
                    .ToArray();

                int cn;
                try
                {
                    cn = ctx.Workspace.CheckIn(pc, comment, null, wiInfo, null);
                    if (cn <= 0)
                    {
                        throw new PushFailureException(String.Format("Check-in failed with {0}, exiting", cn));
                    }
                    Logger.Info("Check-in: {0} {1}", cn, comment);
                }
                catch (CheckinException ex)
                {
                    throw new PushFailureException(String.Format("Check-in failed with {0}, exiting", ex.ToString()));
                }
                return (TfsVcsChangeset)ctx.VCS.GetChangeset(cn);
            }
            return null;
        }
        private static bool MergeConflict(TfsVcsContext ctx, Conflict conflict)
        {

            if (ctx.Workspace.MergeContent(conflict, true))
            {
                return true;
            }
            return false;
        }

    }
}
