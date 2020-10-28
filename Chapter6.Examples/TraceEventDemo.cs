using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Analysis;
using Microsoft.Diagnostics.Tracing.Analysis.GC;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;

namespace Chapter6.Examples
{
    /*
     * Uses TraceEventSession for listening to ETW-based (filtered by some keywords)
     * and additionally uses GLAD for high-level GCStats-like analysis.
     */
    [Description("System-wide events listener")]
    class TraceEventDemo
    {
        public static void Main(string[] args)
        {
            string sessionName = "EtwSessionForCLR_" + Guid.NewGuid().ToString();
            Console.WriteLine($"Starting {sessionName}...\r\n");
            using (TraceEventSession userSession = new TraceEventSession(sessionName, TraceEventSessionOptions.Create))
            {
                Task.Run(() =>
                {
                    userSession.EnableProvider(
                        ClrTraceEventParser.ProviderGuid,
                        TraceEventLevel.Verbose,
                        (ulong)(
                            ClrTraceEventParser.Keywords.Contention |  
                            ClrTraceEventParser.Keywords.Threading |   
                            ClrTraceEventParser.Keywords.Exception |   
                            ClrTraceEventParser.Keywords.GCHeapAndTypeNames |
                            ClrTraceEventParser.Keywords.Type | 
                            ClrTraceEventParser.Keywords.GC     
                        )
                    );

                    // This is GLAD: https://devblogs.microsoft.com/dotnet/glad-part-2/
                    userSession.Source.NeedLoadedDotNetRuntimes();
                    userSession.Source.AddCallbackOnProcessStart(proc =>
                    {
                        proc.AddCallbackOnDotNetRuntimeLoad(runtime =>
                        {
                            runtime.GCEnd += RuntimeOnGCEnd;
                        });
                    });
                    // End of GLAD

                    userSession.Source.Clr.GCStart += ClrOnGCStart;
                    userSession.Source.Clr.GCStop += ClrOnGCStop;

                    // this is a blocking call until the session is disposed
                    userSession.Source.Process();
                    Console.WriteLine("End of session");
                });
                GC.Collect();
                // wait for the user to dismiss the session
                Console.WriteLine("Presse ENTER to exit...");
                Console.ReadLine();
            }
        }

        private static void RuntimeOnGCEnd(TraceProcess tp, TraceGC gc)
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] GLAD received from {tp.Name}/{tp.ProcessID}");
            Console.WriteLine($"   {gc.GCGenerationName} {gc.Reason} Pause: {gc.PauseDurationMSec:F2} ms/{gc.DurationMSec:F2} ms");
            Console.WriteLine();
        }

        private static void ClrOnGCStop(GCEndTraceData evt)
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] GCStop received ({evt.ProcessName}/{evt.ProcessID}):");
            for (int i = 0; i < evt.PayloadNames.Length; i++)
            {
                Console.WriteLine($"   {evt.PayloadNames[i]} : {evt.PayloadValue(i) ?? "null"}");
            }
            Console.WriteLine();
        }

        private static void ClrOnGCStart(GCStartTraceData evt)
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] GCStart received:");
            for (int i = 0; i < evt.PayloadNames.Length; i++)
            {
                Console.WriteLine($"   {evt.PayloadNames[i]} : {evt.PayloadValue(i) ?? "null"}");
            }
            Console.WriteLine();
        }
    }
}
