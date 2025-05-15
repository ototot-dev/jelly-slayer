// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 30.09.2019 : 15:20
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.10.2019 : 15:30
// // ***********************************************************************
// // <copyright file="LogLevel.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace FinalFactory.Logging
{
    public enum LogLevel
    {
        [PublicAPI] Off = int.MaxValue,

        [PublicAPI] Emergency = 120000,

        [PublicAPI] Fatal = 110000,

        [PublicAPI] Alert = 100000,

        [PublicAPI] Critical = 90000,

        [PublicAPI] Severe = 80000,

        [PublicAPI] Warn = 60000,

        [PublicAPI] Notice = 50000,

        [PublicAPI] Info = 40000,

        [PublicAPI] Debug = 30000,

        [PublicAPI] Fine = 25000,

        [PublicAPI] Trace = 20000,

        [PublicAPI] Finer = 15000,

        [PublicAPI] Verbose = 10000,

        [PublicAPI] Finest = 5000,

        [PublicAPI] All = 0
    }
}