using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace G_Rewind.classes
{
    internal class GCodeLine
    {
        public string OriginalLine { get; private set; }
        public Dictionary<string, decimal> Parameters { get; private set; }
        public string Comment { get; private set; }

        public GCodeLine(string line)
        {
            OriginalLine = line;
            Parameters = new Dictionary<string, decimal>();
            ParseLine();
        }

        private void ParseLine()
        {
            var parts = OriginalLine.Split(';');
            var commandPart = parts[0].Trim();
            Comment = parts.Length > 1 ? ";" + parts[1].Trim() : "";

            var matches = Regex.Matches(commandPart, @"([A-Z])([-+]?\d*\.?\d+)");
            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                var value = decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                Parameters[key] = value;
            }
        }

        public override string ToString()
        {
            var line = OriginalLine;
            return line;
        }

        public void TagZ(decimal z)
        {
            if (!Parameters.ContainsKey("Z"))
            {
                Parameters["Z"] = z;
            }
        }

        public void TagF(decimal f)
        {
            if (!Parameters.ContainsKey("F"))
            {
                Parameters["F"] = f;
            }
        }
    }
}