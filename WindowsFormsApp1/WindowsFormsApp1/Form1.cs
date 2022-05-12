using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetDataBlock(out UIntPtr DataBlockPntr);

        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetDataBlockInfo(UIntPtr DataBlockPntr,
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
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFirstObjectType(UIntPtr DataBlockPntr, out UIntPtr ObjTypePntr);

        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNextObjectType(UIntPtr p, out UIntPtr pp);
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPerfObjectTypeInfo( UIntPtr p,
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
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCounterDefinition(UIntPtr ObjTypePntr, out UIntPtr CounterDefPntr);
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCounterBlock_Inst(UIntPtr InstDefPntr, out UIntPtr CounterBlockPntr);
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCounterBlock_Obj(UIntPtr ObjTypePntr, out UIntPtr CounterBlockPntr);

        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFirstInstance(UIntPtr p, out UIntPtr pp);
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNextInstance(UIntPtr p, out UIntPtr pp);
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        //[HandleProcessCorruptedStateExceptions]
        public static extern int GetInstanceInfo(   UIntPtr p,
                                                    int CodePage,
                                                    out int ByteLength,
                                                    out int ParentObjectTitleIndex,
                                                    out int ParentObjectInstance,
                                                    StringBuilder Name);
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int GetCounterInfo(UIntPtr PerfCounterDefPntr,
                                                out int ByteLength,
                                                out int CounterHelpTitleIndex,
                                                out int CounterNameTitleIndex,
                                                out int CounterSize,
                                                out int CounterType,
                                                out long DefaultScale,
                                                out int DetailLevel);
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int GetNextCounter(UIntPtr PerfCounterDefPntr1, out UIntPtr PerfCounterDefPntr2);
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool GetCounterValue(UIntPtr DataBlockPntr,
                                                    UIntPtr pObject,
                                                    UIntPtr pCounter,
                                                    UIntPtr pCounterBlock,
                                                    out ulong Data,
                                                    out long Time,
                                                    out int MultiCounterData,
                                                    out long Frequency);
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool GetCalculatedValue(int CounterType0,
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
        
        [DllImport("perf_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCounterType(int numbertype, StringBuilder name, StringBuilder description);


        public DataBlock PerfDataBlock1 = new DataBlock();
        public DataBlock PerfDataBlock2 = new DataBlock();

        Dictionary<int, string> names = new Dictionary<int, string>();
        Dictionary<int, string> helps = new Dictionary<int, string>();
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ImageList il = new ImageList();
            il.Images.Add(Image.FromFile("open.png"));
            il.Images.Add(Image.FromFile("data.png"));
            il.Images.Add(Image.FromFile("key.jpg"));
            il.Images.Add(Image.FromFile("file.png"));
            treeView1.ImageList = il;

            ImageList il2 = new ImageList();
            il2.Images.Add(Image.FromFile("param.png"));
            il2.Images.Add(Image.FromFile("counter.png"));
            listView1.SmallImageList = il2;

            FillNames(names);

            FillHelps(helps);

            GetPerformanceData(PerfDataBlock1);
            GetPerformanceData(PerfDataBlock2);

            CalculateCounterValues(PerfDataBlock1, PerfDataBlock2);

            treeView1.Nodes.Add("DataBlock", "DataBlock", 1);
            
            foreach (ObjectType obj in PerfDataBlock1.objects)
            {
                TreeNode objNode = new TreeNode();
                int index = obj.ObjectNameTitleIndex;
                if (names.ContainsKey(index))
                {
                    string name = names[index];
                    objNode.Text = name;
                }
                else
                {
                    objNode.Text = index.ToString();
                }
                
                if (obj.instances.Count > 0)
                {
                    foreach (InstanceDefinition inst in obj.instances)
                    {
                        objNode.Nodes.Add(inst.Name, inst.Name, 3);
                    }
                }


                objNode.ImageIndex = 2;
                treeView1.Nodes[0].Nodes.Add(objNode);
            }

        }

        private void CalculateCounterValues(DataBlock PerfDataBlock1, DataBlock PerfDataBlock2)
        {
            StringBuilder Value = new StringBuilder(255);

            foreach (ObjectType obj in PerfDataBlock1.objects)
            {
                foreach (InstanceDefinition inst in obj.instances)
                {
                    foreach (CounterDefinition counter in inst.counters)
                    {
                        CounterDefinition counter2 = GetCounter(PerfDataBlock2, counter.CounterNameTitleIndex, inst.Name, obj.ObjectNameTitleIndex);
                        RAW_DATA rd = counter2.RawData;
                        if (counter2 != null)
                        {
                            bool b = GetCalculatedValue(counter.CounterType,
                                                        counter.RawData.Data,
                                                        counter.RawData.Time,
                                                        counter.RawData.MultiCounterData,
                                                        counter.RawData.Frequency,
                                                        counter2.CounterType,
                                                        rd.Data,
                                                        rd.Time,
                                                        rd.MultiCounterData,
                                                        rd.Frequency,
                                                        Value);
                        }
                        else
                        {
                            bool b = GetCalculatedValue(counter.CounterType,
                                                        counter.RawData.Data,
                                                        counter.RawData.Time,
                                                        counter.RawData.MultiCounterData,
                                                        counter.RawData.Frequency,
                                                        0,
                                                        0,
                                                        0,
                                                        0,
                                                        0,
                                                        Value);
                        }
                        counter.VALUE = Value.ToString();
                        Value.Clear();
                    }

                }
                
                foreach (CounterDefinition counter in obj.counters)
                {
                    CounterDefinition counter2 = GetCounter(PerfDataBlock2, counter.CounterNameTitleIndex, obj.ObjectNameTitleIndex);
                    RAW_DATA rd = counter2.RawData;
                    if (counter2 != null)
                    {
                        bool b = GetCalculatedValue(counter.CounterType,
                                                    counter.RawData.Data,
                                                    counter.RawData.Time,
                                                    counter.RawData.MultiCounterData,
                                                    counter.RawData.Frequency,
                                                    counter2.CounterType,
                                                    rd.Data,
                                                    rd.Time,
                                                    rd.MultiCounterData,
                                                    rd.Frequency,
                                                    Value);
                    }
                    else
                    {
                        bool b = GetCalculatedValue(counter.CounterType,
                                                    counter.RawData.Data,
                                                    counter.RawData.Time,
                                                    counter.RawData.MultiCounterData,
                                                    counter.RawData.Frequency,
                                                    0,
                                                    0,
                                                    0,
                                                    0,
                                                    0,
                                                    Value);
                    }
                    counter.VALUE = Value.ToString();
                    Value.Clear();
                }

            }
        }

        private CounterDefinition GetCounter(DataBlock PerfDataBlock, int counterIndex, string InstanceName, int ObjectIndex)
        {
            foreach (ObjectType obj in PerfDataBlock.objects)
            {
                if (obj.ObjectNameTitleIndex == ObjectIndex)
                {
                    foreach (InstanceDefinition inst in obj.instances)
                    {
                        if (inst.Name.Equals(InstanceName))
                        {
                            foreach (CounterDefinition counter in inst.counters)
                            {
                                if (counter.CounterNameTitleIndex == counterIndex)
                                {
                                    return counter;
                                }
                            }
                        }


                    }
                }
                
            }
            return null;
        }

        private CounterDefinition GetCounter(DataBlock PerfDataBlock, int counterIndex, int ObjectIndex)
        {
            foreach (ObjectType obj in PerfDataBlock.objects)
            {
                if (obj.ObjectNameTitleIndex == ObjectIndex)
                {
                    foreach (CounterDefinition counter in obj.counters)
                    {
                        if (counter.CounterNameTitleIndex == counterIndex)
                        {
                            return counter;
                        }
                    }
                }
                

            }
            return null;
        }

        private void GetPerformanceData(DataBlock PerfDataBlock)
        {
            UIntPtr PerfDataBlockPntr = new UIntPtr();
            UIntPtr PerfObjectTypePntr = new UIntPtr();
            UIntPtr CounterDefinitionPntr = new UIntPtr();
            UIntPtr CurrentCounterPntr = new UIntPtr();
            UIntPtr CounterBlockPntr = new UIntPtr();
            UIntPtr PerfInstanceDefinitionPntr = new UIntPtr();

            int b = GetDataBlock(out PerfDataBlockPntr);
            
            long DefaultObject;
            int NumObjectTypes;
            long PerfFreq;
            long PerfTime;
            long PerfTime100nSec;
            int Revision;
            StringBuilder Signature = new StringBuilder(5);
            StringBuilder SystemName = new StringBuilder(20);
            StringBuilder SystemTime = new StringBuilder(100);
            int TotalByteLength;
            int Version;

            int c = GetDataBlockInfo(PerfDataBlockPntr,
                                        out DefaultObject,
                                        out NumObjectTypes,
                                        out PerfFreq,
                                        out PerfTime,
                                        out PerfTime100nSec,
                                        out Revision,
                                        Signature,
                                        SystemName,
                                        SystemTime,
                                        out TotalByteLength,
                                        out Version);

            PerfDataBlock.SystemName = SystemName.ToString();
            PerfDataBlock.SystemTime = SystemTime.ToString();
            PerfDataBlock.DefaultObject = DefaultObject;
            PerfDataBlock.NumObjectTypes = NumObjectTypes;
            PerfDataBlock.PerfFreq = PerfFreq;
            PerfDataBlock.PerfTime = PerfTime;
            PerfDataBlock.PerfTime100nSec = PerfTime100nSec;
            PerfDataBlock.Revision = Revision;
            PerfDataBlock.Signature = Signature.ToString();
            PerfDataBlock.TotalByteLength = TotalByteLength;
            PerfDataBlock.Version = Version;
            PerfDataBlock.address = PerfDataBlockPntr;

            int ObjectNameTitleIndex;
            int ObjectHelpTitleIndex;
            int DetailLevel;
            int NumCounters;
            long DefaultCounter;
            long NumInstances;
            int CodePage;


            int ByteLength;
            int ParentObjectTitleIndex;
            int ParentObjectInstance;
            StringBuilder Name = new StringBuilder(300);

            int CounterHelpTitleIndex;
            int CounterNameTitleIndex;
            int CounterSize;
            int CounterType;
            long DefaultScale;

            ulong Data;
            long Time;
            int MultiCounterData;
            long Frequency;


            ObjectType PerfObjectType;
            InstanceDefinition PerfInstDef;
            CounterDefinition PerfCounterDef;
            RAW_DATA rd;

            int t = GetFirstObjectType(PerfDataBlockPntr, out PerfObjectTypePntr);

            for (int i = 0; i < NumObjectTypes; i++)
            {
                PerfObjectType = new ObjectType();
                int z = GetPerfObjectTypeInfo(PerfObjectTypePntr,
                                                out ObjectNameTitleIndex,
                                                out TotalByteLength,
                                                out ObjectHelpTitleIndex,
                                                out DetailLevel,
                                                out NumCounters,
                                                out DefaultCounter,
                                                out NumInstances,
                                                out CodePage,
                                                out PerfTime,
                                                out PerfFreq);

                PerfObjectType.ObjectNameTitleIndex = ObjectNameTitleIndex;
                PerfObjectType.TotalByteLength = TotalByteLength;
                PerfObjectType.ObjectHelpTitleIndex = ObjectHelpTitleIndex;
                PerfObjectType.DetailLevel = DetailLevel;
                PerfObjectType.NumCounters = NumCounters;
                PerfObjectType.DefaultCounter = DefaultCounter;
                PerfObjectType.NumInstances = NumInstances;
                PerfObjectType.CodePage = CodePage;
                PerfObjectType.PerfTime = PerfTime;
                PerfObjectType.PerfFreq = PerfFreq;
                PerfObjectType.address = PerfObjectTypePntr;


                int q = GetCounterDefinition(PerfObjectTypePntr, out CounterDefinitionPntr);

                if (NumInstances > 0)
                {
                    int w = GetFirstInstance(PerfObjectTypePntr, out PerfInstanceDefinitionPntr);

                    for (int j = 0; j < NumInstances; j++)
                    {
                        PerfInstDef = new InstanceDefinition();

                        int o = GetInstanceInfo(PerfInstanceDefinitionPntr,
                                                PerfObjectType.CodePage,
                                                out ByteLength,
                                                out ParentObjectTitleIndex,
                                                out ParentObjectInstance,
                                                Name);

                        PerfInstDef.address = PerfInstanceDefinitionPntr;
                        PerfInstDef.ByteLength = ByteLength;
                        PerfInstDef.ParentObjectTitleIndex = ParentObjectTitleIndex;
                        PerfInstDef.ParentObjectInstance = ParentObjectInstance;
                        PerfInstDef.Name = Name.ToString();
                        Name.Clear();

                        CurrentCounterPntr = CounterDefinitionPntr;

                        int u = GetCounterBlock_Inst(PerfInstanceDefinitionPntr, out CounterBlockPntr);

                        for (int k = 0; k < PerfObjectType.NumCounters; k++)
                        {
                            PerfCounterDef = new CounterDefinition();

                            int p = GetCounterInfo(CurrentCounterPntr,
                                                    out ByteLength,
                                                    out CounterHelpTitleIndex,
                                                    out CounterNameTitleIndex,
                                                    out CounterSize,
                                                    out CounterType,
                                                    out DefaultScale,
                                                    out DetailLevel);

                            PerfCounterDef.address = CurrentCounterPntr;
                            PerfCounterDef.ByteLength = ByteLength;
                            PerfCounterDef.CounterHelpTitleIndex = CounterHelpTitleIndex;
                            PerfCounterDef.CounterNameTitleIndex = CounterNameTitleIndex;
                            PerfCounterDef.CounterSize = CounterSize;
                            PerfCounterDef.CounterType = CounterType;

                            long scale = 0;
                            if ((DefaultScale & 2147483648) != 0)
                            {
                                scale = DefaultScale - 4294967295 - 1;
                            }
                            else scale = DefaultScale;
                            PerfCounterDef.DefaultScale = scale;
                            PerfCounterDef.DetailLevel = DetailLevel;

                            bool qq = GetCounterValue(PerfDataBlockPntr,
                                                        PerfObjectTypePntr,
                                                        CurrentCounterPntr,
                                                        CounterBlockPntr,
                                                        out Data,
                                                        out Time,
                                                        out MultiCounterData,
                                                        out Frequency);

                            rd = new RAW_DATA();

                            rd.CounterType = PerfCounterDef.CounterType;
                            rd.Data = Data;
                            rd.Time = Time;
                            rd.MultiCounterData = MultiCounterData;
                            rd.Frequency = Frequency;

                            PerfCounterDef.RawData = rd;

                            PerfInstDef.counters.Add(PerfCounterDef);

                            int s = GetNextCounter(CurrentCounterPntr, out CurrentCounterPntr);
                        }

                        PerfObjectType.instances.Add(PerfInstDef);

                        int y = GetNextInstance(CounterBlockPntr, out PerfInstanceDefinitionPntr);
                    }
                }
                else
                {
                    int x = GetCounterBlock_Obj(PerfObjectTypePntr, out CounterBlockPntr);

                    for (int j = 0; j < PerfObjectType.NumCounters; j++)
                    {
                        PerfCounterDef = new CounterDefinition();

                        int n = GetCounterInfo(CounterDefinitionPntr,
                                                out ByteLength,
                                                out CounterHelpTitleIndex,
                                                out CounterNameTitleIndex,
                                                out CounterSize,
                                                out CounterType,
                                                out DefaultScale,
                                                out DetailLevel);

                        PerfCounterDef.address = CounterDefinitionPntr;
                        PerfCounterDef.ByteLength = ByteLength;
                        PerfCounterDef.CounterHelpTitleIndex = CounterHelpTitleIndex;
                        PerfCounterDef.CounterNameTitleIndex = CounterNameTitleIndex;
                        PerfCounterDef.CounterSize = CounterSize;
                        PerfCounterDef.CounterType = CounterType;
                        long scale = 0;
                        if ((DefaultScale & 2147483648) != 0)
                        {
                            scale = DefaultScale - 4294967295 - 1;
                        }
                        else scale = DefaultScale;
                        PerfCounterDef.DefaultScale = scale;
                        PerfCounterDef.DetailLevel = DetailLevel;

                        bool qq = GetCounterValue(PerfDataBlockPntr,
                                                    PerfObjectTypePntr,
                                                    CounterDefinitionPntr,
                                                    CounterBlockPntr,
                                                    out Data,
                                                    out Time,
                                                    out MultiCounterData,
                                                    out Frequency);

                        rd = new RAW_DATA();
                        rd.CounterType = PerfCounterDef.CounterType;
                        rd.Data = Data;
                        rd.Time = Time;
                        rd.MultiCounterData = MultiCounterData;
                        rd.Frequency = Frequency;

                        PerfCounterDef.RawData = rd;

                        PerfObjectType.counters.Add(PerfCounterDef);

                        int m = GetNextCounter(CounterDefinitionPntr, out CounterDefinitionPntr);
                    }
                }
                PerfDataBlock.objects.Add(PerfObjectType);

                int v = GetNextObjectType(PerfObjectTypePntr, out PerfObjectTypePntr);
            }
        }

        private void FillNames(Dictionary<int, string> names)
        {
            RegistryKey hKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Perflib\\019");
            string[] va = (string[])hKey.GetValue("CounterDefinition");


            for (int i = 0; i < va.Length - 1; i = i + 2)
            {
                names.Add(int.Parse(va[i]), va[i + 1]);
            }

        }

        private void FillHelps(Dictionary<int, string> helps)
        {
            RegistryKey hKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Perflib\\019");
            string[] va = (string[])hKey.GetValue("Help");
            for (int i = 0; i < va.Length - 1; i = i + 2)
            {
                helps.Add(int.Parse(va[i]), va[i + 1]);
            }
        }
        
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            textBox1.Text = "";
            listView1.Items.Clear();
            TreeNode t = e.Node;

            if (t.Parent == null)
            {
                ListViewItem lvi = new ListViewItem(new string[] { "SystemName", PerfDataBlock1.SystemName });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                lvi = new ListViewItem(new string[] { "SystemTime", PerfDataBlock1.SystemTime });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                lvi = new ListViewItem(new string[] { "DefaultObject", names[(int)PerfDataBlock1.DefaultObject] });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                lvi = new ListViewItem(new string[] { "NumObjectTypes", PerfDataBlock1.NumObjectTypes.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                lvi = new ListViewItem(new string[] { "PerfFreq", PerfDataBlock1.PerfFreq.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                lvi = new ListViewItem(new string[] { "PerfTime", PerfDataBlock1.PerfTime.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                lvi = new ListViewItem(new string[] { "PerfTime100nSec", PerfDataBlock1.PerfTime100nSec.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                lvi = new ListViewItem(new string[] { "Revision", PerfDataBlock1.Revision.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                lvi = new ListViewItem(new string[] { "Signature", PerfDataBlock1.Signature });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                lvi = new ListViewItem(new string[] { "TotalByteLength", PerfDataBlock1.TotalByteLength.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                lvi = new ListViewItem(new string[] { "Version", PerfDataBlock1.Version.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);
            }
            else if (t.Parent.Text.Equals("DataBlock"))
            {
                ObjectType obj = PerfDataBlock1.objects.ElementAt(t.Index);

                ListViewItem lvi = new ListViewItem(new string[] { "TotalByteLength", obj.TotalByteLength.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);
                lvi = new ListViewItem(new string[] { "ObjectNameTitleIndex", obj.ObjectNameTitleIndex.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);
                lvi = new ListViewItem(new string[] { "ObjectHelpTitleIndex", obj.ObjectHelpTitleIndex.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);
                lvi = new ListViewItem(new string[] { "DetailLevel", obj.DetailLevel.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);
                lvi = new ListViewItem(new string[] { "NumCounters", obj.NumCounters.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);
                lvi = new ListViewItem(new string[] { "NumInstances", obj.NumInstances.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);
                lvi = new ListViewItem(new string[] { "CodePage", obj.CodePage.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);
                lvi = new ListViewItem(new string[] { "PerfTime", obj.PerfTime.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);
                lvi = new ListViewItem(new string[] { "PerfFreq", obj.PerfFreq.ToString() });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                foreach (CounterDefinition counter in obj.counters)
                {
                    if (names.ContainsKey(counter.CounterNameTitleIndex))
                    {
                        lvi = new ListViewItem(new string[] { names[counter.CounterNameTitleIndex], counter.VALUE });
                        lvi.ImageIndex = 1;
                        listView1.Items.Add(lvi);
                    }
                    else
                    {
                        lvi = new ListViewItem(new string[] { counter.CounterNameTitleIndex.ToString(), counter.VALUE });
                        lvi.ImageIndex = 1;
                        listView1.Items.Add(lvi);
                    }

                }

                if (helps.ContainsKey(obj.ObjectHelpTitleIndex))
                {
                    textBox1.Text += helps[obj.ObjectHelpTitleIndex];
                }

            }
            else
            {
                ObjectType obj = PerfDataBlock1.objects.ElementAt(t.Parent.Index);
                InstanceDefinition inst = obj.instances.ElementAt(t.Index);

                ListViewItem lvi = new ListViewItem(new string[] { "Name", inst.Name });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);


                foreach (CounterDefinition counter in inst.counters)
                {
                    if (names.ContainsKey(counter.CounterNameTitleIndex))
                    {
                        lvi = new ListViewItem(new string[] { names[counter.CounterNameTitleIndex], counter.VALUE });
                        lvi.ImageIndex = 1;
                        listView1.Items.Add(lvi);
                    }
                    else
                    {
                        lvi = new ListViewItem(new string[] { counter.CounterNameTitleIndex.ToString(), counter.VALUE });
                        lvi.ImageIndex = 1;
                        listView1.Items.Add(lvi);
                    }
                }
                
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox1.Text = "";
            int index = listView1.SelectedItems[0].Index;

            TreeNode t = treeView1.SelectedNode;
            TreeNode tp = t.Parent;

            if (t.Parent == null)
            {
                switch (index)
                {
                    case 0: //SystemName
                        textBox1.Text += "Имя системы\r\n";
                        break;
                    case 1: //SystemTime
                        textBox1.Text += "Системное время (UTC) \r\n";
                        textBox1.Text += "Time at the system under measurement\r\n";
                        textBox1.Text += "Тип: SYSTEMTIME\r\n";
                        break;
                    case 2: //DefaultObject
                        textBox1.Text += "Объект по умолчанию, отображаемый при получении данных из этой системы \r\n";
                        textBox1.Text += "Object Title Index of default object to display when data from this system is retrieved (-1 = none, but this is not expected to be used) \r\n";
                        textBox1.Text += "Тип: LONG\r\n";
                        break;
                    case 3: //NumObjectTypes
                        textBox1.Text += "Количество типов объектов\r\n";
                        textBox1.Text += "Number of types of objects being reported\r\n";
                        textBox1.Text += "Тип: DWORD\r\n";
                        break;
                    case 4: //PerfFreq
                        textBox1.Text += "Частота счётчика производительности в измеряемой системе\r\n";
                        textBox1.Text += "Performance counter frequency at the system under measurement\r\n";
                        textBox1.Text += "Тип: LARGE_INTEGER\r\n";
                        break;
                    case 5: //PerfTime
                        textBox1.Text += "Значение счётчика производительности в измеряемой системе \r\n";
                        textBox1.Text += "Performance counter value at the system under measurement \r\n";
                        textBox1.Text += "Тип: LARGE_INTEGER\r\n";
                        break;
                    case 6: //PerfTime100nSec
                        textBox1.Text += "Время счётчика производительности (ед. изм. - 100 нсек.) в измеряемой системе \r\n";
                        textBox1.Text += "Performance counter time in 100 nsec units at the system under measurement\r\n";
                        textBox1.Text += "Тип: LARGE_INTEGER\r\n";
                        break;
                    case 7: //Revision
                        textBox1.Text += "Редакция версии \r\n";
                        textBox1.Text += "Revision of these data structures starting at 0 for each Version\r\n";
                        textBox1.Text += "Тип: DWORD\r\n";
                        break;
                    case 8: //Signature
                        textBox1.Text += "Сигнатура \"PERF\" \r\n";
                        textBox1.Text += "Signature: Unicode \"PERF\"\r\n";
                        textBox1.Text += "Тип: WCHAR\r\n";
                        break;
                    case 9: //TotalByteLength
                        textBox1.Text += "Размер блока в байтах \r\n";
                        textBox1.Text += "Total length of data block\r\n";
                        textBox1.Text += "Тип: DWORD\r\n";
                        break;
                    case 10:    //Version
                        textBox1.Text += "Версия \r\n";
                        textBox1.Text += "Version of these data structures starting at 1\r\n";
                        textBox1.Text += "Тип: DWORD\r\n";
                        break;
                }
            }
            else if (t.Parent.Text.Equals("DataBlock"))
            {
                switch (index)
                {
                    case 0: //TotalByteLength

                        textBox1.Text += "Length of this object definition including this structure, the counter definitions, and the instance definitions and the counter blocks for each instance: This is the offset from this structure to the next object, if any\r\n";
                        textBox1.Text += "Тип: DWORD\r\n";
                        break;
                    case 1: //ObjectNameTitleIndex
                        textBox1.Text += "Индекс типа объекта\r\n";
                        textBox1.Text += "Index to name in Title Database\r\n";
                        textBox1.Text += "Тип: DWORD\r\n";
                        break;
                    case 2: //ObjectHelpTitleIndex
                        textBox1.Text += "Индекс описания объекта\r\n";
                        textBox1.Text += "Index to Help in Title Database\r\n";
                        textBox1.Text += "Тип: DWORD\r\n";
                        break;
                    case 3: //DetailLevel
                        
                        textBox1.Text += "Object level of detail (for controlling display complexity); will be min of detail levels for all this object's counters\r\n";
                        textBox1.Text += "Тип: DWORD\r\n";
                        break;
                    case 4: // NumCounters
                        textBox1.Text += "Количество счётчиков для каждого объекта\r\n";
                        textBox1.Text += "Number of counters in each counter block (one counter block per instance)\r\n";
                        textBox1.Text += "Тип: DWORD\r\n";
                        break;
                    case 5: //NumInstances
                        textBox1.Text += "Количество объектов\r\n";
                        textBox1.Text += "Number of object instances for which counters are being returned from the system under measurement. If the object defined will never have any instance data structures (InstanceDefinition) then this value should be -1, if the object can have 0 or more instances, but has none present, then this should be 0, otherwise this field contains the number of instances of this counter.\r\n";
                        textBox1.Text += "Тип: LONG\r\n";
                        break;
                    case 6: //CodePage
                        textBox1.Text += "Кодовая страница имени объекта\r\n";
                        textBox1.Text += "0 if instance strings are in UNICODE, else the Code Page of the instance names\r\n";
                        textBox1.Text += "Тип: DWORD\r\n";
                        break;
                    case 7: //PerfTime
                        textBox1.Text += "Время\r\n";
                        textBox1.Text += "Sample Time in \"Object\" units\r\n";
                        textBox1.Text += "Тип: LARGE_INTEGER\r\n";
                        break;
                    case 8: //PerfFreq
                        textBox1.Text += "Частота\r\n";
                        textBox1.Text += "Frequency of \"Object\" units in counts per second.\r\n";
                        textBox1.Text += "Тип: LARGE_INTEGER\r\n";
                        break;
                    default:
                        int newindex = index - 9;

                        int objindex = t.Index;

                        ObjectType obj = PerfDataBlock1.objects.ElementAt(objindex);

                        CounterDefinition counter = obj.counters.ElementAt(newindex);

                        StringBuilder name = new StringBuilder(50);
                        StringBuilder description = new StringBuilder(500);

                        int a = GetCounterType(counter.CounterType, name, description);
                        textBox1.Text += "Размер: " + counter.CounterSize + " байт" + "\r\n";
                        textBox1.Text += "Тип счётчика: " + counter.CounterType + " = " + name.ToString() + "\r\n";
                        textBox1.Text += "Описание типа счётчика: " + description + "\r\n";
                        textBox1.Text += "Масштаб по умолчанию: " + Math.Pow(10,counter.DefaultScale) + "\r\n";
                        textBox1.Text += "Уровень детализации: " + counter.DetailLevel + " = " + GetDetailLevel(counter.DetailLevel) + "\r\n";


                        if (helps.ContainsKey(counter.CounterHelpTitleIndex))
                        {
                            textBox1.Text += helps[counter.CounterHelpTitleIndex];
                        }
                        break;
                }
            }
            else
            {
                switch (index)
                {
                    case 0:
                        textBox1.Text += "Имя объекта\r\n";
                        break;
                    default:
                        int newindex = index - 1;

                        ObjectType obj = PerfDataBlock1.objects.ElementAt(tp.Index);
                        InstanceDefinition inst = obj.instances.ElementAt(t.Index);
                        CounterDefinition counter = inst.counters.ElementAt(newindex);

                        StringBuilder name = new StringBuilder(50);
                        StringBuilder description = new StringBuilder(500);

                        int a = GetCounterType(counter.CounterType, name, description);

                        textBox1.Text += "Размер: " + counter.CounterSize + " байт" + "\r\n";
                        textBox1.Text += "Тип счётчика: " + counter.CounterType + " = " + name.ToString() + "\r\n";
                        textBox1.Text += "Описание типа счётчика: " + description + "\r\n";
                        textBox1.Text += "Масштаб по умолчанию: " + Math.Pow(10, counter.DefaultScale) + "\r\n";
                        textBox1.Text += "Уровень детализации: " + counter.DetailLevel + " = " + GetDetailLevel(counter.DetailLevel) + "\r\n";

                        if (helps.ContainsKey(counter.CounterHelpTitleIndex))
                        {
                            textBox1.Text += helps[counter.CounterHelpTitleIndex];
                        }

                        break;
                }
            }
        }

        private string GetDetailLevel(int num)
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
    }
}