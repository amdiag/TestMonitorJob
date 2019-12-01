using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading;


class TimerMonitor
{
    private const int millsInSec = 1000;
    static void Main(string[] args)
    {
        int KillTime = new int();
        int RefreshTime = new int();
        if (args.Length != 3)
        {
           printUsage();
           return; 
        }
        if ( (!string.IsNullOrEmpty(args[0])) && 
            int.TryParse(args[1], out KillTime) &&            
            int.TryParse(args[2], out RefreshTime))
        {            
            var autoEvent = new AutoResetEvent(false);
            //
            var statusChecker = new StatusChecker( args[0], KillTime);
            var stateTimer = new Timer(statusChecker.StartProcessMonitor,
                                       autoEvent, 0, RefreshTime* millsInSec);
            autoEvent.WaitOne();
        }
        else{
            Console.WriteLine(args[0]+" "+KillTime+" "+RefreshTime);
            printUsage();
            return;
        }
    }

    private static void printUsage()
    {
        Console.WriteLine("Usage: monitor <process name> <time limit> <refresh time>");
    }
}


class StatusChecker
{
    private string processName;
    private int KillTime;

    public StatusChecker(string name, int time)
    {
        processName = name;
        KillTime = time;
    }

    public void StartProcessMonitor(Object stateInfo)
    {
        ArrayList listToKill = FindProcess(Process.GetProcessesByName(processName));
        if (listToKill.Count != 0)
        {
            foreach (Process proc in listToKill)
            {
                this.KillProcess(proc);
            }

        }
    }

    private ArrayList FindProcess(Process[] listProcesses)
    {
        ArrayList listToKill = new ArrayList();
        if (listProcesses.Any())
        {
            foreach (Process proc in listProcesses)
            {
                DateTime ProcessTime = proc.StartTime;
                TimeSpan duration = DateTime.Now - ProcessTime;

                if (duration.TotalMinutes > KillTime)
                    listToKill.Add(proc);
            }
        }
        return listToKill;
    }

    private bool KillProcess(Process process)
    {
        process.Kill();
        while (!process.HasExited) { }
        Console.WriteLine("Process killed. Info: "+process.Id+" "+process.ProcessName+". Started time "+process.StartTime.ToString());
        return true;
    }
}