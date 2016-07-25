using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsMerger.Core.Vcs
{
    public class UnresolvedConflictException
        : Exception
    {
        public UnresolvedConflictException(string changeSet)
            : base(String.Format("Conflict occured while merging {0} changeset", changeSet))
        {

        }
    }
}
