using System;

namespace CrystalEngine.DI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor, Inherited = true, AllowMultiple = false)]
    public sealed class InjectAttribute : Attribute
    {

    }
}