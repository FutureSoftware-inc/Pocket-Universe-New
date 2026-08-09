using System.Collections.Generic;

namespace CrystalEngine.Services
{
    public interface ISerializationStrategy
    {
        byte[] Serialize(Dictionary<string, Dictionary<string, object>> stateGraph);
        Dictionary<string, Dictionary<string, object>> Deserialize(byte[] bytes);
    }
}