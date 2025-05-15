using System;

namespace FinalFactory.Logging
{
    public class LogBit : LogBase
    {
        private readonly string _name;

        public LogBit(string name) => _name = name;

        public LogBit(Type type) => _name = type.Name;

        public override void AddLogEntry(LogLevel level, object message)
        {
            LogManager.PushLog(new LogMessage(level, _name, message.ToString()));
        }

        public override void AddLogEntry(LogLevel level, Exception exception, object message = null)
        {
            LogManager.PushLog(new LogMessage(level, _name, $"{message}{Environment.NewLine}{exception}"));
        }

        public override void AddLogEntry(LogLevel level, IFormatProvider provider, string format, params object[] args)
        {
            LogManager.PushLog(new LogMessage(level, _name, string.Format(provider, format, args)));
        }
    }
}