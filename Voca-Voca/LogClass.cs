using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voca_Voca
{
    static class LogClass
    {
        public static void LogWrite(string logtxt)
        {
            FileStream fs = new FileStream("log.tmp", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLineAsync(DateTime.Now + " " + logtxt);
            sw.Close();
            fs.Close();
        }
    }
}
