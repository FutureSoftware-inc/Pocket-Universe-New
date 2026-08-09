using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CrystalEngine.Services
{
    public static class BinaryPacker
    {
        public static byte[] Pack(Dictionary<string, Dictionary<string, object>> graph)
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream, Encoding.UTF8))
            {
                writer.Write(graph.Count);
                foreach (var providerNode in graph)
                {
                    writer.Write(providerNode.Key);
                    writer.Write(providerNode.Value.Count);
                    foreach (var fieldNode in providerNode.Value)
                    {
                        writer.Write(fieldNode.Key);
                        PackValue(writer, fieldNode.Value);
                    }
                }
            }
            return stream.ToArray();
        }

        public static Dictionary<string, Dictionary<string, object>> Unpack(byte[] bytes)
        {
            Dictionary<string, Dictionary<string, object>> graph = new();
            if (bytes == null || bytes.Length == 0) return graph;
            using (MemoryStream stream = new(bytes))
            {
                using (BinaryReader reader = new(stream, Encoding.UTF8))
                {
                    int providerCount = reader.ReadInt32();

                    for (int i = 0; i < providerCount; i++)
                    {
                        string providerKey = reader.ReadString();
                        int fieldCount = reader.ReadInt32();
                        Dictionary<string, object> providerData = new();
                        for (int j = 0; j < fieldCount; j++)
                        {
                            string fieldName = reader.ReadString();
                            object fieldValue = UnpackValue(reader);
                            providerData[fieldName] = fieldValue;
                        }

                        graph[providerKey] = providerData;
                    }
                }
            }
            return graph;
        }

        private static void PackValue(BinaryWriter writer, object value)
        {
            if (value == null)
            {
                writer.Write((byte)0);
                return;
            }
            switch (value)
            {
                case int i: writer.Write((byte)1); writer.Write(i); break;
                case float f: writer.Write((byte)2); writer.Write(f); break;
                case bool b: writer.Write((byte)3); writer.Write(b); break;
                case string s: writer.Write((byte)4); writer.Write(s); break;
                case double d: writer.Write((byte)5); writer.Write(d); break;
                case long l: writer.Write((byte)6); writer.Write(l); break;
                default:
                    writer.Write((byte)4);
                    writer.Write(value.ToString());
                    break;
            }
        }

        private static object UnpackValue(BinaryReader reader)
        {
            byte typeMarker = reader.ReadByte();
            return typeMarker switch
            {
                0 => null,
                1 => reader.ReadInt32(),
                2 => reader.ReadSingle(),
                3 => reader.ReadBoolean(),
                4 => reader.ReadString(),
                5 => reader.ReadDouble(),
                6 => reader.ReadInt64(),
                _ => null
            };
        }
    }
}