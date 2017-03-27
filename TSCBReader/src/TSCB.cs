using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GameFormatReader.Common;
using WArchiveTools.Compression;

namespace TSCBReader.src
{
    public class TSCB
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
        }

        public class SecondSectionInstance
        {

        }

        public List<FirstSectionInstance> FirstSectionInstances { get; private set; }
        public List<SecondSectionInstance> SecondSectionInstances { get; private set; }
    }
}
