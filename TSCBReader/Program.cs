using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSCBReader.src;

namespace TSCBReader
{
    class Program
    {
        static void Main(string[] args)
        {
            TSCB file = new TSCB();
            file.LoadTSCB(@"D:\Dropbox\Breath of the Wild Hacking\MainField.tscb");
        }
    }
}
