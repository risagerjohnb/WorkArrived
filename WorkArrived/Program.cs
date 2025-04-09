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
            int count = ReadCount();
            count++;
            WriteCount(count);
            WriteTimestamp(count);
            Console.WriteLine($"Work wifi detected! Count increased to: {count}");
        }
        else
        {
            Console.WriteLine("Not connected to work wifi.");
        }
    }

    static string GetConnectedWifi()
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
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logEntry = $"{timestamp} - Count: {count}";
        File.AppendAllText(logFile, logEntry + Environment.NewLine);
    }
}