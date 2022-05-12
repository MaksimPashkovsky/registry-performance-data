using System;
using System.Text;
using System.Runtime.InteropServices;

namespace WindowsFormsApp1
{
    public class Externals
    {
        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetDataBlock(out UIntPtr DataBlockPntr);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetDataBlockInfo(
            UIntPtr DataBlockPntr,
            out long DefaultObject,
            out int NumObjectTypes,
            out long PerfFreq,
            out long PerfTime,
            out long PerfTime100nSec,
            out int Revision,
            StringBuilder Signature,
            StringBuilder SystemName,
            StringBuilder SystemTime,
            out int TotalByteLength,
            out int Version);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFirstObjectType(UIntPtr DataBlockPntr, out UIntPtr ObjTypePntr);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNextObjectType(UIntPtr p, out UIntPtr pp);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPerfObjectTypeInfo(
            UIntPtr p,
            out int ObjectNameTitleIndex,
            out int TotalByteLength,
            out int ObjectHelpTitleIndex,
            out int DetailLevel,
            out int NumCounters,
            out long DefaultCounter,
            out long NumInstances,
            out int CodePage,
            out long PerfTime,
            out long PerfFreq);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCounterDefinition(UIntPtr ObjTypePntr, out UIntPtr CounterDefPntr);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCounterBlock_Inst(UIntPtr InstDefPntr, out UIntPtr CounterBlockPntr);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCounterBlock_Obj(UIntPtr ObjTypePntr, out UIntPtr CounterBlockPntr);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFirstInstance(UIntPtr p, out UIntPtr pp);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNextInstance(UIntPtr p, out UIntPtr pp);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int GetInstanceInfo(
            UIntPtr p,
            int CodePage,
            out int ByteLength,
            out int ParentObjectTitleIndex,
            out int ParentObjectInstance,
            StringBuilder Name);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int GetCounterInfo(
            UIntPtr PerfCounterDefPntr,
            out int ByteLength,
            out int CounterHelpTitleIndex,
            out int CounterNameTitleIndex,
            out int CounterSize,
            out int CounterType,
            out long DefaultScale,
            out int DetailLevel);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int GetNextCounter(UIntPtr PerfCounterDefPntr1, out UIntPtr PerfCounterDefPntr2);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool GetCounterValue(
            UIntPtr DataBlockPntr,
            UIntPtr pObject,
            UIntPtr pCounter,
            UIntPtr pCounterBlock,
            out ulong Data,
            out long Time,
            out int MultiCounterData,
            out long Frequency);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool GetCalculatedValue(
            int CounterType0,
            ulong Data0,
            long Time0,
            int MultiCounterData0,
            long Frequency0,
            int CounterType1,
            ulong Data1,
            long Time1,
            int MultiCounterData1,
            long Frequency1,
            StringBuilder FinalValue);

        [DllImport("dll\\perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCounterType(int numbertype, StringBuilder name, StringBuilder description);
    }
}
