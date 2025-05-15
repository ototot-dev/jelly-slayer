// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 30.09.2019 : 15:20
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.10.2019 : 15:30
// // ***********************************************************************
// // <copyright file="LogManager.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using FinalFactory.Logging.Receivers;
using JetBrains.Annotations;

namespace FinalFactory.Logging
{
    [PublicAPI]
    public static class LogManager
    {
        public const int MaxLogEntries = 100000;
        private static readonly Log Log;
        private static ILoggerFactory _factory;

        [PublicAPI]
        public static LogLevel GlobalLogLevel { get; set; } = LogLevel.All;

        public static ILogReceiver[] LogStack { get; private set; } = Array.Empty<ILogReceiver>();

        private static List<LogMessage> _logMessages = new(10000);

        public static ILogContextProvider ContextProvider { get; set; }
        
        [PublicAPI] public static ILoggerFactory Factory { get; set; } = new DefaultLoggerFactory();

        [PublicAPI]
        public static Log GetLogger(string name) => Factory.CreateLogger(name);

        [PublicAPI]
        public static Log GetLogger(Type type) => Factory.CreateLogger(type);

        static LogManager()
        {
#if UNITY_5_3_OR_NEWER
            AddLogReceiver(new UnityLogger());
#endif
            
            Log = GetLogger(typeof(AppDomain));
            AppDomain.CurrentDomain.UnhandledException += LogAnyExceptions;
        }
        
        private static void LogAnyExceptions(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error((Exception)e.ExceptionObject, "unhandled");
        }
        
        public static void AddLogReceiver(ILogReceiver receiver, bool pushOldMessages = true)
        {
            var list = LogStack.ToList();
            list.Add(receiver);
            if (pushOldMessages)
            {
                foreach (var message in _logMessages)
                {
                    receiver.Push(message);
                }
            }
            LogStack = list.ToArray();
        }
        
        public static void PushLog(LogMessage message)
        {
            if (_logMessages.Count + 1 >= MaxLogEntries)
            {
                _logMessages.RemoveAt(0);
            }
            _logMessages.Add(message);
            for (var i = 0; i < LogStack.Length; i++)
            {
                LogStack[i].Push(message);
            }
        }
    }
}