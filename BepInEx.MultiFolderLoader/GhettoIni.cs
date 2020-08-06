using System;
using System.Collections.Generic;
using System.IO;

namespace BepInEx.MultiFolderLoader
{
    public class GhettoIni
    {
        public static Dictionary<string, Section> Read(string path)
        {
            if (!File.Exists(path))
                return null;
            var result = new Dictionary<string, Section>(StringComparer.InvariantCultureIgnoreCase);
            using var sr = new StreamReader(path);
            var curSection = new Section {Name = "*"};

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith(";"))
                    continue;
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    result[curSection.Name] = curSection;

                    var sectionName = line.Substring(0, line.Length - 2).Trim().ToLower();
                    if (!result.TryGetValue(sectionName, out curSection))
                        curSection = new Section
                        {
                            Name = sectionName
                        };
                    continue;
                }

                var sep = line.IndexOf("=", StringComparison.Ordinal);
                if (sep == -1) continue;
                var key = line.Substring(0, sep).Trim();
                var value = sep < line.Length ? line.Substring(sep + 1).Trim() : "";
                curSection.Entries[key] = value;
            }

            if (curSection != null)
                result[curSection.Name] = curSection;

            return result;
        }

        public class Section
        {
            public string Name { get; set; }

            public Dictionary<string, string> Entries { get; } =
                new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }
    }
}