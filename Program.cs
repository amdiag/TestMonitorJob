using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;


class TimerMonitor
{
    private const int MillsInSec = 1000;
    static void Main(string[] args)
    {
        int killTime = 0;
        int refreshTime = 0;
        if (args.Length != 3)
        {
           PrintUsage();
           return; 
        }
        if ((!string.IsNullOrEmpty(args[0])) &&
            int.TryParse(args[1], out killTime) &&
            int.TryParse(args[2], out refreshTime))
        {
            var autoEvent = new AutoResetEvent(false);
            var statusChecker = new StatusChecker(args[0], killTime);
            using (var stateTimer = new Timer(statusChecker.StartProcessMonitor, autoEvent, 0, refreshTime * MillsInSec))
            {
                autoEvent.WaitOne();
            }
        }
        else
        {
            Console.WriteLine(args[0] + " " + killTime + " " + refreshTime);
            PrintUsage();
            return;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: monitor <process name> <time limit> <refresh time>.");
    }
}


class StatusChecker
{
    private readonly string _processName;
    private readonly int _killTime;

    public StatusChecker(string name, int time)
    {
        _processName = name;
        _killTime = time;
    }

    public void StartProcessMonitor(object stateInfo)
    {
        var processesToKill = FindProcess(Process.GetProcessesByName(_processName));
        if (processesToKill.Any())
        {
            foreach (Process proc in processesToKill)
            {
                KillProcess(proc);
            }
        }
    }

    private Process[] FindProcess(Process[] processes)
    {
        if (processes.Any())
        {
            return processes.Where(x => (DateTime.Now - x.StartTime).TotalMinutes > _killTime).ToArray();
        }
        return new Process[0];
    }

    private void KillProcess(Process process)
    {
        try
        {
            process.Kill();
            Console.WriteLine($"Process killed. Info: {process.Id} {process.ProcessName}. Started time {process.StartTime.ToLongDateString()}.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured. Info: {process.Id} {process.ProcessName} {Environment.NewLine} {e.Message}.");
        }
    }
}