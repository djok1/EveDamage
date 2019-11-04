using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace EveDamage
{
    public partial class Form1 : Form
    {
        private StreamReader input;
        List<string> Lines = new List<string>();
        List<CombatInstance> CombatList = new List<CombatInstance>();
        CombatData ThisCombatData = new CombatData();
        List<DamageType> DamageSource;
        List<CombatData> Listeners;
        int select,type;
        public Form1()
        {
            InitializeComponent();
        }
        public void OpenFile()
        {
            DialogResult result;
            string fileName;
            using (OpenFileDialog fileChooser = new OpenFileDialog())
            {
                result = fileChooser.ShowDialog();
                fileName = fileChooser.FileName;
            }
            if (result == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    MessageBox.Show("Invalid File Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    try
                    {
                        input = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read));
                        btnOpen.Enabled = false;
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Error opening file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        public void AddLines()
        {
            try
            {
                while (!input.EndOfStream)
                {
                    Lines.Add(input.ReadLine());
                }
                input.Close();
                btnOpen.Enabled = true;
            }
            catch
            {
                MessageBox.Show("Please selecet a file");
            }

        }
        private void PopulateChart()
        {
            double Incoming = double.Parse(lblTaken.Text), Outgoing = double.Parse(lblDelt.Text);
            chartIncoming.Series["Incoming"].Points.Clear();
            chartOutgoing.Series["Outgoing"].Points.Clear();
            DamageSource = new List<DamageType>();
                DamageSource = CombatList[select].GetIn();
                foreach (DamageType D in DamageSource)
                {
                chartIncoming.Series["Incoming"].Points.AddXY(D.Name, D.Damage/Incoming);
                }
 
                DamageSource = CombatList[select].GetOut();
                foreach (DamageType D in DamageSource)
                {
                chartOutgoing.Series["Outgoing"].Points.AddXY(D.Name, D.Damage/Outgoing);
            }

        }
        private void cmbSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            select = cmbSelect.SelectedIndex;
            lblSource.Text = "";
            lblDelt.Text = CombatList[select].DamageGiven.ToString();
            lblTaken.Text = CombatList[select].DamageTaken.ToString();
            lblEnd.Text = CombatList[select].End.ToString();
            if(select >= 0)
            {
                cmbDamageType.Enabled = true;
            }
            cmbDamageType.Text = "Select Damage Type";
            cmbSource.Text = "Select Damage Source";
            cmbSource.Enabled = false;
            PopulateChart();

        }
        private void ParseCombat()
        {
            cmbSelect.Items.Clear();
            foreach (CombatInstance C in CombatList)
            {
                cmbSelect.Items.Add(C.DateTime);
            }
            cmbSelect.Enabled = true;
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            CombatList = new List<CombatInstance>();
            ThisCombatData = new CombatData();
            Listeners = new List<CombatData>();
            OpenFile();
            AddLines();
            ThisCombatData.PassLines(Lines);
            ThisCombatData.ParseData();
            Lines.Clear();
            CombatList = ThisCombatData.GetCombat();
            ParseCombat();
        }

        private void cmbSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblSource.Text = "";
            int Source = cmbSource.SelectedIndex;
            if(Source >= 0)
            {
                lblSource.Text = DamageSource[Source].Damage.ToString();
            }
        }
        public void OpenFolder()
        {
            DialogResult result;
            string folderName;
            using (FolderBrowserDialog folderChooser = new FolderBrowserDialog())
            {
                result = folderChooser.ShowDialog();
                folderName = folderChooser.SelectedPath;
            }
            if(result == DialogResult.OK)
            {
                foreach (string fileName in Directory.EnumerateFiles(folderName, "*.txt"))
                {
                    if (string.IsNullOrEmpty(fileName))
                    {
                        MessageBox.Show("Invalid File Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        try
                        {
                            input = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read));
                            AddListeners();
                        }
                        catch (IOException)
                        {
                            MessageBox.Show("Error opening file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
        bool FirstPass = true;
        private void AddListeners()
        {
            bool HasListener = false;
            AddLines();
            int C = 0;
            int Index = 0;
            int Location = 0;
            string tempString = "";
            int NoListener = -1;
            if (!FirstPass)
            {
                foreach (CombatData D in Listeners)
                {
                    for (int I = 0; I < 10 && I < Lines.Count - 1; I++)
                    {
                        if (Lines[I] == D.Listener)
                        {
                            HasListener = true;
                            Location = Index;
                            tempString = Lines[I];
                        }
                        if (Lines[I].Contains("Listener:"))
                        {
                            tempString = Lines[I];
                        }
                    }
                    if(D.Listener == "No Listener")
                    {
                        NoListener = Index;
                    }
                    Index++;
                }
                if(HasListener)
                {
                    Listeners[Location].PassLines(Lines);
                    Lines.Clear();
                }
                else
                {
                    if (tempString == "")
                    {
                        tempString = "No Listener";
                        if(NoListener != -1)
                        {
                            Listeners[NoListener].PassLines(Lines);
                            Lines.Clear();
                        }
                        else
                        {
                            Listeners.Add(new CombatData());
                            Listeners[Listeners.Count - 1].PassLines(Lines);
                            Listeners[Listeners.Count - 1].Listener = tempString;
                            Lines.Clear();
                        }
                    }
                    else
                    {
                        Listeners.Add(new CombatData());
                        Listeners[Listeners.Count - 1].PassLines(Lines);
                        Listeners[Listeners.Count - 1].Listener = tempString;
                        Lines.Clear();
                    }

                }
            }
            else
            {
                foreach (string l in Lines)
                {
                    if (l.Contains("Listener:"))
                    {
                        tempString = l;
                    }
                    C++;
                    if (C > 5)
                    {
                        break;
                    }
                }
                if (tempString == "")
                {
                    tempString = "No Listener";
                }
                Listeners.Add(new CombatData());
                Listeners[0].PassLines(Lines);
                Listeners[0].Listener = tempString;
                Lines.Clear();
                FirstPass = false;
            }

        }
        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            CombatList = new List<CombatInstance>();
            ThisCombatData = new CombatData();
            Listeners = new List<CombatData>();
            OpenFolder();
            foreach(CombatData L in Listeners)
            {
                if(L.Listener != "No Listener")
                {
                    L.ParseData();
                }
                
            }
            foreach (CombatData L in Listeners)
            {
                cmbListener.Items.Add(L.Listener);
            }
            cmbListener.Enabled = true;
            cmbDamageType.Enabled = false;
            cmbSelect.Enabled = false;
            cmbSource.Enabled = false;
        }

        private void cmbListener_SelectedIndexChanged(object sender, EventArgs e)
        {
            int Index = cmbListener.SelectedIndex;
            if(Index >= 0)
            {
                CombatData ThisCombatData = Listeners[Index];
                CombatList = ThisCombatData.GetCombat();
                ParseCombat();
                cmbSelect.Enabled = true;
                cmbDamageType.Enabled = false;
                cmbSource.Enabled = false;
                cmbSelect.Text = "Select Encounter Date Time";
                cmbDamageType.Text = "Select Damage Type";
                cmbSource.Text = "Select Damage Source";
                lblDelt.Text = "";
                lblTaken.Text = "";
                lblEnd.Text = "";
                lblSource.Text = "";
            }
        }

        private void cmbDamageType_SelectedIndexChanged(object sender, EventArgs e)
        {
            DamageSource = new List<DamageType>();
            cmbSource.Items.Clear();
            lblSource.Text = "";
            type = cmbDamageType.SelectedIndex;
            if(type == 0)
            {
                DamageSource = CombatList[select].GetIn();
                foreach(DamageType D in DamageSource)
                {
                    cmbSource.Items.Add(D.Name);
                }
                cmbSource.Enabled = true;
            }
            else if(type == 1)
            {
                DamageSource = CombatList[select].GetOut();
                foreach (DamageType D in DamageSource)
                {
                    cmbSource.Items.Add(D.Name);
                }
                cmbSource.Enabled = true;
            }
            else
            {
                cmbSource.Enabled = false;
            }
            cmbSource.Text  = "Select Damage Source";
        }
    }
}
