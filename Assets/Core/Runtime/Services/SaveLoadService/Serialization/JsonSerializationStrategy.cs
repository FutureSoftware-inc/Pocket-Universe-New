using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CrystalEngine.Services
{
    public sealed class JsonSerializationStrategy : ISerializationStrategy
    {
        public byte[] Serialize(Dictionary<string, Dictionary<string, object>> stateGraph)
        {
            string jsonString = JsonConverter.ToJson(stateGraph);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        public Dictionary<string, Dictionary<string, object>> Deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return new Dictionary<string, Dictionary<string, object>>();
            }
            string jsonString = Encoding.UTF8.GetString(bytes);
            return JsonConverter.FromJson(jsonString);
        }

        private static class JsonConverter
        {
            public static string ToJson(Dictionary<string, Dictionary<string, object>> graph)
                => JsonUtility.ToJson(graph, true);

            public static Dictionary<string, Dictionary<string, object>> FromJson(string json)
                => JsonUtility.FromJson<Dictionary<string, Dictionary<string, object>>>(json);
        }
    }    
}