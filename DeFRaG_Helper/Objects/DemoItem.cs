using System;
using System.Text.RegularExpressions;

namespace DeFRaG_Helper
{
    public class DemoItem
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public long Size { get; set; }
        public string MapName { get; private set; }
        public string Type { get; private set; }
        public string Physics { get; private set; }
        public string Time { get; private set; }
        public long TotalMilliseconds { get; private set; }
        public string PlayerName { get; private set; }
        public string PlayerCountry { get; private set; }

        public string FormattedSize
        {
            get
            {
                if (Size >= 1_000_000) // More than or equal to 1 MB
                    return $"{Size / 1_000_000.0:#.##} MB";
                else if (Size >= 1_000) // More than or equal to 1 KB
                    return $"{Size / 1_000.0:#.##} KB";
                else
                    return $"{Size} Bytes";
            }
        }

        public void DecodeName()
        {
            var pattern = @"^(?<MapName>[^\[]+)\[(?<Type>[^\.]+)\.(?<Physics>[^\]]+)\](?<Time>\d{2}\.\d{2}\.\d{3})\((?<PlayerName>[^\.]+)\.(?<PlayerCountry>[^\)]+)\)";
            var match = Regex.Match(Name, pattern);
            if (match.Success)
            {
                MapName = match.Groups["MapName"].Value;
                Type = match.Groups["Type"].Value;
                Physics = match.Groups["Physics"].Value;
                Time = match.Groups["Time"].Value;
                TotalMilliseconds = ConvertTimeToMilliseconds(Time);
                PlayerName = match.Groups["PlayerName"].Value;
                PlayerCountry = match.Groups["PlayerCountry"].Value;
            }
            else
            {
                Console.WriteLine("No match found.");
            }
        }


        private long ConvertTimeToMilliseconds(string timeString)
        {
            // Split the time string into minutes, seconds, and milliseconds.
            var parts = timeString.Split(new char[] { '.', '.' });
            if (parts.Length == 3)
            {
                // Parse each part of the time string.
                int minutes = int.Parse(parts[0]);
                int seconds = int.Parse(parts[1]);
                int milliseconds = int.Parse(parts[2]);

                // Calculate total milliseconds.
                long totalMilliseconds = (minutes * 60 * 1000) + (seconds * 1000) + milliseconds;
                return totalMilliseconds;
            }
            else
            {
                // Return 0 or throw an exception if the format is incorrect.
                // Depending on your application's needs, you might choose to handle this differently.
                return 0;
            }
        }
        private string ConvertMillisecondsToTime(long totalMilliseconds)
        {
            // Calculate minutes, seconds, and milliseconds from the total milliseconds
            int minutes = (int)(totalMilliseconds / (60 * 1000));
            int seconds = (int)((totalMilliseconds / 1000) % 60);
            int milliseconds = (int)(totalMilliseconds % 1000);

            // Format and return the time string
            return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
        }










    }
}

