// // ***********************************************************************
// // Assembly         : Tagger.Plugins
// // Author           : HeiaSamahi
// // Created          : 14.05.2017 : 16:19
// //
// // Last Modified By : Heia Samahi
// // Last Modified On : 26.05.2017 : 10:40
// // ***********************************************************************
// // <copyright file="AdvancedSearch.cs" company="Heikyu">
// //     Copyright (c) Heikyu. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using Finalfactory.Tagger;
using UnityEngine;

namespace Plugins.HeiKyu.Tagger.Examples
{
    public class AdvancedSearchExample
    {
        /// <summary>
        /// This method is intended to illustrate how the TaggerAdvancedSearch is used.
        /// </summary>
        public void Search()
        {
            // Logical... this is what we want to do...
            // Red && (Small || Large && Blue)

            // We make sure that the tags are existing... this is only necesseary if the tags are not already created in editor time.
            TaggerSystem.Data.AddTag("Red", "Color");
            TaggerSystem.Data.AddTag("Blue", "Color");
            TaggerSystem.Data.AddTag("Small", "Size");
            TaggerSystem.Data.AddTag("Large", "Size");

            // METHOD #1
            // Combine TaggerAdvancedSearch classes

            // Then we create our searches
            TaggerAdvancedSearch red = new TaggerAdvancedSearch();
            TaggerAdvancedSearch small = new TaggerAdvancedSearch();
            TaggerAdvancedSearch large = new TaggerAdvancedSearch();
            TaggerAdvancedSearch blue = new TaggerAdvancedSearch();

            // Set the tags to the searches..
            red.SetTags("Red");
            small.SetTags("Small");
            large.SetTags("Large");
            blue.SetTags("Blue");

            // You can use & and | operators.. Please note the brackets!
            (red & (small | (large & blue))).Research();

            // You can reduce the brackets to ... if you understand the processing order of the operators.
            (red & (small | large & blue)).Research();

            // Or ... you can use the Methods...
            red.And(small.Or(large.And(blue))).Research();

            // METHOD #2
            // Use mostly strings.

            // Begin with your search.
            TaggerAdvancedSearch search = new TaggerAdvancedSearch();

            // Set first tags.
            search.SetTags("Red");

            // Then setup search Logic.
            // Equals logic above.
            search.And("Small").Or("Large").And("Blue");
            
            // Now call research and you become all gameobjects.
            search.Research();

            // NOTE #1: One TaggerAdvancedSearch can only contain one child search.
            // NOTE #2: One TaggerAdvancedSearch can hold multiple tags. They act as And
            // Example: We are simplifing the search above.
            red = new TaggerAdvancedSearch();
            small = new TaggerAdvancedSearch();
            TaggerAdvancedSearch largeAndBlue = new TaggerAdvancedSearch();
            
            red.SetTags("Red");
            small.SetTags("Small");
            largeAndBlue.SetTags("Large", "Blue");

            // METHOD #1
            (red & (small | largeAndBlue)).Research();

            // METHOD #2
            search = new TaggerAdvancedSearch();

            // Set first tags.
            search.SetTags("Red");

            // Then setup search Logic.
            // Equals logic above.
            search.And("Small").Or("Large", "Blue");

            // Now call research and you become all gameobjects.
            search.Research();

            // All examples give the same result. There are only different ways. Find the most logical for you.
        }
    }
}