using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Analysis;
using Microsoft.Diagnostics.Tracing.Analysis.GC;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace Chapter6.Examples
{
    /*
     * Uses DiagnosticsClient for listening to EventPipes-based (filtered by some keywords)
     * and additionally uses GLAD for high-level GCStats-like analysis.
     */
    [Description("Out-of-process monitoring based on EventPipes")]
    class EventPipeProviderDemo
    {
        static void Main(string[] args)
        {
            Console.Write("Process ID to monitor: ");
            int processId = int.Parse(Console.ReadLine());
            PrintRuntimeGCEvents(processId);
        }

        static void PrintRuntimeGCEvents(int processId)
        {
            var providers = new List<EventPipeProvider>()
            {
                new EventPipeProvider("Microsoft-Windows-DotNETRuntime",
                    EventLevel.Informational, (long)ClrTraceEventParser.Keywords.GC)
            };

            var client = new DiagnosticsClient(processId);
            using (var session = client.StartEventPipeSession(providers, false))
            {
                var source = new EventPipeEventSource(session.EventStream);

                // https://devblogs.microsoft.com/dotnet/glad-part-2/
                source.NeedLoadedDotNetRuntimes();
                source.AddCallbackOnProcessStart(proc =>
                {
                    proc.AddCallbackOnDotNetRuntimeLoad(runtime =>
                    {
                        runtime.GCEnd += RuntimeOnGCEnd;
                    });
                });

                source.Clr.All += (TraceEvent obj) => { Console.WriteLine(obj.EventName); };
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
        }

        private static void RuntimeOnGCEnd(TraceProcess p, TraceGC gc)
        {
            Console.WriteLine($"{gc.GCGenerationName} {gc.Reason} {gc.PauseDurationMSec:F2}/{gc.DurationMSec:F2}");
        }
    }
}
