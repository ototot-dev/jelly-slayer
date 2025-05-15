using System;
using System.IO;
using FinalFactory.Utilities;
using UnityEngine;

namespace FinalFactory.Logging.Receivers
{
    public class FileLogger : ILogReceiver
    {
        private static readonly Log Log = LogManager.GetLogger(typeof(FileLogger));
        private readonly StreamWriter _writer;

        public FileLogger()
        {
            var folderPath = Path.Combine(Application.persistentDataPath, "Logs");
            Directory.CreateDirectory(folderPath);
            var fileName = Path.Combine(folderPath, Application.productName.SanitizeFileName() + "-" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".log");
            Log.Info("Log File Path: " + fileName);
            _writer = new StreamWriter(fileName);
        }

        public void Push(LogMessage message)
        {
            _writer.WriteLine(message.ToString(true));
#if FINALLOG_STACKTRACE
            _writer.WriteLine(message.Trace.ToString());
#endif
            _writer.Flush();
        }
    }
}