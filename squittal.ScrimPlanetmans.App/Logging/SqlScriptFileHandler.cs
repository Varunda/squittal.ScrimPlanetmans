using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace squittal.ScrimPlanetmans.Logging
{
    public class SqlScriptFileHandler
    {
        public static IEnumerable<string> GetAdHocSqlFileNames()
        {
            var basePath = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            var adhocScriptDirectory = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "../sql_adhoc"));

            try
            {
                return Directory.GetFiles(adhocScriptDirectory)
                    .Where(iter => iter.EndsWith(".sql"))
                    .Select(iter => Path.GetFileName(iter))
                    .OrderBy(f => f).ToList();
            }
            catch
            {
                // Ignore
                return null;
            }
        }
    }
}
