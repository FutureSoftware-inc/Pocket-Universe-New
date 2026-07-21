using System;

namespace Crystal.Common
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SelectorIconAttribute : Attribute
    {
        public string IconName { get; }

        public SelectorIconAttribute(string iconName)
        {
            IconName = iconName;
        }
    }
}
