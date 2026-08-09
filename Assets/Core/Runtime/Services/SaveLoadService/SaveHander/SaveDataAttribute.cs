using System;

namespace CrystalEngine.Services
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class SaveDataAttribute : Attribute
    {

    }
}