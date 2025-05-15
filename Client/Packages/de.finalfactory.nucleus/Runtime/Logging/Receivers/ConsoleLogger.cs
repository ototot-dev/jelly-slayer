// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 30.09.2019 : 15:20
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.10.2019 : 15:30
// // ***********************************************************************
// // <copyright file="ConsoleLogger.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;

// ReSharper disable once CheckNamespace
namespace FinalFactory.Logging
{
    public class ConsoleLogger : ILogReceiver
    {
        public void Push(LogMessage message) => Console.WriteLine(message.ToString(true));
    }
}