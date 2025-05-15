// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 09.12.2020 : 14:27
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 09.12.2020 : 14:27
// // ***********************************************************************
// // <copyright file="SerializeType.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;

namespace FinalFactory.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SerializeTypeAttribute : Attribute
    {
        public readonly string Identifier;
        public readonly string[] OldIdentifiers;

        public SerializeTypeAttribute(string identifier = null, params string[] oldIdentifiers)
        {
            Identifier = identifier;
            OldIdentifiers = oldIdentifiers;
        }
    }
}