using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Nl2TrackGen.Models;

namespace Nl2TrackGen.Services
{
    public static class Nl2CsvExporter
    {
        public static string Export(List<TrackPoint> points)
        {
            var sb = new StringBuilder();

            // Header for NL2 2.6.8 (Tab separated)
            // "No." "PosX" "PosY" "PosZ" "FrontX" "FrontY" "FrontZ" "LeftX" "LeftY" "LeftZ" "UpX" "UpY" "UpZ"
            sb.AppendLine("\"No.\"\t\"PosX\"\t\"PosY\"\t\"PosZ\"\t\"FrontX\"\t\"FrontY\"\t\"FrontZ\"\t\"LeftX\"\t\"LeftY\"\t\"LeftZ\"\t\"UpX\"\t\"UpY\"\t\"UpZ\"");

            int counter = 1;
            foreach (var p in points)
            {
                // Strict invariant culture for numbers
                var culture = CultureInfo.InvariantCulture;

                sb.Append(counter.ToString(culture));
                sb.Append('\t');
                sb.Append(Format(p.Position.X));
                sb.Append('\t');
                sb.Append(Format(p.Position.Y));
                sb.Append('\t');
                sb.Append(Format(p.Position.Z));
                sb.Append('\t');
                sb.Append(Format(p.Front.X));
                sb.Append('\t');
                sb.Append(Format(p.Front.Y));
                sb.Append('\t');
                sb.Append(Format(p.Front.Z));
                sb.Append('\t');
                sb.Append(Format(p.Left.X));
                sb.Append('\t');
                sb.Append(Format(p.Left.Y));
                sb.Append('\t');
                sb.Append(Format(p.Left.Z));
                sb.Append('\t');
                sb.Append(Format(p.Up.X));
                sb.Append('\t');
                sb.Append(Format(p.Up.Y));
                sb.Append('\t');
                sb.Append(Format(p.Up.Z)); // Last one, no trailing tab needed, but new line
                sb.AppendLine();

                counter++;
            }

            return sb.ToString();
        }

        private static string Format(float value)
        {
            return value.ToString("F6", CultureInfo.InvariantCulture);
        }
    }
}
