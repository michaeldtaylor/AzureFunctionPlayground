using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AddressFulfilment.Shared.Extensions
{
    public static class JsonExtensions
    {
        public static readonly JsonSerializerSettings DefaultSettings = CreateSettings();
        public static readonly JsonSerializerSettings DefaultTypedSettings = CreateTypedSettings();

        public static string ToJson(this object obj, Formatting formatting = Formatting.Indented,
            JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(obj, formatting, settings ?? DefaultSettings);
        }

        public static string ToJson(this object obj, Type objectType, Formatting formatting = Formatting.Indented,
            JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(obj, objectType, formatting, settings ?? DefaultSettings);
        }

        public static T FromJson<T>(this string value, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject<T>(value, settings ?? DefaultSettings);
        }

        public static object FromJson(this string value, Type type, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject(value, type, settings ?? DefaultSettings);
        }

        public static T FromJson<T>(this Stream stream, JsonSerializerSettings settings = null)
        {
            var serializer = JsonSerializer.Create(settings ?? DefaultSettings);

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

        public static string ToFullTypeJson(this object value, Formatting formatting = Formatting.Indented)
        {
            return ToJson(value, formatting, DefaultTypedSettings);
        }

        public static string ToFullTypeJson(this object value, Type propertyType,
            Formatting formatting = Formatting.Indented)
        {
            return ToJson(value, propertyType, formatting, DefaultTypedSettings);
        }

        public static T FromFullTypeJson<T>(this string value)
        {
            return FromJson<T>(value, DefaultTypedSettings);
        }

        public static T FromFullTypeJson<T>(this Stream stream)
        {
            return FromJson<T>(stream, DefaultTypedSettings);
        }

        public static object FromFullTypeJson(this string value, Type type)
        {
            return FromJson(value, type, DefaultTypedSettings);
        }

        public static T DeepCloneViaJson<T>(this T value)
        {
            var json = value.ToFullTypeJson();

            return (T) json.FromFullTypeJson(value.GetType());
        }

        private static JsonSerializerSettings CreateSettings()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                ContractResolver = new PrivateSetterContractResolver(),
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            settings.Converters.Add(new StringEnumConverter());

            return settings;
        }

        private static JsonSerializerSettings CreateTypedSettings()
        {
            var settings = CreateSettings();

            settings.TypeNameHandling = TypeNameHandling.Auto;

            return settings;
        }

        private class PrivateSetterContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);

                if (prop.Writable)
                {
                    return prop;
                }

                prop.Writable = member.IsPropertyWithSetter();

                return prop;
            }
        }
    }
}
