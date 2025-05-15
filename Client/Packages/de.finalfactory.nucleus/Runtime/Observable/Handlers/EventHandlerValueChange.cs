// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 07.12.2020 : 19:21
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 07.12.2020 : 19:21
// // ***********************************************************************
// // <copyright file="a.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

namespace FinalFactory.Observable.Handlers
{
    public delegate void EventHandlerValueChange<in TData>(TData oldValue, TData newValue);
    public delegate void EventHandlerValueChange<in TOrigin, in TData>(TOrigin sender, TData oldValue, TData newValue);
}