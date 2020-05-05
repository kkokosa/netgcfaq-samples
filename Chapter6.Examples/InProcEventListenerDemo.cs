using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Text;
using System.Threading;

namespace Chapter6.Examples
{
    [Description("In-process event listener")]
    class InProcEventListenerDemo
    {
        public static void Main(string[] args)
        {
            GcEventListener listener = new GcEventListener();

            Console.WriteLine("\nPress ENTER to trigger a few finalizers...");
            Console.ReadLine();

            GC.Collect(2, GCCollectionMode.Forced, true, true);

            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }
    }

    sealed class GcEventListener : EventListener
    {
        // from https://docs.microsoft.com/en-us/dotnet/framework/performance/garbage-collection-etw-events
        private const int GC_KEYWORD = 0x0000001;
        private const int TYPE_KEYWORD = 0x0080000;
        private const int GCHEAPANDTYPENAMES_KEYWORD = 0x1000000;

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "Microsoft-Windows-DotNETRuntime")
            {
                EnableEvents(eventSource,
                    EventLevel.Verbose,
                    (EventKeywords)(GC_KEYWORD | GCHEAPANDTYPENAMES_KEYWORD | TYPE_KEYWORD));
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs evt)
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {evt.EventName} received:");
            for (int i = 0; i < evt.Payload.Count; i++)
            {
                Console.WriteLine($"   {evt.PayloadNames[i]} : {evt.Payload[i] ?? "null"}");
            }
            Console.WriteLine();
        }
    }
}
