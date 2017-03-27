using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GameFormatReader.Common;
using System.Diagnostics;
using WArchiveTools.Compression;

namespace TSCBReader.src
{
    public class TSCB
    {
        public class FirstSection
        {
            public class FirstSectionInstance
            {
                public int Index { get; private set; }
                public float UnknownFloat1 { get; private set; }
                public float UnknownFloat2 { get; private set; }
                public float UnknownFloat3 { get; private set; }
                public float UnknownFloat4 { get; private set; }

                public void Load(EndianBinaryReader reader)
                {
                    Index = reader.ReadInt32();
                    UnknownFloat1 = reader.ReadSingle();
                    UnknownFloat2 = reader.ReadSingle();
                    UnknownFloat3 = reader.ReadSingle();
                    UnknownFloat4 = reader.ReadSingle();
                }

                public override string ToString()
                {
                    return $"[{ Index }]";
                }
            }

            public List<FirstSectionInstance> FirstSectionInstances { get; private set; }

            public int UnknownInt1 { get; private set; }
            public int UnknownInt2 { get; private set; }
            public float UnknownFloat1 { get; private set; }
            public int UnknownInt3 { get; private set; }

            public void Load(EndianBinaryReader reader, int instanceCount)
            {
                FirstSectionInstances = new List<FirstSectionInstance>();

                UnknownInt1 = reader.ReadInt32();
                UnknownInt2 = reader.ReadInt32();
                UnknownFloat1 = reader.ReadSingle();
                UnknownInt3 = reader.ReadInt32();

                int firstSectionSize = reader.ReadInt32();

                for (int i = 0; i < instanceCount; i++)
                {
                    long curPos = reader.BaseStream.Position;
                    int instanceOffset = reader.PeekReadInt32();

                    reader.BaseStream.Seek(instanceOffset, SeekOrigin.Current);

                    FirstSectionInstance inst = new FirstSectionInstance();
                    inst.Load(reader);
                    FirstSectionInstances.Add(inst);

                    reader.BaseStream.Seek(curPos + 4, SeekOrigin.Begin);
                }
            }
        }

        public class SecondSection
        {
            public void Load(EndianBinaryReader reader)
            {

            }
        }

        public FirstSection FirstSectionData { get; private set; }
        public SecondSection SecondSectionData { get; private set; }

        public float UnknownFloat1 { get; private set; }
        public float UnknownFloat2 { get; private set; }

        private int m_stringTableOffset;
        private int m_firstSectionCount;
        private int m_secondSectionCount;

        public void LoadTSCB(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);

                if (new string(reader.ReadChars(4)) != "TSCB")
                    throw new FormatException("File was not TSCB!");

                Trace.Assert(reader.ReadInt32() == 0x0A000000);
                Trace.Assert(reader.ReadInt32() == 1);

                m_stringTableOffset = reader.ReadInt32();
                UnknownFloat1 = reader.ReadSingle();
                UnknownFloat2 = reader.ReadSingle();
                m_firstSectionCount = reader.ReadInt32();
                m_secondSectionCount = reader.ReadInt32();

                FirstSectionData = new FirstSection();
                FirstSectionData.Load(reader, m_firstSectionCount);

                SecondSectionData = new SecondSection();
                SecondSectionData.Load(reader);
            }
        }
    }
}
