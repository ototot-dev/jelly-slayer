// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 30.09.2019 : 15:20
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.10.2019 : 15:30
// // ***********************************************************************
// // <copyright file="ILoggerFactory.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;

// ReSharper disable once CheckNamespace
namespace FinalFactory.Logging
{
    public interface ILoggerFactory
    {
        Log CreateLogger(string name);
        Log CreateLogger(Type type);
    }
}