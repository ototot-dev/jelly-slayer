// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : .. : 
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.06.2020 : 14:48
// // ***********************************************************************
// // <copyright file="IProperty.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

namespace FinalFactory.Observable
{
    public interface IProperty : IReadOnlyProperty
    {
        new object Value { get; set; }

        void Refire();
    }

    public interface IProperty<T> : IProperty, IReadOnlyProperty<T>
    {
        new T Value { get; set; }
    }
}