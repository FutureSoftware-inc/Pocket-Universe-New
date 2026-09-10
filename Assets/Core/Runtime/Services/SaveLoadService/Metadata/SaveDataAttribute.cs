using System;

namespace CrystalEngine.Services
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class SaveDataAttribute : Attribute
    {
        public SerializationFormat Format { get; }
        public StorageType Storage {  get; }

        public SaveDataAttribute(SerializationFormat format = SerializationFormat.None, StorageType storage = StorageType.LocalFile)
        {
            Format = format;
            Storage = storage;
        }
    }
}