using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDamage
{
    class CombatInstance
    {
        //Class for holding the info for each combat instance
        //Total damage delt
        public int DamageGiven { get; set; }
        //Total damage taken
        public int DamageTaken { get; set; }
        //Start time as string
        public string DateTime { get; set; }
        //Start time of combat instance
        public DateTime Start { get; set; }
        //End time of combat instance
        public DateTime End { get; set; }
        private List<DamageType> DamageSourcesIn = new List<DamageType>();
        private List<DamageType> DamageSourcesOut = new List<DamageType>();
        public CombatInstance(string date, DateTime start)
        {
            DamageGiven = 0;
            DamageTaken = 0;
            DateTime = date;
            Start = start;
        }
        public void AddDamage(string name, int damage, string type)
        {
            int I = 0,Location = 0;
            bool ContainsName = false;
            if (type == "In")
            {
                foreach (DamageType D in DamageSourcesIn)
                {
                    if (D.Name == name)
                    {
                        ContainsName = true;
                        Location = I;
                    }
                    I++;
                }
                if (ContainsName)
                {
                    DamageSourcesIn[Location].Damage += damage;
                }
                else
                {
                    DamageSourcesIn.Add(new DamageType(name, damage));
                }
            }
            else if(type == "Out")
            {
                foreach (DamageType D in DamageSourcesOut)
                {
                    if (D.Name == name)
                    {
                        ContainsName = true;
                        Location = I;
                    }
                    I++;
                }
                if (ContainsName)
                {
                    DamageSourcesOut[Location].Damage += damage;
                }
                else
                {
                    DamageSourcesOut.Add(new DamageType(name, damage));
                }
            }
            
        }
        public List<DamageType> GetIn()
        {
            return DamageSourcesIn;
        }
        public List<DamageType> GetOut()
        {
            return DamageSourcesOut;
        }
    }
    class CombatData
    {
        //Name of charcter related to combatdata
        public string Listener { get; set; }
        //Lines in file 
        private List<string> Lines ;
        //Lines of outgoing damage
        private List<string> Outgoing ;
        //Lines of imcoming damage
        private List<string> Incoming ;
        //List of differnt Combat instances 
        private List<CombatInstance> CombatList;
        //Miss counters
        private int YouMiss, TheyMiss;
        public CombatData()
        {
            Lines = new List<string>();
            Outgoing = new List<string>();
            Incoming = new List<string>();
            CombatList = new List<CombatInstance>();
        }
        // Time 1 and time 2 to be used to compair and check datetimes
        DateTime t1, t2;
        // The differnce between t1 and t2
        TimeSpan TDelta;
        // a counter int
        int I = 0;
        //a bool to make sure its not the first pass for the parsedata meathod
        bool FirstPass = true;
        public void ParseData()
        {
            bool contains = false;
            foreach (string s in Lines)
            {
                //if its the first combat line start the combatinstance list
                if (s.Contains("combat") && FirstPass)
                {
                    contains = true;
                    int DateBreak1 = s.IndexOf("["), DateBreak2 = s.IndexOf("]");
                    t1 = t2;
                    DateBreak1 = s.IndexOf("[");
                    DateBreak2 = s.IndexOf("]");
                    DateBreak1 += 1;
                    DateBreak2 -= DateBreak1;
                    t2 = Convert.ToDateTime(s.Substring(DateBreak1, DateBreak2));
                    FirstPass = false;
                    CombatList.Add(new CombatInstance(t2.ToString(), t2));
                }
                ParseLines(s);
            }
            //sets the final Combat instance end time
            if (contains)
            {
                CombatList[I].End = t2;
                SeperateCombat();
            }
            
            //parses out the incoming list and outgoing list 
            
        }
        private void ParseLines(string s)
        {
            //Checks to see what kinda of line it is and sets it to the proper list
            if (s.Contains("Listener"))
            {
                Listener = s.Substring(12);
            }
            else if (s.Contains("combat"))
            {
                int DateBreak1 = s.IndexOf("["), DateBreak2 = s.IndexOf("]");
                t1 = t2;
                DateBreak1 += 1;
                DateBreak2 -= DateBreak1;
                t2 = Convert.ToDateTime(s.Substring(DateBreak1, DateBreak2));
                TDelta = t2 - t1;
                if (TDelta.Minutes > 10)
                {
                    CombatList.Add(new CombatInstance(t2.ToString(), t2));
                    CombatList[I].End = t1;
                    I++;
                }
                if (s.Contains("misses you completely"))
                {
                    TheyMiss++;
                }
                else if (s.Contains("Your "))
                {
                    YouMiss++;
                }
                else if (s.Contains("color=0xff00ffff"))
                {
                    Outgoing.Add(s);
                }
                else if (s.Contains("color=0xffcc0000"))
                {
                    Incoming.Add(s);
                }
            }
        }
        public void PassLines(List<string> lines)
        {
                foreach(string s in lines)
                {
                    Lines.Add(s);
                }
        }
        private void SeperateCombat()
        {
            int Break1, Break2;
            int tempInt = 0;
            string tempString = "";
            int C = 0;
            int DateBreak1, DateBreak2;
            if (Outgoing.Count > 0)
            {
                DateBreak1 = Outgoing[0].IndexOf("[");
                DateBreak2 = Outgoing[0].IndexOf("]");
                DateBreak1 += 1;
                DateBreak2 -= DateBreak1;
                foreach (String s in Outgoing)
                {

                    t2 = Convert.ToDateTime(s.Substring(DateBreak1, DateBreak2));
                    DateBreak1 = s.IndexOf("[");
                    DateBreak2 = s.IndexOf("]");
                    DateBreak1 += 1;
                    DateBreak2 -= DateBreak1;
                    Break1 = s.IndexOf("<b>");
                    Break2 = s.IndexOf("</b>");
                    Break1 += 3;
                    Break2 -= Break1;
                    tempInt = int.Parse(s.Substring(Break1, Break2));
                    if (!(t2 < CombatList[C].End.AddMinutes(2) && t2 > CombatList[C].Start.AddMinutes(-2)))
                    {
                        C++;
                    }
                    CombatList[C].DamageGiven += tempInt;
                    Break1 = s.IndexOf("<color=0x77ffffff> - ");
                    Break2 = s.LastIndexOf(" - ");
                    Break1 += 21;
                    Break2 -= Break1;
                    tempString = (s.Substring(Break1, Break2));
                    CombatList[C].AddDamage(tempString, tempInt, "Out");
                }
            }
           if(Incoming.Count > 0)
            {
                int G = 0;
                DateBreak1 = Incoming[0].IndexOf("[");
                DateBreak2 = Incoming[0].IndexOf("]");
                DateBreak1 += 1;
                DateBreak2 -= DateBreak1;
                foreach (String s in Incoming)
                {
                    t2 = Convert.ToDateTime(s.Substring(DateBreak1, DateBreak2));
                    DateBreak1 = s.IndexOf("[");
                    DateBreak2 = s.IndexOf("]");
                    DateBreak1 += 1;
                    DateBreak2 -= DateBreak1;
                    Break1 = s.IndexOf("<b>");
                    Break2 = s.IndexOf("</b>");
                    Break1 += 3;
                    Break2 -= Break1;
                    tempInt = int.Parse(s.Substring(Break1, Break2));
                    if (!(t2 < CombatList[G].End.AddMinutes(2) && t2.AddMinutes(2) > CombatList[G].Start))
                    {
                        G++;
                    }
                    CombatList[G].DamageTaken += tempInt;
                    Break1 = s.IndexOf("<color=0xffffffff>");
                    Break2 = s.LastIndexOf("</b>");
                    Break1 += 18;
                    Break2 -= Break1;
                    tempString = (s.Substring(Break1, Break2));
                    CombatList[G].AddDamage(tempString, tempInt, "In");
                }
            }
            
        }
        public List<CombatInstance> GetCombat()
        {
            //used to retrun the parsed data
            return CombatList;
        }
    }
    class DamageType
    {
        //Class used to sperate out the damage dealers
        //Name of damage dealer
        public string Name { get; set; }
        //Damage from this source
        public double Damage { get; set; }
        public DamageType(string name,double damage)
        {
            Name = name;
            Damage = damage;
        }
    }
}
