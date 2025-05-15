using System;

namespace FinalFactory.Logging
{
    public class DefaultLoggerFactory : ILoggerFactory
    {
        public Log CreateLogger(string name) => new LogBit(name);

        public Log CreateLogger(Type type) => new LogBit(type);
    }
}