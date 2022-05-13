using Microsoft.Win32;
using System.Collections.Generic;

namespace WindowsFormsApp1
{
    public class Utils
    {
        public static Dictionary<int, string> GetNamesFromRegistry()
        {
            Dictionary<int, string> names = new Dictionary<int, string>();
            RegistryKey hKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Perflib\\009");
            string[] va = (string[])hKey.GetValue("CounterDefinition");
            for (int i = 0; i < va.Length - 1; i += 2)
            {
                names.Add(int.Parse(va[i]), va[i + 1]);
            }
            return names;
        }

        public static Dictionary<int, string> GetHelpsFromRegistry()
        {
            Dictionary<int, string> helps = new Dictionary<int, string>();
            RegistryKey hKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Perflib\\009");
            string[] va = (string[])hKey.GetValue("Help");
            for (int i = 0; i < va.Length - 1; i = i + 2)
            {
                helps.Add(int.Parse(va[i]), va[i + 1]);
            }
            return helps;
        }

        public static string GetDetailLevel(int num)
        {
            switch (num)
            {
                case 100:
                    return "Novice";
                case 200:
                    return "Advanced";
                case 300:
                    return "Expert";
                case 400:
                    return "Wizard";
            }
            return "Unknown";
        }

        public static string GetDataBlockPropertiesDescriptionByName(string name)
        {
            switch (name)
            {
                case "SystemName":
                    return "System name\r\n";
                case "SystemTime":
                    return "System time (UTC)\r\n" +
                        "Time at the system under measurement\r\n" +
                        "Type: SYSTEMTIME\r\n";
                case "DefaultObject":
                    return "Object Title Index of default object to " +
                        "display when data from this system is retrieved (-1 = " +
                        "none, but this is not expected to be used)\r\n" +
                        "Type: LONG\r\n";
                case "NumObjectTypes":
                    return "Number of types of objects being reported\r\n" +
                        "Type: DWORD\r\n";
                case "PerfFreq":
                    return "Performance counter frequency at the system " +
                        "under measurement\r\n" +
                        "Type: LARGE_INTEGER\r\n";
                case "PerfTime":
                    return "Performance counter value at the system " +
                        "under measurement \r\n" +
                        "Type: LARGE_INTEGER\r\n";
                case "PerfTime100nSec":
                    return "Performance counter time in 100 nsec units " +
                        "at the system under measurement\r\n" +
                        "Type: LARGE_INTEGER\r\n";
                case "Revision":
                    return "Revision of these data structures starting " +
                        "at 0 for each Version\r\n" +
                        "Type: DWORD\r\n";
                case "Signature":
                    return "Signature: Unicode \"PERF\"\r\n" +
                        "Type: WCHAR\r\n";
                case "TotalByteLength":
                    return "Total length of data block\r\n" +
                        "Type: DWORD\r\n";
                case "Version":
                    return "Version of these data structures starting at 1\r\n" +
                        "Type: DWORD\r\n";
                default:
                    return "";
            }
        }

        public static string GetObjectTypePropertiesDescriptionByName(string name)
        {
            switch (name)
            {
                case "TotalByteLength":
                    return "Length of this object definition including this " +
                        "structure, the counter definitions, and the instance definitions " +
                        "and the counter blocks for each instance: This is the offset from " +
                        "this structure to the next object, if any\r\n" +
                        "Type: DWORD\r\n";
                case "ObjectNameTitleIndex":
                    return "Index to name in Title Database\r\n" +
                        "Type: DWORD\r\n";
                case "ObjectHelpTitleIndex":
                    return "Index to Help in Title Database\r\n" +
                    "Type: DWORD\r\n";
                case "DetailLevel":
                    return "Object level of detail (for controlling display complexity); " +
                        "will be min of detail levels for all this object's counters\r\n" +
                    "Type: DWORD\r\n";
                case "NumCounters":
                    return "Number of counters in each counter block (one " +
                        "counter block per instance)\r\n" +
                    "Type: DWORD\r\n";
                case "NumInstances":
                    return "Number of object instances for which counters are " +
                        "being returned from the system under measurement. If the object defined " +
                        "will never have any instance data structures (InstanceDefinition) then " +
                        "this value should be -1, if the object can have 0 or more instances, " +
                        "but has none present, then this should be 0, otherwise this field contains " +
                        "the number of instances of this counter.\r\n" +
                        "Type: LONG\r\n";
                case "CodePage":
                    return "0 if instance strings are in UNICODE, else the Code Page " +
                        "of the instance names\r\n" +
                        "Type: DWORD\r\n";
                case "PerfTime":
                    return "Sample Time in \"Object\" units\r\n" +
                        "Type: LARGE_INTEGER\r\n";
                case "PerfFreq":
                    return "Frequency of \"Object\" units in counts per second.\r\n" +
                        "Type: LARGE_INTEGER\r\n";
                default:
                    return "";
            }
        }
    }
}
