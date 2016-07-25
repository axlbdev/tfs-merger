using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsMerger.Core.Vcs
{
    public interface IChangeset
    {
        string Id { get; }
        DateTime CreationDate { get; }
        int[] RelatedIssues { get; }
        string Comment { get; }
    }
}
