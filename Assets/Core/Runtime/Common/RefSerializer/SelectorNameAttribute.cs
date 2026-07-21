using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class SelectorNameAttribute : Attribute
{
    public string Name { get; }

    public SelectorNameAttribute(string name)
    {
        Name = name;
    }
}
