using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsonToMrv
{
    class SmilesReplaceHelper
    {
        public static string ReplaceInvalidCaractorsToService(string smiles)
        {
            return smiles.Replace("§1", "#").Replace("§2", "/").Replace("§3", "\\").Replace("§5", "%").Replace("§6", "+");
        }

        public static string ReplaceInvalidCaractorsToWeb(string smiles)
        {
            return smiles.Replace("#", "§1").Replace("/", "§2").Replace("\\", "§3").Replace("%", "§5").Replace("+", "§6");
        }
    }
}
