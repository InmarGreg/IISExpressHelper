﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IISExpressHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            const string PID_FILE = "pid.txt";

            if (File.Exists(PID_FILE))
            {
                try
                {
                    int pid = Convert.ToInt32(File.ReadAllText(PID_FILE));

                    using (Process process = Process.GetProcessById(pid))
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                }
                catch
                {
                    // PID may be stale or garbage - who cares, catch and continue.
                }

                File.Delete(PID_FILE);
            }

            if (args.Count() > 0 && File.Exists(args[0])) // assume args[0] == csproj of web app
            {
                XElement element = XDocument.Load(args[0])
                                            .Descendants()
                                            .Where(x => string.Compare(x.Name.LocalName, "DevelopmentServerPort", StringComparison.CurrentCultureIgnoreCase) == 0)
                                            .FirstOrDefault();

                if (element != null)
                {
                    ProcessStartInfo info = new ProcessStartInfo();
                    info.CreateNoWindow = true;
                    info.UseShellExecute = false;
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    info.FileName = "C:\\Program Files (x86)\\IIS Express\\iisexpress.exe";
                    info.Arguments = string.Format("/systray:false /path:\"{0}\" /port:{1}", Path.GetDirectoryName(args[0]), element.Value);

                    using (Process process = new Process())
                    {
                        process.StartInfo = info;
                        process.Start();

                        File.WriteAllText(PID_FILE, Convert.ToString(process.Id));
                    }
                }
            }
        }
    }
}
