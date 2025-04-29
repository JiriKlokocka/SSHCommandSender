using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Formats.Tar;

namespace SSHCommandSender
{

    public class LogWriter
    {
        private string m_exePath = string.Empty;
        //StreamWriter txtWriter;


        public LogWriter()
        {
            try
            {
                m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                using (StreamWriter txtWriter = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                    txtWriter.Write("\r\nProgram start : ");
                    txtWriter.WriteLine("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
                    txtWriter.WriteLine("-------------------------------");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
      
        public void Log(string logMessage)
        {
            try
            {
                using (StreamWriter txtWriter = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                    txtWriter.WriteLine($"{DateTime.Now.ToShortTimeString()}: {logMessage}");
                }
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
