using System;
using System.Text;
using System.IO;

namespace Player
{
    public enum EventType
    {
        Info = 0,
        Success = 1,
        Warning = 2,
        Error = 3,
        FatalSystemDestroyingUltraError = 9001
    }

    class Log
    {
        static bool _canlog = true;
        public static bool CanLog
        {
            get
            {
                return _canlog;
            }
        }

        static string logfile = Path.Combine(Prop.AppFolder, "Player.log");

        static StreamWriter _logwriter;
        public static StreamWriter LogWriter
        {
            get
            {
                if (!CanLog)
                    return null;

                if (_logwriter == null)
                {
                    try
                    {                        
                        bool overwrite = false;
                        if (File.Exists(logfile))
                        {
                            //Logfile erstellen oder zum Anhängen öffnen
                            FileInfo fi = new FileInfo(logfile);
                            if (fi.Length >= 1000000)
                            {
                                overwrite = true;                                
                            }
                        }
                        _logwriter = new StreamWriter(logfile, !overwrite, Encoding.Unicode);
                        _logwriter.AutoFlush = true;
                        Write("Log geöffnet. Overwrite: " + overwrite, EventType.Info);
                    }
                    catch (Exception ex)
                    {
                        _canlog = false;
                        System.Windows.MessageBox.Show("Fehler beim erstellen der Logdatei!" + Environment.NewLine + ex.ToString(), "Fehler!");
                    }
                }
                return _logwriter;
            }
        }
        

        public static void Write(string message, EventType type)
        {
            if (LogWriter != null && LogWriter.BaseStream != null && LogWriter.BaseStream.CanWrite && CanLog)
            {
                LogWriter.WriteLine("[{0}]    [{1}]    {2}", DateTime.Now.ToString(@"dd\.MM\.yyyy HH\:mm\:ss\.ff"), type, message);
            }
            else
                Console.WriteLine("LOGEINTRAG NICHT SCHREIBBAR!");

            Console.WriteLine(string.Format("{0}: {1} ({2})", DateTime.Now, message, type));
        }
    }
}