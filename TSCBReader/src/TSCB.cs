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

                long sectionStartPos = reader.BaseStream.Position;
                int firstSectionSize = reader.ReadInt32();

                for (int i = 0; i < instanceCount; i++)
                {
                    long curPos = reader.BaseStream.Position; // Store current position
                    int instanceOffset = reader.PeekReadInt32(); // Peek-read the offset to the instance. We need to stay at this offset since the instance's offset is relative to it

                    reader.BaseStream.Seek(instanceOffset, SeekOrigin.Current); // Seek to instance
                    FirstSectionInstance inst = new FirstSectionInstance();
                    inst.Load(reader);
                    FirstSectionInstances.Add(inst);

                    reader.BaseStream.Seek(curPos + 4, SeekOrigin.Begin); // Return to the offset block ready to read the next offset
                }

                reader.BaseStream.Seek(sectionStartPos + firstSectionSize, SeekOrigin.Begin);
            }
        }

        public class SecondSection
        {
            public class SecondSectionInstance
            {
                public float UnknownFloat1 { get; private set; }
                public float UnknownFloat2 { get; private set; }
                public float UnknownFloat3 { get; private set; }
                public float UnknownFloat4 { get; private set; }
                public float UnknownFloat5 { get; private set; }
                public float UnknownFloat6 { get; private set; }
                public float UnknownFloat7 { get; private set; }
                public int UnknownInt1 { get; private set; }
                public string FileName { get; private set; }
                public int UnknownInt2 { get; private set; }
                public int UnknownInt3 { get; private set; }
                public int UnknownInt4 { get; private set; }
                public List<int> VariableSectionInts { get; private set; }

                private int m_stringOffset;
                private int m_variableSectionCount;

                public void Load(EndianBinaryReader reader)
                {
                    VariableSectionInts = new List<int>();

                    UnknownFloat1 = reader.ReadSingle();
                    UnknownFloat2 = reader.ReadSingle();
                    UnknownFloat3 = reader.ReadSingle();
                    UnknownFloat4 = reader.ReadSingle();
                    UnknownFloat5 = reader.ReadSingle();
                    UnknownFloat6 = reader.ReadSingle();
                    UnknownFloat7 = reader.ReadSingle();
                    UnknownInt1 = reader.ReadInt32();

                    // We'll load the file name from the string table at the end of the file
                    m_stringOffset = reader.PeekReadInt32(); // Peek-read file name offset. We need to stay at the current offset, since the file name offset is relative to it.
                    long curPos = reader.BaseStream.Position; // Save current position to come back later
                    reader.BaseStream.Seek(m_stringOffset, SeekOrigin.Current); // Seek to the start of the file name from the current position
                    FileName = reader.ReadStringUntil('\0'); // Read the file name until the null terminator
                    reader.BaseStream.Seek(curPos + 4, SeekOrigin.Begin); // Return to the instance data at the field right after the file name offset

                    UnknownInt2 = reader.ReadInt32();
                    UnknownInt3 = reader.ReadInt32();
                    UnknownInt4 = reader.ReadInt32();

                    if (UnknownInt1 == 0)
                        return;

                    m_variableSectionCount = reader.ReadInt32();

                    for (int i = 0; i < m_variableSectionCount; i++)
                        VariableSectionInts.Add(reader.ReadInt32());
                }

                public override string ToString()
                {
                    return $"[{ VariableSectionInts.Count }]";
                }
            }

            public List<SecondSectionInstance> SecondSectionInstances { get; private set; }

            public void Load(EndianBinaryReader reader, int sectionCount)
            {
                SecondSectionInstances = new List<SecondSectionInstance>();

                long basePos; // This will be the position of the stream within the offset block

                for (int i = 0; i < sectionCount; i++)
                {
                    basePos = reader.BaseStream.Position; // Update position in the offset block
                    int instanceOffset = reader.ReadInt32(); // Read the instance offset
                    long nextInstOffset = reader.BaseStream.Position; // Save the position to return to after reading instance data

                    reader.BaseStream.Seek(basePos + instanceOffset, SeekOrigin.Begin);
                    SecondSectionInstance inst = new SecondSectionInstance();
                    inst.Load(reader);
                    SecondSectionInstances.Add(inst);
                    reader.BaseStream.Seek(nextInstOffset, SeekOrigin.Begin); // Return to the next instance's offset so we can load it
                }
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
                SecondSectionData.Load(reader, m_secondSectionCount);
            }
        }
    }
}
