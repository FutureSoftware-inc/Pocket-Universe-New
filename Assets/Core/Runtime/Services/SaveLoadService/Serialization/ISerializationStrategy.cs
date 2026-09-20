using System.Collections.Generic;

namespace CrystalEngine.Services
{
    internal interface ISerializationStrategy
    {
        byte[] Serialize(Dictionary<string, Dictionary<string, object>> stateGraph);
        Dictionary<string, Dictionary<string, object>> Deserialize(byte[] bytes);
    }
}