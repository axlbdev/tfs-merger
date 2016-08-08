using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsMerger.Core.UI;

namespace TfsMerger
{
    public class ConsoleQuestionary
        : IQuestionary
    {
        public bool ConflictsResolved(string addInfo)
        {
            int i = 0;
            for (i = 0; i < 3; i++)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(addInfo);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Conflict cannot be automatically resolved, resolve it manually and enter 'continue'");
                Console.ResetColor();
                var answ = Console.ReadLine();
                if (answ != "continue")
                {
                    Console.WriteLine("You entered [" + answ + "] try again");
                }
                else
                {
                    break;
                }
            }
            if (i == 3)
            {
                return false;
            }
            return true;
        }
    }
}
