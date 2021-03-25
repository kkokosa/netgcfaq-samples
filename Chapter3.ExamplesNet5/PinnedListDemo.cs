using System;
using System.ComponentModel;
using System.Text;

namespace Chapter3.Examples
{
    [Description("Growing list of pinned objects")]
    class PinnedListDemo
    {
        unsafe public static void Main(string[] args)
        {
            var pinnedArray = GC.AllocateArray<byte>(128, pinned: true);
            fixed (byte* ptr = pinnedArray)
            {
                // This is no-op 'pinning' as it does not influence the GC 
            }
        }
    }
}
