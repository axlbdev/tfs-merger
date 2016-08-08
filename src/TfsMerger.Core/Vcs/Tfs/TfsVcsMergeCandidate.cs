using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsMerger.Core.Vcs.Tfs
{
    public class TfsVcsMergeCandidate
        : IVcsMergeCandidate
    {
        public Microsoft.TeamFoundation.VersionControl.Client.MergeCandidate Candidate { get; private set; }
        public TfsVcsMergeCandidate(Microsoft.TeamFoundation.VersionControl.Client.MergeCandidate candidate)
        {
            this.Candidate = candidate;
        }
        public static explicit operator Microsoft.TeamFoundation.VersionControl.Client.MergeCandidate(TfsVcsMergeCandidate c)
        {
            return c.Candidate;
        }
        public static explicit operator TfsVcsMergeCandidate(Microsoft.TeamFoundation.VersionControl.Client.MergeCandidate c)
        {
            return new TfsVcsMergeCandidate(c);
        }
    }
}
