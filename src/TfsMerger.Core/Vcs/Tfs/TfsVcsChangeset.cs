using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsMerger.Core.Vcs.Tfs
{
    public class TfsVcsChangeset
        : IChangeset
    {
        public readonly Changeset Changeset;
        public TfsVcsChangeset(Changeset cs)
        {
            Changeset = cs;
        }

        public string Id
        {
            get { return Changeset.ChangesetId.ToString(); }
        }

        public DateTime CreationDate
        {
            get { return Changeset.CreationDate; }
        }

        public int[] RelatedIssues
        {
            get { return Changeset.WorkItems.Select(x=>x.Id).ToArray(); }
        }

        public string Comment
        {
            get { return Changeset.Comment; }
        }
    }
}
