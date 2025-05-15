#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   Company.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

namespace FinalFactory
{
    public class Company
    {
        public static readonly Company FinalFactory = new Company("FinalFactory", "Final Factory", "Final", "FF", "https://finalfactory.de");
        
        public readonly string Name;
        public readonly string DisplayName;
        public readonly string ShortName;
        public readonly string Initials;
        public readonly string Website;

        public Company(string name, string displayName, string shortName, string initials, string website)
        {
            Name = name;
            DisplayName = displayName;
            ShortName = shortName;
            Initials = initials;
            Website = website;
        }
    }
}