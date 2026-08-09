using System.Collections.Generic;

namespace CrystalEngine.Services
{
    public sealed class BinarySerializationStrategy : ISerializationStrategy
    {
        public byte[] Serialize(Dictionary<string, Dictionary<string, object>> stateGraph)
        {
            return BinaryPacker.Pack(stateGraph);
        }

        public Dictionary<string, Dictionary<string, object>> Deserialize(byte[] bytes)
        {
            return BinaryPacker.Unpack(bytes);
        }
    }
}
