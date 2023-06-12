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
                case "-c":
                case "-a":
                    break;
                default:
                    Console.WriteLine($"Unrecognized argument \"{s}\".");
                    PrintUsage();
                    return;
            }
        }

        if (args.Contains("-a"))
        {
            if (args.Contains("-d"))
            {
                Console.WriteLine($"Arguments -a and -d may not be used simultaneously.");
                return;
            }

            (int x, int y) = Console.GetCursorPosition();

            ulong minDiff = 1000ul;

            ulong lastTickCount = 0;
            while (true)
            {
                ulong tickCount = GetTickCount64();
                if (tickCount - lastTickCount < minDiff)
                {
                    Thread.Sleep(50);
                    continue;
                }

                lastTickCount = tickCount;

                Console.SetCursorPosition(x, y);
                Console.WriteLine(FormatUptime(tickCount, args));
            }
        }
        else
        {
            Console.WriteLine(FormatUptime(GetTickCount64(), args));
        }
    }

    private static string FormatUptime(ulong tickCount, HashSet<string> args)
    {
        TimeSpan time = TimeSpan.FromMilliseconds(tickCount);

        StringBuilder output = new StringBuilder();

        if (args.Contains("-d"))
        {
            DateTime date = DateTime.Now - time;

            if (args.Contains("-c"))
            {
                output.Append($"{date:G}");
            }
            else
            {
                output.Append($"{date:F}");
            }
        }
        else
        {
            if (args.Contains("-c"))
            {

                output.Append($"{time:c}");
            }
            else
            {
                if (time.Days >= 1)
                {
                    output.Append($"{time.Days} Day{Plural(time.Days)}");
                }

                if (time.Days >= 7)
                {
                    int weeks = time.Days / 7;
                    int daysInToWeek = time.Days % 7;
                    output.Append($" ({weeks} Week{Plural(weeks)} and {daysInToWeek} Day{Plural(daysInToWeek)})");
                }

                if (time.Hours > 0)
                {
                    output.Append($", {time.Hours} Hour{Plural(time.Hours)}");
                }

                if (time.Minutes > 0)
                {
                    output.Append($", {time.Minutes} Minute{Plural(time.Minutes)}");
                }

                if (time.Seconds > 0)
                {
                    output.Append($", {time.Seconds} Second{Plural(time.Seconds)}");
                }
            }
        }

        return output.ToString();
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
        Console.WriteLine($"  -c               Display more compact date or time information");
        Console.WriteLine($"  -a               Continuously display uptime");
    }

    [LibraryImport("kernel32.dll", EntryPoint = "GetTickCount64", SetLastError = true)]
    private static partial ulong GetTickCount64();
}
