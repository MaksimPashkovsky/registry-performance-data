using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public DataBlock PerfDataBlock1;
        public DataBlock PerfDataBlock2;

        Dictionary<int, string> names;
        Dictionary<int, string> helps;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ImageList il = new ImageList();
            il.Images.Add(Image.FromFile("Images\\open.png"));
            il.Images.Add(Image.FromFile("Images\\data.png"));
            il.Images.Add(Image.FromFile("Images\\key.jpg"));
            il.Images.Add(Image.FromFile("Images\\file.png"));
            treeView1.ImageList = il;

            ImageList il2 = new ImageList();
            il2.Images.Add(Image.FromFile("Images\\param.png"));
            il2.Images.Add(Image.FromFile("Images\\counter.png"));
            listView1.SmallImageList = il2;

            names = Utils.GetNamesFromRegistry();
            helps = Utils.GetHelpsFromRegistry();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PerfDataBlock1 = DataBlock.GetPerformanceData();
            PerfDataBlock2 = DataBlock.GetPerformanceData();
            DataBlock.CalculateCounterValues(PerfDataBlock1, PerfDataBlock2);
            FillTree();
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PerfDataBlock1 = PerfDataBlock2;
            PerfDataBlock2 = DataBlock.GetPerformanceData();
            DataBlock.CalculateCounterValues(PerfDataBlock1, PerfDataBlock2);
            treeView1.Nodes.Clear();
            FillTree();
            button1.Enabled = false;
        }

        private void FillTree()
        {
            treeView1.Nodes.Add("DataBlock", "DataBlock", 1);
            foreach (ObjectType obj in PerfDataBlock1.GetObjects())
            {
                TreeNode objNode = new TreeNode();
                int index = obj.ObjectNameTitleIndex;
                objNode.Text = names.ContainsKey(index) ? names[index] : index.ToString();
                if (obj.GetInstances().Count > 0)
                {
                    foreach (InstanceDefinition inst in obj.GetInstances())
                    {
                        objNode.Nodes.Add(inst.Name, inst.Name, 3);
                    }
                }
                objNode.ImageIndex = 2;
                treeView1.Nodes[0].Nodes.Add(objNode);
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            textBox1.Text = "";
            listView1.Items.Clear();
            TreeNode t = e.Node;

            if (t.Parent == null)
            {
                foreach (var field in PerfDataBlock1.GetType().GetFields())
                {
                    ListViewItem lvi = new ListViewItem(new string[] { field.Name, field.GetValue(PerfDataBlock1).ToString() });
                    lvi.ImageIndex = 0;
                    listView1.Items.Add(lvi);
                }
            }
            else if (t.Parent.Text.Equals("DataBlock"))
            {
                ObjectType obj = PerfDataBlock1.GetObjects().ElementAt(t.Index);
                foreach (var field in obj.GetType().GetFields())
                {
                    ListViewItem lvi = new ListViewItem(new string[] { field.Name, field.GetValue(obj).ToString() });
                    lvi.ImageIndex = 0;
                    listView1.Items.Add(lvi);
                }
                foreach (CounterDefinition counter in obj.GetCounters())
                {
                    ListViewItem lvi = new ListViewItem(new string[] { 
                        names.ContainsKey(counter.CounterNameTitleIndex) ? 
                        names[counter.CounterNameTitleIndex] : counter.CounterNameTitleIndex.ToString(), counter.Value 
                    });
                    lvi.ImageIndex = 1;
                    listView1.Items.Add(lvi);
                }
                if (helps.ContainsKey(obj.ObjectHelpTitleIndex))
                    textBox1.Text += helps[obj.ObjectHelpTitleIndex];
            }
            else
            {
                ObjectType obj = PerfDataBlock1.GetObjects().ElementAt(t.Parent.Index);
                InstanceDefinition inst = obj.GetInstances().ElementAt(t.Index);
                ListViewItem lvi = new ListViewItem(new string[] { "Name", inst.Name });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                foreach (CounterDefinition counter in inst.Counters)
                {
                    lvi = new ListViewItem(new string[] {
                        names.ContainsKey(counter.CounterNameTitleIndex) ?
                        names[counter.CounterNameTitleIndex] : counter.CounterNameTitleIndex.ToString(), counter.Value
                    });
                    lvi.ImageIndex = 1;
                    listView1.Items.Add(lvi);
                }
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox1.Text = "";
            int index = listView1.SelectedItems[0].Index;
            string name = listView1.SelectedItems[0].Text;

            TreeNode t = treeView1.SelectedNode;
            TreeNode tp = t.Parent;

            if (t.Parent == null)
            {
                textBox1.Text = Utils.GetDataBlockPropertiesDescriptionByName(name);
            }
            else if (t.Parent.Text.Equals("DataBlock"))
            {
                ObjectType obj = PerfDataBlock1.GetObjects().ElementAt(t.Index);
                int numOfFields = obj.GetType().GetFields().Length;
                if (index < numOfFields) {
                    textBox1.Text = Utils.GetObjectTypePropertiesDescriptionByName(name);
                    return;
                }
                CounterDefinition counter = obj.GetCounters().ElementAt(index - numOfFields);
                StringBuilder countername = new StringBuilder(50);
                StringBuilder description = new StringBuilder(500);
                Externals.GetCounterType(counter.CounterType, countername, description);
                textBox1.Text = string.Format(counter.GetDescription(), countername.ToString(), description.ToString());
                if (helps.ContainsKey(counter.CounterHelpTitleIndex))
                    textBox1.Text += helps[counter.CounterHelpTitleIndex];
            }
            else
            {
                if (index == 0) {
                    textBox1.Text += "Object name\r\n";
                    return;
                }
                ObjectType obj = PerfDataBlock1.GetObjects().ElementAt(tp.Index);
                InstanceDefinition inst = obj.GetInstances().ElementAt(t.Index);
                CounterDefinition counter = inst.Counters.ElementAt(index - 1);
                StringBuilder countername = new StringBuilder(50);
                StringBuilder description = new StringBuilder(500);
                Externals.GetCounterType(counter.CounterType, countername, description);
                textBox1.Text = string.Format(counter.GetDescription(), countername.ToString(), description.ToString());
                if (helps.ContainsKey(counter.CounterHelpTitleIndex))
                    textBox1.Text += helps[counter.CounterHelpTitleIndex];
            }
        }
    }
}