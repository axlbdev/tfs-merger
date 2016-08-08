using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsMerger.Core.Vcs
{
    public class MergeCandidate
    {
        public IChangeset Changeset { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public IVcsMergeCandidate Candidate { get; set; }
        public override string ToString()
        {
            return String.Format("{0} {1}\t->\t{2}\t{3}\t{4}\t{5}", Changeset.CreationDate, Path.GetFileName(Source), Path.GetFileName(Destination), Changeset.Id, String.Join(",", Changeset.RelatedIssues), Changeset.Comment).Replace(Environment.NewLine, " ");
        }
    }
}
