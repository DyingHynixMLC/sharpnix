using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;

namespace ConsoleApp3
{
/* Example Sharpnix.cfg file
// Sharpie < This is the username
// sharpie < This is the hostname
// UTC+2 < This is the timezone, Sharpie lives in Lofoten, so she uses UTC+2.
// RCM < This is where the recovery mode tag goes. It will be whitespace by default.
// 0-100 < This is sharpie's hunger level, however it is currently unused.
*/
    internal class Program
    {
        public static string shellPath = "/";
        public static string build = "0.1.0";
        public static string SharpnixCFG = @".\Sharpnix\Sharpnix.ini";
        public static string userName = "root";
        public static string hostName = "root";
        public static string UTCOffset = "UTC+0";
        public static bool isRCM = false;
        static string? ReadLineFromFile(string filePath, int lineNumber)
        {
            using StreamReader reader = new StreamReader(filePath);
            for (int i = 1; i < lineNumber; i++)
                if (reader.ReadLine() == null) return null;

            return reader.ReadLine();
        }

        static DateTime ApplyUTCOffset(string timeStr, string utcOffsetStr)
        {
            if (!DateTime.TryParse(timeStr, out DateTime baseTime))
            {
                Console.WriteLine("Invalid time format.");
            }
            if (!utcOffsetStr.StartsWith("UTC") || utcOffsetStr.Length < 5)
            {
                Console.WriteLine("Invalid UTC Offset format.");
            }
            string offsetPart = utcOffsetStr.Substring(3);
            if (!int.TryParse(offsetPart, out int hoursOffset))
            {
                Console.WriteLine("Invalid UTC Offset value.");
            }
            try
            {
                return baseTime.AddHours(hoursOffset);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UTC offset out of bounds. Returning default.");
                return baseTime;
            }
        }
        static void DateTimeNow()
        {
            string TimeNow = DateTime.UtcNow.ToString();
            DateTime offset = ApplyUTCOffset(TimeNow, UTCOffset);
            Console.WriteLine(offset);
        }
        public static string path => TranslatePath(shellPath);

        static string TranslatePath(string unixPath)
        {
            string basePath = @".\Sharpnix";
            if (unixPath == "/")
                return basePath;

            string dosPath = basePath + unixPath.Replace('/', '\\');
            return dosPath;
        }

        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--sudo")
            {
                userName = "root";
            }
            else
            {
                userName = ReadLineFromFile(SharpnixCFG, 1);
            }
            Console.WriteLine(@"
   _____ _                            _      
  / ____| |                          (_)     
 | (___ | |__   __ _ _ __ _ __  _ __  ___  __
  \___ \| '_ \ / _` | '__| '_ \| '_ \| \ \/ /
  ____) | | | | (_| | |  | |_) | | | | |>  < 
 |_____/|_| |_|\__,_|_|  | .__/|_| |_|_/_/\_\
                         | |                 
                         |_|                 
");

            Console.WriteLine("Starting Sharpnix build " + build);
            Thread.Sleep(1000);

            if (!File.Exists(SharpnixCFG))
            {
                oobe();
            }
            hostName = ReadLineFromFile(SharpnixCFG, 2);
            UTCOffset = ReadLineFromFile(SharpnixCFG, 3);

            string RCM = ReadLineFromFile(SharpnixCFG, 4);
            isRCM = RCM == "RCM";

            Console.WriteLine("Sharpnix initialized.");
            PostStart();
        }

        static void oobe()
        {
            Directory.CreateDirectory(@".\Sharpnix");
            File.Create(SharpnixCFG).Close();
            int sharpieTries = 0;
            Console.WriteLine("       /\\_/\\  \r\n      ( o.o ) \r\n       > ^ <");
            Console.WriteLine("Hi! I'm Sharpie. Let's get started configuring Sharpnix for you!");
            Thread.Sleep(1250);
            Console.WriteLine("Alrighty, let's get you started. First off I need your hostname.");
            hostName = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(hostName))
            {
                Console.WriteLine("Hm, that doesn't seem quite right. Try making it again!");
                hostName = Console.ReadLine();
            }
            while (hostName?.ToLower() == "sharpie")
            {
                if (sharpieTries <= 2)
                {
                    Console.WriteLine("What the hell? No.. You can't be me!");
                }
                else if (sharpieTries == 3)
                {
                    Console.WriteLine("Stop it! You can't be me!!");
                }
                else
                {
                    Console.WriteLine("Seriously! Stop!");
                }
                hostName = Console.ReadLine();
                sharpieTries++;

                if (sharpieTries >= 5)
                {
                    Console.Clear();
                    Console.WriteLine("       /\\_/\\  \r\n      ( ò.ó ) \r\n       > ^ <");
                    Console.WriteLine("You know what? Enough.");
                    Thread.Sleep(4000);
                    Console.WriteLine("byebye!");
                    Thread.Sleep(1000);
                    Environment.Exit(sharpieTries);
                }
            } // little easter egg lol, can be circumvented by manually editing sharpnix.ini

            Thread.Sleep(1250);
            Console.WriteLine(hostName + "? Excellent choice!");
            Console.WriteLine("Let's continue, shall we?");
            Console.WriteLine("Alrighty, time for your username, i will refer to you by that!");
            userName = Console.ReadLine();
            Console.WriteLine("Okay, let's get your timezone!");
            UTCOffset = Console.ReadLine();
            Console.WriteLine("Great! We're done now. Enjoy!");
            Console.Clear();
            using StreamWriter streamWriter = new StreamWriter(SharpnixCFG);
            string config = $"{userName}\n{hostName}\n{UTCOffset}\n\n100";
            streamWriter.Write(config);
        }
        static void PostStart()
        {
            Console.Write($"{userName}@{hostName}-Sharpnix:{shellPath}$ ");
            string cmd = Console.ReadLine();
            if (cmd != null)
            {
                Execute(cmd);
            }
        }

        static void Execute(string cmd)
        {
            string outputFile = null;
            if (cmd.Contains('>'))
            {
                string[] redirParts = cmd.Split('>', 2, StringSplitOptions.RemoveEmptyEntries);
                cmd = redirParts[0].Trim();
                outputFile = redirParts[1].Trim();
            }

            string[] parts = cmd.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                PostStart();
                return;
            }

            string command = parts[0];
            string argument = parts.Length > 1 ? parts[1] : null;

            StringWriter outputWriter = outputFile != null ? new StringWriter() : null;
            TextWriter originalOut = Console.Out;
            if (outputWriter != null)
                Console.SetOut(outputWriter);

            switch (command)
            {
                case "help":
                    Console.WriteLine("Commands: help, cd, ls, mkdir, touch, clear, exit, shutdown");
                    break;

                case "cd":
                    {
                        string target = string.IsNullOrWhiteSpace(argument) ? "/" : argument;

                        string nextShellPath = target.StartsWith("/")
                            ? target
                            : Path.Combine(shellPath, target).Replace('\\', '/');

                        if (!nextShellPath.StartsWith("/"))
                            nextShellPath = "/" + nextShellPath;

                        string translated = TranslatePath(nextShellPath);
                        if (Directory.Exists(translated))
                        {
                            shellPath = nextShellPath;
                        }
                        else
                        {
                            Console.WriteLine($"Sharpnix: cd: {argument}: No such file or directory");
                        }
                        break;
                    }

                case "mkdir":
                    if (!string.IsNullOrWhiteSpace(argument))
                    {
                        Directory.CreateDirectory(Path.Combine(path, argument));
                    }
                    break;

                case "touch":
                    if (!string.IsNullOrWhiteSpace(argument))
                    {
                        File.Create(Path.Combine(path, argument)).Close();
                    }
                    break;

                case "ls":
                    {
                        if (Directory.Exists(path))
                        {
                            string[] files = Directory.GetFiles(path);
                            string[] dirs = Directory.GetDirectories(path);

                            if (dirs.Length > 0)
                            {
                                Console.WriteLine("\n-- Directories:");
                                foreach (var dir in dirs)
                                {
                                    Console.Write(Path.GetFileName(dir) + " ");
                                }
                            }

                            if (files.Length > 0)
                            {
                                Console.WriteLine("\n-- Files:");
                                foreach (var file in files)
                                {
                                    Console.Write(Path.GetFileName(file) + " ");
                                }
                                Console.Write("\n");
                            }
                        }
                        else
                        {
                            Console.WriteLine("ls: No such file or directory");
                        }
                        break;
                    }

                case "clear":
                    Console.Clear();
                    break;

                case "oobe":
                    oobe();
                    break;

                case "time":
                    if (!string.IsNullOrWhiteSpace(argument))
                    {
                        if (argument.Contains("UTC+") || argument.Contains("UTC-"))
                        {
                            UTCOffset = argument;
                            DateTimeNow();
                            UTCOffset = "UTC+0";
                        }
                    }
                    else
                    {
                        DateTimeNow();
                    }
                    break;

                case "cat":
                    if (!string.IsNullOrWhiteSpace(argument))
                    {
                        cat(argument);
                    }
                    break;

                case "shutdown":
                case "exit":
                    Environment.Exit(0);
                    break;

                case "reboot":
                    Reboot(false);
                    break;
                case "sudo":
                    Reboot(true);
                    break;
                case "neofetch":
                    Neofetch();
                    break;
                case "rm":
                    if (File.Exists(argument))
                    {
                        File.Delete(argument);
                    }
                    else if (Directory.Exists(argument))
                    {
                        Directory.Delete(argument, true);
                    }
                    else
                    {
                        Console.WriteLine($"No such file or directory: {argument}");
                    }
                    break;
                case "su":
                    if (string.IsNullOrWhiteSpace (argument))
                    {
                        Console.WriteLine("No user specified.");
                    }
                    else if (argument == "root")
                    {
                        Reboot(true);
                    }
                    else
                    {
                        userName = argument;
                    }
                    break;
                case "kysnowfag":
                    bsod();
                    break;
                default:
                    Console.WriteLine($"Sharpnix: {cmd}: command not found");
                    break;
            }

            if (outputWriter != null)
            {
                Console.SetOut(originalOut);
                string outputPath = Path.Combine(path, outputFile);
                try
                {
                    File.WriteAllText(outputPath, outputWriter.ToString());
                }
                catch
                {
                    Console.WriteLine($"Sharpnix: Unable to write to {outputFile}");
                }
            }

            PostStart();
        }

        static void cat(string file)
        {
            string output;
            try
            {
                StreamReader sr = new StreamReader(path + "\\" + file);
                while ((output = sr.ReadLine()) != null)
                {
                    Console.WriteLine(output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"cat: {file}: No such file or directory");
            }
        }
        
        static void Neofetch()
{
    string[] logo = new[]
    {
    "         ",
    "         ",  
    " ██  ██  ",
    "████████ ",
    " ██  ██  ",
    "████████ ",
    " ██  ██  ",
    "         ",
    "         "
};

    ConsoleColor[] rainbow = new[]
    {
    ConsoleColor.Red,
    ConsoleColor.Yellow,
    ConsoleColor.Green,
    ConsoleColor.Cyan,
    ConsoleColor.Blue,
    ConsoleColor.Magenta
};

    long usedBytes = GC.GetTotalMemory(false);
    long maxBytes = Environment.Is64BitProcess ? 4L * 1024 * 1024 * 1024 : 2L * 1024 * 1024 * 1024;

    int usedMB = (int)(usedBytes / (1024 * 1024));
    int maxMB = (int)(maxBytes / (1024 * 1024 / 4));

    int bars = 10;
    int filled = (int)((double)usedMB / maxMB * bars);
    string memBar = "[" + new string('#', filled) + new string('-', bars - filled) + "]";

    string[] info = new[]
    {
    $"{userName}@{hostName}-Sharpnix",
    "---------------------",
    $"OS: Sharpnix CLI",
    $"Host: {hostName} {build}",
    $"Uptime: {Environment.TickCount / 1000}s",
    $"Theme: default",
    $"Packages: 1 (default)",
    $"Terminal: /dev/shell1",
    $"Shell: Sharpnix_{build}",
    $"Memory:   {memBar}  {usedMB}MB / {maxMB}MB",
};

    int lines = Math.Max(logo.Length, info.Length);
    for (int i = 0; i < lines; i++)
    {
        if (i < logo.Length)
        {
            Console.ForegroundColor = rainbow[i % rainbow.Length];
            Console.Write(logo[i]);
        }
        else
        {
            Console.Write(new string(' ', logo[0].Length));
        }

        Console.ResetColor();
        Console.Write("   ");

        if (i < info.Length)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(info[i]);
        }
        else
        {
            Console.WriteLine();
        }
    }

    Console.ResetColor();
}
        static void Reboot(bool restartAdmin)
        {
            if (!restartAdmin)
            {
                Console.Clear();
                Console.WriteLine($"Broadcast message from {userName}@{hostName}-Sharpnix on pts/1");
                Console.WriteLine();
                Console.WriteLine("The system will reboot NOW!");
                Thread.Sleep(2500);
                Console.Clear();
                Process.Start(Environment.ProcessPath);
                Environment.Exit(0);
            }
            else { RestartAsAdmin(); }
        }
static bool IsAdministrator()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

        static void RestartAsAdmin()
        {
            var psi = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas",
                Arguments = "--sudo"
            };

            Process.Start(psi);
            Environment.Exit(0);
        }
        static void bsod()
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\r\n        __\r\n   _   / /\r\n  (_) | | \r\n   _  | | \r\n  (_) | | \r\n       \\_\\");
            string[] lines =
            {
        "      ",
        "      ",
        "      ",
        " Sharpnix crashed and needs to restart.",
        "",
        " Press any key to reboot...",
        ""
    };

            int width = Console.WindowWidth;
            int height = Console.WindowHeight;

            int top = (height - lines.Length) / 2;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int left = (width - line.Length) / 2;
                Console.SetCursorPosition(Math.Max(0, left), top + i);
                Console.Write(line);
            }

            Console.ResetColor();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ReadKey(true);
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("An error occurred. Code: \"sysrb\". The system will now reboot.");
            Thread.Sleep(150);
            Console.Clear();
            Process.Start(Environment.ProcessPath);
            Environment.Exit(0);
        }





    }


}
