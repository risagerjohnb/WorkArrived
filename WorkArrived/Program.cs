using System.Diagnostics;
using System.Text.RegularExpressions;

class Program
{
    static string workWifiSSID = "On720.com";
    static string countFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "work_count.txt");
    static string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "work_log.txt");

    static void Main()
    {
        Console.WriteLine("Searching Wifi...");

        string currentWifi = GetConnectedWifi();
        Console.WriteLine($"Connected to: {currentWifi}");

        if (currentWifi == workWifiSSID)
        {
            DateTime lastRunDate = ReadLastRunDate();
            DateTime currentDate = DateTime.Now.Date;

            if (lastRunDate != currentDate)
            {
                int count = ReadCount();
                count++;
                WriteCount(count);
                WriteTimestamp(count);
                Console.WriteLine($"Work wifi detected! Count increased to: {count}");
            }
            else
            {
                Console.WriteLine("Already counted for today.");
            }
        }
        else
        {
            Console.WriteLine("Not connected to work wifi.");
        }
    }

    static string GetConnectedWifi()
    {
        if (OperatingSystem.IsWindows())
        {
            return GetWifiWindows();
        }
        else if (OperatingSystem.IsMacOS())
        {
            return GetWifiMac();
        }
        return "Unknown OS";
    }

    static string GetWifiWindows()
    {
        Process process = new Process();
        process.StartInfo.FileName = "netsh";
        process.StartInfo.Arguments = "wlan show interfaces";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Match match = Regex.Match(output, @"SSID\s+:\s+(.*)");
        return match.Success ? match.Groups[1].Value.Trim() : "Unknown";
    }

    static string GetWifiMac()
    {
        Process process = new Process();
        process.StartInfo.FileName = "/System/Library/PrivateFrameworks/Apple80211.framework/Versions/Current/Resources/airport";
        process.StartInfo.Arguments = "-I";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Match match = Regex.Match(output, @"SSID: (.+)");
        return match.Success ? match.Groups[1].Value.Trim() : "Unknown";
    }

    static int ReadCount()
    {
        if (File.Exists(countFile))
        {
            string countStr = File.ReadAllText(countFile);
            if (int.TryParse(countStr, out int count))
            {
                return count;
            }
        }
        return 0;
    }

    static void WriteCount(int count)
    {
        File.WriteAllText(countFile, count.ToString());
    }

    static void WriteTimestamp(int count)
    {
        string timestamp = DateTime.Now.ToString("dd-MM-yyyy");
        string logEntry = $"{timestamp} - Count: {count}";
        File.AppendAllText(logFile, logEntry + Environment.NewLine);
    }

    static DateTime ReadLastRunDate()
    {
        if (File.Exists(logFile))
        {
            string[] logLines = File.ReadAllLines(logFile);
            if (logLines.Length > 0)
            {
                string lastLogLine = logLines[^1];
                string[] parts = lastLogLine.Split(" - ");
                if (parts.Length > 0 && DateTime.TryParseExact(parts[0], "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime lastRunDate))
                {
                    return lastRunDate.Date;
                }
            }
        }
        return DateTime.MinValue;
    }
}
