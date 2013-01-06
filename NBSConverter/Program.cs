using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBSConverter
{
    class Note
    {
        public Note(short tick,short jumps,short layer,byte inst,sbyte key)
        {
            m_tick = tick;
            m_jumps = jumps;
            m_layer = layer;
            m_inst = inst;
            m_key = key;
        }

        public short m_tick;
        public short m_jumps;
        public short m_layer;
        public byte m_inst;
        public sbyte m_key;

        
    }
    class Program
    {
        static BinaryReader m_reader;

        static string readString()
        {
            string s = "";
            int len = m_reader.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                s += m_reader.ReadChar();
            }
            return s;
        }

        static void Main(string[] args)
        {
            if(args.Length != 0)
            {
                m_reader = new BinaryReader(new FileStream(args[0],FileMode.Open));

                short length_in_ticks = m_reader.ReadInt16();
                short height = m_reader.ReadInt16();

                string name = readString();
                string author = readString();

                string origAuthor = readString();

                string songDesc = readString();

                short tempo = m_reader.ReadInt16();

                m_reader.ReadByte(); // autosave on and autosave interval
                m_reader.ReadByte();

                byte timeSig = m_reader.ReadByte();

                m_reader.ReadInt32(); // minutes spent
                m_reader.ReadInt32(); // left clicks
                m_reader.ReadInt32(); // right clicks
                m_reader.ReadInt32(); // blocks added
                m_reader.ReadInt32(); // blocks removed

                string importedName = readString();


                List<Note> m_notes = new List<Note>();

                short tick = -1;

                while (true)
                {
                    short jumps = m_reader.ReadInt16();
                    if (jumps == 0) { break; }
                    short tickJumps = jumps;
                    tick += jumps;

                    short layer = -1;
                    while (true)
                    {
                        jumps = m_reader.ReadInt16();
                        if (jumps == 0) { break; }
                        layer++;

                        byte inst = m_reader.ReadByte();
                        sbyte key = m_reader.ReadSByte();

                        key = Clamp(key, 33, 57);
                        key -= 33;

                        m_notes.Add(new Note(tick,tickJumps,layer,inst,(sbyte)key));
                    }
                }

                string header= "{length="+length_in_ticks.ToString() +
                                     ",numLayers="+height.ToString() +
                                     ",name=\""+name+
                                     "\",author=\""+author+
                                     "\",originalAuthor=\""+origAuthor+
                                     "\",desc=\""+escape(songDesc)+
                                     "\",tempo="+ ( ((double)tempo) / 100.0).ToString()+
                                     ",timesig=" + timeSig.ToString() +
                                     ",importedName=\"" + importedName + "\"}";

                int i = 1;
                string notes = "{";
                
                foreach (Note n in m_notes)
                {
                    notes += "[" + i.ToString() + "]={tick=" + n.m_tick.ToString() + ",layer=" + n.m_layer.ToString() + ",key=" + n.m_key.ToString() + ",instrument=" + n.m_inst.ToString() +",jumps=" + n.m_jumps.ToString() + "},";
                    i++;
                }

                notes += "}";

                if(name == "")
                {
                    name = args[1].Replace("nbs", "");
                }

                File.WriteAllText(name+".lua","{header="+header+",notes="+notes+"}");
            }
            else
            {
                Console.WriteLine();
            }
        }

        private static sbyte Clamp(sbyte key, sbyte p1, sbyte p2)
        {
            if (key < p1) { return p1; }
            if (key > p2) { return p2; }
            return key;
        }

        private static string escape(string s)
        {
            return s.Replace("\"", "\\\"").Replace("\r","\\n");
        }
    }
}
