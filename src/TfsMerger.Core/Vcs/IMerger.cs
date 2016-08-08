using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TfsMerger.Core.UI;

namespace TfsMerger.Core.Vcs
{
    public interface IMerger
    {
        IEnumerable<MergeCandidate> GetCandidates(IVcsContext context, IDictionary<string, string> directions);
        IChangeset MergeChangeset(IVcsContext context, string src, string dst, IChangeset changeset, Regex[] acceptTheirs, Regex[] acceptYours, IQuestionary questionary, string mergeIssue = null);
    }
}
