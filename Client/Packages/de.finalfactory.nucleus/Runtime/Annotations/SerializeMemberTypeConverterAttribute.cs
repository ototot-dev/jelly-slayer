using System;

namespace FinalFactory.Annotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SerializeMemberTypeConverterAttribute : Attribute
    {
        public readonly Type Type;
        
        public SerializeMemberTypeConverterAttribute(Type type)
        {
            Type = type;
        }
    }
}