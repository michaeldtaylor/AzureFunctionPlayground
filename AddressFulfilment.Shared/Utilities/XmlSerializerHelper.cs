using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AddressFulfilment.Shared.Extensions;

namespace AddressFulfilment.Shared.Utilities
{
    public static class XmlSerializerHelper
    {
        private static readonly object SyncRoot = new object();

        public static T CreateInstance<T>(Stream stream, string rootNode)
        {
            using (var reader = new StreamReader(stream))
            {
                var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootNode));

                return (T)serializer.Deserialize(reader);
            }
        }

        public static T CreateInstance<T>(string serializedType)
        {
            try
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(serializedType)))
                {
                    var reader = XmlDictionaryReader.CreateTextReader(memoryStream, Encoding.UTF8, new XmlDictionaryReaderQuotas(), null);
                    var serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                throw;
            }
        }

        public static string SerializeInstance(object instance, IDictionary<string, string> namespaceDictionary = null, bool formatted = true)
        {
            lock (SyncRoot)
            {
                var output = new MemoryStream();
                var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = formatted };

                using (var xmlWriter = XmlWriter.Create(output, settings))
                {
                    var serializer = new XmlSerializer(instance.GetType());
                    var namespaces = new XmlSerializerNamespaces();

                    if (namespaceDictionary != null)
                    {
                        foreach (var kp in namespaceDictionary)
                        {
                            namespaces.Add(kp.Key, kp.Value);
                        }
                    }
                    else
                    {
                        namespaces.Add(string.Empty, string.Empty);
                    }

                    serializer.Serialize(xmlWriter, instance, namespaces);
                }

                output.Rewind();

                var reader = new StreamReader(output);

                return reader.ReadToEnd();
            }
        }

        public static XmlCDataSection GetCDataSection(string text)
        {
            var doc = new XmlDocument();
            return doc.CreateCDataSection(text ?? string.Empty);
        }
    }
}