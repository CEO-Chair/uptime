using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace uptime;

internal partial class Program
{

    public static void Main(string[] rawArgs)
    {
        HashSet<string> args = rawArgs.Distinct().ToHashSet();

        foreach (string s in args)
        {
            switch (s)
            {
                case "-d":
                case "-a":
                    break;
                default:
                    Console.WriteLine($"Unrecognized argument \"{s}\".");
                    PrintUsage();
                    return;
            }
        }

        ulong tickCount = GetTickCount64();

        DateTime startDate = DateTime.UtcNow - TimeSpan.FromMilliseconds(tickCount);

        if (args.Contains("-a"))
        {
            if (args.Contains("-d"))
            {
                Console.WriteLine($"Arguments -a and -d may not be used simultaneously.");
                return;
            }

            (int x, int y) = Console.GetCursorPosition();

            // We start with the stopwatch not measuring time so that
            // we don't wait one second before printing the first time
            Stopwatch sw = new Stopwatch();

            while (true)
            {
                if (sw.Elapsed.TotalSeconds < 1f && sw.IsRunning)
                {
                    Thread.Sleep(70);
                    continue;
                }

                sw.Restart();

                Console.SetCursorPosition(x, y);
                Console.Write(FormatUptime(startDate, args));
            }
        }
        else
        {
            Console.WriteLine(FormatUptime(startDate, args));
        }
    }

    private static string FormatUptime(DateTime startTime, HashSet<string> args)
    {
        StringBuilder builder = new StringBuilder();

        if (args.Contains("-d"))
        {
            builder.Append($"{startTime.ToLocalTime():F}");
            return builder.ToString();
        }

        TimeSpan time = DateTime.UtcNow - startTime;

        if (time.Days > 0)
        {
            builder.Append($"{time.Days} Day{Plural(time.Days)}, ");
        }

        if (time.Hours > 0)
        {
            builder.Append($"{time.Hours} Hour{Plural(time.Hours)}, ");
        }

        if (time.Minutes > 0)
        {
            builder.Append($"{time.Minutes} Minute{Plural(time.Minutes)}, ");
        }

        builder.Append($"{time.Seconds} Second{Plural(time.Seconds)}, ");

        // Remove trailing ", "
        builder.Length -= 2;

        return builder.ToString();
    }

    private static string Plural<T>(T number) where T : INumber<T>
    {
        return number != T.One ? "s" : "";
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: uptime [options]");
        Console.WriteLine();
        Console.WriteLine("Options: ");
        Console.WriteLine($"  -d               Display date of startup");
        Console.WriteLine($"  -a               Continuously display uptime");
    }

    [LibraryImport("kernel32.dll", EntryPoint = "GetTickCount64", SetLastError = true)]
    private static partial ulong GetTickCount64();
}
