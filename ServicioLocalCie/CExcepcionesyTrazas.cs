using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Configuration;

namespace ServicioLocalCie
{
    class CExcepcionesyTrazas
    {
        private static string StrPathLog
        {
            get
            {
                //para esto se debe agregar esa llave en el app.config d esta forma:
                /*<appSettings>
                          <add key="strPathLog" value="D:\LogServicio" />
                 </appSettings>
                */
                var value = ConfigurationManager.AppSettings["strPathLog"];
                return value;
            }
        }

        public static void ExcepcionyTraza(Exception exception)
        {
            try
            {
                if (!Directory.Exists(StrPathLog))
                    Directory.CreateDirectory(StrPathLog);

                string path = string.Format("{0}\\{1}", StrPathLog, "LogError.txt");

                TextWriter tw = new StreamWriter(path, true);
                tw.WriteLine("");
                tw.WriteLine("Error: " + DateTime.Now + ": " + exception);
                tw.WriteLine("");
                tw.Close();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Application", "Excepción: " + ex.Message);
            }
        }
        public static void ExcepcionyTraza(string mensaje)
        {
            try
            {
                if (!Directory.Exists(StrPathLog))
                    Directory.CreateDirectory(StrPathLog);

                string path = string.Format("{0}\\{1}", StrPathLog, "LogInfo.txt");

                TextWriter tw = new StreamWriter(path, true);
                tw.WriteLine("{0}, {1}", mensaje, DateTime.Now);
                tw.Close();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Application", "Excepción: " + ex.Message);
            }
        }
    }
}
