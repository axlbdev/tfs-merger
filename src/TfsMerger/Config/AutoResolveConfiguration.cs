using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TfsMerger.Config
{
    public class AutoResolveConfiguration
    {
        public string[] AcceptTheirs { get; set; }
        public string[] AcceptYours { get; set; }
    }
}
