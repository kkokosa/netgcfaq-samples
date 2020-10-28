using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Analysis;
using Microsoft.Diagnostics.Tracing.Analysis.GC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Chapter6.Examples
{
    /*
     * Uses EventPipeEventSource for analyzing EventPipes-based trace file
     * and additionally uses GLAD for high-level GCStats-like analysis.
     */
    [Description("Nettrace file-based monitoring (based on EventPipes)")]
    class NetTraceProviderDemo
    {
        static void Main(string[] args)
        {
            Console.Write("Collect a new trace by 'dotnet trace collect --profile gc-collect -p <PID>'");
            Console.Write("Nettrace file to analyze: ");
            string file = Console.ReadLine();
            PrintRuntimeGCEvents(file);
        }

        private static void PrintRuntimeGCEvents(string file)
        {
            var source = new EventPipeEventSource(file);

            // https://devblogs.microsoft.com/dotnet/glad-part-2/
            source.NeedLoadedDotNetRuntimes();
            source.AddCallbackOnProcessStart(proc =>
            {
                proc.AddCallbackOnDotNetRuntimeLoad(runtime =>
                {
                    runtime.GCEnd += RuntimeOnGCEnd;
                });
            });

            //source.Clr.All += (TraceEvent obj) => { Console.WriteLine(obj.EventName); };
            try
            {
                source.Process();
            }
            // NOTE: This exception does not currently exist. It is something that needs to be added to TraceEvent.
            catch (Exception e)
            {
                Console.WriteLine("Error encountered while processing events");
                Console.WriteLine(e.ToString());
            }
        }

        private static void RuntimeOnGCEnd(TraceProcess p, TraceGC gc)
        {
            Console.WriteLine($"{gc.GCGenerationName} {gc.Reason} {gc.PauseDurationMSec:F2}/{gc.DurationMSec:F2}");
        }
    }
}
