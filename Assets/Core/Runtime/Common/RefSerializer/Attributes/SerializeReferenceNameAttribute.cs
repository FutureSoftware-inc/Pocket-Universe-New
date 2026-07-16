using System;

namespace Crystal.Common
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SerializeReferenceNameAttribute : Attribute
    {
        public string Name { get; }

        public SerializeReferenceNameAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name for the dropdown menu cannot be empty.", nameof(name));
            }
            Name = name;
        }
    }
}