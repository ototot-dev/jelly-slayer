// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 30.09.2019 : 15:20
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.10.2019 : 15:30
// // ***********************************************************************
// // <copyright file="LogBase.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace FinalFactory.Logging
{
    public abstract class LogBase : Log
    {
        public override bool IsTraceEnabled => LogManager.GlobalLogLevel <= LogLevel.Trace && LogLevel <= LogLevel.Trace;
        public override bool IsDebugEnabled => LogManager.GlobalLogLevel <= LogLevel.Debug && LogLevel <= LogLevel.Debug;
        public override bool IsInfoEnabled => LogManager.GlobalLogLevel <= LogLevel.Info && LogLevel <= LogLevel.Info;
        public override bool IsWarnEnabled => LogManager.GlobalLogLevel <= LogLevel.Warn && LogLevel <= LogLevel.Warn;
        public override bool IsErrorEnabled => LogManager.GlobalLogLevel <= LogLevel.Critical && LogLevel <= LogLevel.Critical;
        public override bool IsFatalEnabled => LogManager.GlobalLogLevel <= LogLevel.Fatal && LogLevel <= LogLevel.Fatal;
        public override LogLevel LogLevel { get; set; }

        public override Log SetLogLevel(LogLevel level)
        {
            LogLevel = level;
            return this;
        }
    }
}