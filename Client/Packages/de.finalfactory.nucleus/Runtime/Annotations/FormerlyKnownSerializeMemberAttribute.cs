using System;

namespace FinalFactory.Annotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FormerlyKnownSerializeMemberAttribute : Attribute
    {
        public readonly string Name;

        public FormerlyKnownSerializeMemberAttribute(string name)
        {
            Name = name;
        }
    }
}