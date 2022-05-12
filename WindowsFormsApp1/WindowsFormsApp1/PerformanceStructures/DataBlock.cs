using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsFormsApp1
{
    public class DataBlock
    {
        public readonly UIntPtr Address;
        public List<ObjectType> Objects;
        public readonly string SystemName;
        public readonly string SystemTime;
        public readonly long DefaultObject;
        public readonly int NumObjectTypes;
        public readonly long PerfFreq;
        public readonly long PerfTime;
        public readonly long PerfTime100nSec;
        public readonly int Revision;
        public readonly string Signature;
        public readonly int TotalByteLength;
        public readonly int Version;
        
        public DataBlock(UIntPtr address, 
            string systemName, 
            string systemTime, 
            long defaultObject, 
            int numObjectTypes, 
            long perfFreq, 
            long perfTime, 
            long perfTime100nSec, 
            int revision, 
            string signature, 
            int totalByteLength, 
            int version)
        {
            Objects = new List<ObjectType>();
            Address = address;
            SystemName = systemName;
            SystemTime = systemTime;
            DefaultObject = defaultObject;
            NumObjectTypes = numObjectTypes;
            PerfFreq = perfFreq;
            PerfTime = perfTime;
            PerfTime100nSec = perfTime100nSec;
            Revision = revision;
            Signature = signature;
            TotalByteLength = totalByteLength;
            Version = version;
        }

        public static DataBlock GetFromPointer(UIntPtr PerfDataBlockPntr)
        {
            StringBuilder Signature = new StringBuilder(5);
            StringBuilder SystemName = new StringBuilder(20);
            StringBuilder SystemTime = new StringBuilder(100);

            Externals.GetDataBlockInfo(PerfDataBlockPntr,
                out long DefaultObject,
                out int NumObjectTypes,
                out long PerfFreq,
                out long PerfTime,
                out long PerfTime100nSec,
                out int Revision,
                Signature,
                SystemName,
                SystemTime,
                out int TotalByteLength,
                out int Version);

            return new DataBlock(
                systemName: SystemName.ToString(),
                systemTime: SystemTime.ToString(),
                defaultObject: DefaultObject,
                numObjectTypes: NumObjectTypes,
                perfFreq: PerfFreq,
                perfTime: PerfTime,
                perfTime100nSec: PerfTime100nSec,
                revision: Revision,
                signature: Signature.ToString(),
                totalByteLength: TotalByteLength,
                version: Version,
                address: PerfDataBlockPntr);
        }

    }
}
