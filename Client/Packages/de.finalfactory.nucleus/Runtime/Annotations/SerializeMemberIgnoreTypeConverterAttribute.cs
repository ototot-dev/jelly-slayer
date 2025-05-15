// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 28.11.2021 : 20:09
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 28.11.2021 : 20:09
// // ***********************************************************************
// // <copyright file="SerializeMemberIgnoreTypeConverterAttribute.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;

namespace FinalFactory.Annotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SerializeMemberIgnoreTypeConverterAttribute : Attribute
    {
        
    }
}