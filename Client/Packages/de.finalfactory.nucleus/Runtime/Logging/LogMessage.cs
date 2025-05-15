using System;
using System.Diagnostics;

namespace FinalFactory.Logging
{
    public readonly struct LogMessage
    {
        public readonly LogLevel Level;
        public readonly string Sender;
        public readonly string Message;
        public readonly DateTime Time;
        public readonly string Context;
#if FINALLOG_STACKTRACE
        public readonly StackTrace Trace;
#endif

        public LogMessage(LogLevel level, string sender, string message)
        {
            Level = level;
            Sender = sender;
            Message = message;
            Time = DateTime.Now;
#if FINALLOG_STACKTRACE
            Trace = new StackTrace(2, true);
#endif
            if (LogManager.ContextProvider != null)
            {
#if FINALLOG_STACKTRACE
                Context = LogManager.ContextProvider.DetermineContext(Trace);
#else
                Context = LogManager.ContextProvider.DetermineContext(null);
#endif
            }
            else
            {
                Context = "Game";
            }
        }

        public string ToString(bool includeTime = false)
        {
            var txt = $"[{Level}][{Context}][{Sender}] {Message}";
            if (includeTime)
                txt = $"[{Time:yy-MM-dd--hh-mm-ss:fff}]{txt}";
            return txt;
        }
    }
}