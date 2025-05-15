// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 30.09.2019 : 15:20
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.10.2019 : 15:30
// // ***********************************************************************
// // <copyright file="ILog.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;
using System.Diagnostics;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace FinalFactory.Logging
{
    public abstract class Log
    {
        public abstract bool IsTraceEnabled { get; }
        public abstract bool IsDebugEnabled { get; }
        public abstract bool IsInfoEnabled { get; }
        public abstract bool IsWarnEnabled { get; }
        public abstract bool IsErrorEnabled { get; }
        public abstract bool IsFatalEnabled { get; }
        public abstract LogLevel LogLevel { get; set; }
#if FINALLOG_DISABLE_TRACE
        [Conditional("I DO NOT EXIST")]
#endif
        public void Trace(object message)
        {
            if (IsTraceEnabled) AddLogEntry(LogLevel.Trace, message);
        }
#if FINALLOG_DISABLE_TRACE
        [Conditional("I DO NOT EXIST")]
#endif
        public void Trace(Exception exception, object message = null)
        {
            if (IsTraceEnabled) AddLogEntry(LogLevel.Trace, exception, message);
        }
#if FINALLOG_DISABLE_TRACE
        [Conditional("I DO NOT EXIST")]
#endif
        public void Trace(IFormatProvider provider, string format, params object[] args)
        {
            if (IsTraceEnabled) AddLogEntry(LogLevel.Trace, provider, format, args);
        }
#if FINALLOG_DISABLE_DEBUG
        [Conditional("I DO NOT EXIST")]
#endif
        public void Debug(object message)
        {
            if (IsDebugEnabled) AddLogEntry(LogLevel.Debug, message);
        }

#if FINALLOG_DISABLE_DEBUG
        [Conditional("I DO NOT EXIST")]
#endif
        public void Debug(Exception exception, object message = null)
        {
            if (IsDebugEnabled) AddLogEntry(LogLevel.Debug, exception, message);
        }

#if FINALLOG_DISABLE_DEBUG
        [Conditional("I DO NOT EXIST")]
#endif
        public void Debug(IFormatProvider provider, string format, params object[] args)
        {
            if (IsDebugEnabled) AddLogEntry(LogLevel.Debug, provider, format, args);
        }

#if FINALLOG_DISABLE_INFO
        [Conditional("I DO NOT EXIST")]
#endif
        public void Info(object message)
        {
            if (IsInfoEnabled) AddLogEntry(LogLevel.Info, message);
        }

#if FINALLOG_DISABLE_INFO
        [Conditional("I DO NOT EXIST")]
#endif
        public void Info(Exception exception, object message = null)
        {
            if (IsInfoEnabled) AddLogEntry(LogLevel.Info, exception, message);
        }

#if FINALLOG_DISABLE_INFO
        [Conditional("I DO NOT EXIST")]
#endif
        public void Info(IFormatProvider provider, string format, params object[] args)
        {
            if (IsInfoEnabled) AddLogEntry(LogLevel.Info, provider, format, args);
        }

#if FINALLOG_DISABLE_WARN
        [Conditional("I DO NOT EXIST")]
#endif
        public void Warn(object message)
        {
            if (IsWarnEnabled) AddLogEntry(LogLevel.Warn, message);
        }

#if FINALLOG_DISABLE_WARN
        [Conditional("I DO NOT EXIST")]
#endif
        public void Warn(Exception exception, object message = null)
        {
            if (IsWarnEnabled) AddLogEntry(LogLevel.Warn, exception, message);
        }

#if FINALLOG_DISABLE_WARN
        [Conditional("I DO NOT EXIST")]
#endif
        public void Warn(IFormatProvider provider, string format, params object[] args)
        {
            if (IsWarnEnabled) AddLogEntry(LogLevel.Warn, provider, format, args);
        }

#if FINALLOG_DISABLE_ERROR
        [Conditional("I DO NOT EXIST")]
#endif
        public void Error(object message)
        {
            if (IsErrorEnabled) AddLogEntry(LogLevel.Critical, message);
        }

#if FINALLOG_DISABLE_ERROR
        [Conditional("I DO NOT EXIST")]
#endif
        public void Error(Exception exception, object message = null)
        {
            if (IsErrorEnabled) AddLogEntry(LogLevel.Critical, exception, message);
        }

#if FINALLOG_DISABLE_ERROR
        [Conditional("I DO NOT EXIST")]
#endif
        public void Error(IFormatProvider provider, string format, params object[] args)
        {
            if (IsErrorEnabled) AddLogEntry(LogLevel.Critical, provider, format, args);
        }

#if FINALLOG_DISABLE_FATAL
        [Conditional("I DO NOT EXIST")]
#endif
        public void Fatal(object message)
        {
            if (IsFatalEnabled) AddLogEntry(LogLevel.Fatal, message);
        }

#if FINALLOG_DISABLE_FATAL
        [Conditional("I DO NOT EXIST")]
#endif
        public void Fatal(Exception exception, object message = null)
        {
            if (IsFatalEnabled) AddLogEntry(LogLevel.Fatal, exception, message);
        }

#if FINALLOG_DISABLE_FATAL
        [Conditional("I DO NOT EXIST")]
#endif
        public void Fatal(IFormatProvider provider, string format, params object[] args)
        {
            if (IsFatalEnabled) AddLogEntry(LogLevel.Fatal, provider, format, args);
        }
        
        public abstract void AddLogEntry(LogLevel level, object message);
        public abstract void AddLogEntry(LogLevel level, Exception exception, object message = null);
        public abstract void AddLogEntry(LogLevel level, IFormatProvider provider, string format, params object[] args);
        
        public abstract Log SetLogLevel(LogLevel level);
    }
}