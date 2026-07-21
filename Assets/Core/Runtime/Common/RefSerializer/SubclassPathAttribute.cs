using System;

namespace Crystal.Common
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SubclassPathAttribute : Attribute
    {
        public string Path { get; }

        public SubclassPathAttribute(string path)
        {
            Path = path;
        }
    }
}
