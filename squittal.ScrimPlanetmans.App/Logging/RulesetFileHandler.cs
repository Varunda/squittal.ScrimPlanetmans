using System;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using squittal.ScrimPlanetmans.ScrimMatch.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace squittal.ScrimPlanetmans.Logging
{
    public class RulesetFileHandler
    {
        public async static Task<bool> WriteToJsonFile(string fileName, JsonRuleset ruleset)
        {
            var basePath = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            var rulesetsDirectory = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "../rulesets"));

            var path = fileName.EndsWith(".json") ? $"{rulesetsDirectory}/{fileName}" : $"{rulesetsDirectory}/{fileName}.json";

            try
            {
                using (FileStream fileStream = File.Create(path))
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                    await JsonSerializer.SerializeAsync(fileStream, ruleset, options);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
                return false;
            }
        }

        public async static Task<JsonRuleset> ReadFromJsonFile(string fileName)
        {
            var basePath = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            var rulesetsDirectory = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "../rulesets"));

            var path = fileName.EndsWith(".json") ? $"{rulesetsDirectory}/{fileName}" : $"{rulesetsDirectory}/{fileName}.json";

            try
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    var ruleset = await JsonSerializer.DeserializeAsync<JsonRuleset>(fileStream);

                    return ruleset;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
                
                return null;
            }
        }

        public static IEnumerable<string> GetJsonRulesetFileNames()
        {
            var basePath = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            var rulesetsDirectory = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "../rulesets"));

            var rulesets = new List<string>();

            try
            {
                string[] files = Directory.GetFiles(rulesetsDirectory)
                    .Where(iter => iter.EndsWith(".json")).ToArray();

                foreach (string file in files) {
                    rulesets.Add(Path.GetFileName(file));
                }

                return rulesets.OrderBy(f => f).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
                return null;
            }
        }
    }
}
