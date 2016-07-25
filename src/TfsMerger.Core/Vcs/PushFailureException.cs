using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsMerger.Core.Vcs
{
    public class PushFailureException
         :Exception
    {
        public PushFailureException(string message)
            : base(message)
        { }
    }
}
