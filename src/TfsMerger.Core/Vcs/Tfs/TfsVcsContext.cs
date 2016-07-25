using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsMerger.Core.Vcs.Tfs
{
    public class TfsVcsContext
        : IVcsContext
    {
        public TfsTeamProjectCollection Collection { get; set; }
        public VersionControlServer VCS { get; set; }
        public WorkItemStore WiStore { get; set; }
        public Workspace Workspace { get; set; }

        private TfsVcsContext (string collectionUri)
        {
            Collection = new TfsTeamProjectCollection(new Uri(collectionUri));
            VCS = (VersionControlServer)Collection.GetService(typeof(VersionControlServer));
            WiStore = (WorkItemStore)Collection.GetService(typeof(WorkItemStore));
            var wss = VCS.QueryWorkspaces(null, VCS.AuthenticatedUser, System.Net.Dns.GetHostName().ToString());
            Workspace = wss.First();
        }

        public string MapPath(string tfsPath)
        {
            return Path.Combine(Workspace.Folders.First().LocalItem, tfsPath.Substring(2));
        }

        public static IVcsContext Create(string collectionUri)
        {
            return new TfsVcsContext(collectionUri);
        }
    }
}
