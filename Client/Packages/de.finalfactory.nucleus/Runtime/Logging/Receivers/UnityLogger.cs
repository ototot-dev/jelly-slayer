// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 04.08.2019 : 10:37
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 04.08.2019 : 11:09
// // ***********************************************************************
// // <copyright file="UnityLogger.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

#if UNITY_5_3_OR_NEWER

using System;

namespace FinalFactory.Logging.Receivers
{
    public class UnityLogger : ILogReceiver
    {
        public void Push(LogMessage message) => GetLogMethod(message.Level)($"#{message.ToString()}");

        private static Action<object> GetLogMethod(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Off:
                    return null;
                case LogLevel.Emergency:
                case LogLevel.Fatal:
                case LogLevel.Alert:
                case LogLevel.Critical:
                case LogLevel.Severe:
                    return UnityEngine.Debug.LogError;
                case LogLevel.Warn:
                    return UnityEngine.Debug.LogWarning;
                case LogLevel.Notice:
                case LogLevel.Info:
                case LogLevel.Debug:
                case LogLevel.Fine:
                case LogLevel.Trace:
                case LogLevel.Finer:
                case LogLevel.Verbose:
                case LogLevel.Finest:
                case LogLevel.All:
                    return UnityEngine.Debug.Log;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}

#endif