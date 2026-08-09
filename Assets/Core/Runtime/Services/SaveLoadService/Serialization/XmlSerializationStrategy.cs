using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace CrystalEngine.Services
{
    public sealed class XmlSerializationStrategy : ISerializationStrategy
    {
        private readonly XmlWriterSettings _writerSettings;
        private readonly XmlReaderSettings _readerSettings;

        public XmlSerializationStrategy()
        {
            _writerSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = System.Text.Encoding.UTF8,
                OmitXmlDeclaration = false
            };
            _readerSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };
        }

        public byte[] Serialize(Dictionary<string, Dictionary<string, object>> stateGraph)
        {
            using (MemoryStream memoryStream = new())
            {
                using (XmlWriter writer = XmlWriter.Create(memoryStream, _writerSettings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("CrystalSaveData");
                    foreach (var providerNode in stateGraph)
                    {
                        writer.WriteStartElement("DataProvider");
                        writer.WriteAttributeString("Key", providerNode.Key);
                        foreach (var fieldNode in providerNode.Value)
                        {
                            writer.WriteStartElement("DataField");
                            writer.WriteAttributeString("Name", fieldNode.Key);
                            string typeName = fieldNode.Value?.GetType().AssemblyQualifiedName ?? "null";
                            writer.WriteAttributeString("Type", typeName);
                            writer.WriteString(fieldNode.Value?.ToString() ?? string.Empty);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                return memoryStream.ToArray();
            }
        }

        public Dictionary<string, Dictionary<string, object>> Deserialize(byte[] bytes)
        {
            Dictionary<string, Dictionary<string, object>> rootGraph = new();
            if (bytes == null || bytes.Length == 0) return rootGraph;
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                using (XmlReader reader = XmlReader.Create(memoryStream, _readerSettings))
                {
                    string currentProviderKey = null;
                    Dictionary<string, object> currentProviderData = null;
                    while (reader.Read())
                    {
                        if (reader.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }
                        if (reader.Name == "DataProvider")
                        {
                            currentProviderKey = reader.GetAttribute("Key");
                            currentProviderData = new Dictionary<string, object>();
                            rootGraph[currentProviderKey] = currentProviderData;
                        }
                        else if (reader.Name == "DataField" && currentProviderData != null)
                        {
                            string fieldName = reader.GetAttribute("Name");
                            string typeName = reader.GetAttribute("Type");
                            reader.Read();
                            string rawValue = reader.Value;
                            if (typeName != "null" && !string.IsNullOrEmpty(fieldName))
                            {
                                Type targetType = Type.GetType(typeName);
                                if (targetType != null)
                                {
                                    object convertedValue = Convert.ChangeType(rawValue, targetType);
                                    currentProviderData[fieldName] = convertedValue;
                                }
                            }
                        }
                    }
                }
            }
            return rootGraph;
        }
    }
}