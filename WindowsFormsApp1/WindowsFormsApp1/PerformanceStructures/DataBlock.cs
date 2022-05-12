using System;
using System.Collections.Generic;

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
    }
}
