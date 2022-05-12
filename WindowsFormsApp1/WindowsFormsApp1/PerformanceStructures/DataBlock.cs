using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class DataBlock
    {
        public UIntPtr address;
        public List<ObjectType> objects = new List<ObjectType>();
        public string SystemName;
        public string SystemTime;
        public long DefaultObject;
        public int NumObjectTypes;
        public long PerfFreq;
        public long PerfTime;
        public long PerfTime100nSec;
        public int Revision;
        public string Signature;
        public int TotalByteLength;
        public int Version;
    }
}
