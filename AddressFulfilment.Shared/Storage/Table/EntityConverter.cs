using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using AddressFulfilment.Shared.Extensions;
using AddressFulfilment.Shared.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace AddressFulfilment.Shared.Storage.Table
{
    public static class EntityConverter
    {
        public static readonly DateTime MinimumAzureTableStorageDate = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts the given 'poco' in to a <see cref="DynamicTableEntity" /> that can be persisted to
        /// Azure Table Storage.
        /// </summary>
        /// <param name="poco">The object to convert</param>
        /// <param name="partitionKey">Function to Select Partition Key Object</param>
        /// <param name="rowKey">Function to Select Row key from Object</param>
        /// <returns>Dynamic Table Entity to be stored in Azure Table Storage</returns>
        public static DynamicTableEntity ConvertToDynamicTableEntity(
            object poco,
            string partitionKey = null,
            string rowKey = null)
        {
            var dynamicTableEntity = new DynamicTableEntity
            {
                RowKey = rowKey,
                PartitionKey = partitionKey,
                Properties = new Dictionary<string, EntityProperty>()
            };

            if (poco is ITableEntity tableEntity)
            {
                dynamicTableEntity.ETag = tableEntity.ETag;
            }

            foreach (var propertyInfo in GetSerializableProperties(poco.GetType()))
            {
                bool shouldIgnore;

                if (poco is ITableEntity)
                {
                    shouldIgnore = propertyInfo.Name == nameof(ITableEntity.PartitionKey)
                                   || propertyInfo.Name == nameof(ITableEntity.RowKey)
                                   || propertyInfo.Name == nameof(ITableEntity.Timestamp)
                                   || propertyInfo.Name == nameof(ITableEntity.ETag);
                }
                else
                {
                    shouldIgnore = propertyInfo.GetCustomAttribute<PartitionKeyAttribute>() != null
                                   || propertyInfo.GetCustomAttribute<RowKeyAttribute>() != null;
                }

                if (!shouldIgnore)
                {
                    var entityProperty = CreateEntityProperty(propertyInfo, poco);
                    var storageProperty = propertyInfo.GetCustomAttribute<StoragePropertyAttribute>();
                    var name = storageProperty == null ? propertyInfo.Name : storageProperty.Name;

                    dynamicTableEntity.Properties.Add(name, entityProperty);
                }
            }

            return dynamicTableEntity;
        }

        /// <summary>
        ///  Converts a dynamic table entity to .NET Object
        /// </summary>
        /// <typeparam name="TOutput">The type of object to convert to</typeparam>
        /// <param name="entity">Dynamic table Entity</param>
        /// <returns>The deserialised object built from table entity.</returns>
        public static TOutput ConvertTo<TOutput>(DynamicTableEntity entity)
        {
            return (TOutput)EntityConverter<TOutput>.TableEntityResolver(entity.PartitionKey, entity.RowKey, entity.Timestamp, entity.Properties, entity.ETag);
        }

        public static EntityProperty CreateEntityProperty(Type propertyType, object value)
        {
            // We care about the actual type, so strip away the 'nullable wrapper' if present
            propertyType = NonNullableType(propertyType);

            // We want to use the _actual_ type of the value, which may not be the prop type (i.e. if declared as object)
            var type = NonNullableType(value?.GetType() ?? propertyType);

            if (type == typeof(byte[]))
            {
                return new EntityProperty((byte[])value);
            }

            if (type == typeof(bool))
            {
                return new EntityProperty((bool?)value);
            }

            if (type == typeof(DateTime))
            {
                return new EntityProperty(CorrectDateTimeForAzure((DateTime?)value));
            }

            if (type == typeof(DateTimeOffset))
            {
                return new EntityProperty((DateTimeOffset?)value);
            }

            if (type == typeof(float))
            {
                return new EntityProperty((float?)value);
            }

            if (type == typeof(double))
            {
                return new EntityProperty((double?)value);
            }

            if (type == typeof(long))
            {
                return new EntityProperty((long?)value);
            }

            if (type == typeof(int))
            {
                return new EntityProperty((int?)value);
            }

            if (type == typeof(Guid))
            {
                return new EntityProperty((Guid?)value);
            }

            if (type == typeof(string))
            {
                return new EntityProperty((string)value);
            }

            if (type.IsEnum)
            {
                return new EntityProperty(value?.ToString());
            }

            // Anything that can not be handled natively will be stored as a JSON blob. We care about
            // using the _propertyType_ and not the actual type to ensure we output the required
            // JSON type expression
            return new EntityProperty(value.ToFullTypeJson(propertyType));
        }

        private static Type NonNullableType(Type propertyType)
        {
            var underlyingType = Nullable.GetUnderlyingType(propertyType);

            return underlyingType != null ? underlyingType : propertyType;
        }

        public static IEnumerable<PropertyInfo> GetSerializableProperties(Type type)
        {
            return type.GetProperties()
                .Where(p => !p.HasAttribute<IgnorePropertyAttribute>() || p.HasAttribute<PartitionKeyAttribute>() || p.HasAttribute<RowKeyAttribute>())
                .Where(p => !p.GetIndexParameters().Any());
        }

        private static EntityProperty CreateEntityProperty(PropertyInfo prop, object poco)
        {
            try
            {
                var value = prop.GetValue(poco);
                var propertyType = prop.PropertyType;

                return CreateEntityProperty(propertyType, value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not create entity property from {prop.DeclaringType.Name}.{prop.Name}", ex);
            }
        }

        private static DateTime? CorrectDateTimeForAzure(DateTime? value)
        {
            if (value == null)
            {
                return null;
            }

            // tired of correcting the dates by hand each time
            return value < MinimumAzureTableStorageDate ? MinimumAzureTableStorageDate : value;
        }
    }

    public class StoragePropertyAttribute : Attribute
    {
        public StoragePropertyAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    public static class EntityConverter<T>
    {
        public static object TableEntityResolver(string partitionKey, string rowKey, DateTimeOffset timestamp, IDictionary<string, EntityProperty> properties, string etag)
        {
            var type = typeof(T);
            var poco = Activator.CreateInstance(type, true);

            foreach (var propertyInfo in EntityConverter.GetSerializableProperties(type).Where(p => p.CanWrite))
            {
                var storageProperty = propertyInfo.GetCustomAttribute<StoragePropertyAttribute>();
                var name = storageProperty == null ? propertyInfo.Name : storageProperty.Name;

                if (properties.TryGetValue(name, out var dynamicProperty))
                {
                    try
                    {
                        TrySetProperty(poco, dynamicProperty, propertyInfo);
                    }
                    catch (Exception ex)
                    {
                        TraceLogger.Error(ex.Message, nameof(EntityConverter), ex);
                    }
                }
                else
                {
                    if (propertyInfo.GetCustomAttribute<PartitionKeyAttribute>() != null)
                    {
                        var value = TypeDescriptor.GetConverter(propertyInfo.PropertyType).ConvertFrom(partitionKey);
                        propertyInfo.SetValue(poco, value);
                    }

                    if (propertyInfo.GetCustomAttribute<RowKeyAttribute>() != null)
                    {
                        var value = TypeDescriptor.GetConverter(propertyInfo.PropertyType).ConvertFrom(rowKey);
                        propertyInfo.SetValue(poco, value);
                    }
                }
            }

            if (poco is ITableEntity tableEntity)
            {
                tableEntity.PartitionKey = partitionKey;
                tableEntity.RowKey = rowKey;
                tableEntity.ETag = etag;
                tableEntity.Timestamp = timestamp;
            }

            return poco;
        }

        private static void TrySetProperty(object entity, EntityProperty entityProperty, PropertyInfo propertyInfo)
        {
            var underlyingType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
            var type = underlyingType != null ? underlyingType : propertyInfo.PropertyType;

            switch (entityProperty.PropertyType)
            {
                case EdmType.String:
                    if (type.IsEnum)
                    {
                        if (!string.IsNullOrEmpty(entityProperty.StringValue))
                        {
                            propertyInfo.SetValue(entity, Enum.Parse(type, entityProperty.StringValue), null);
                        }
                        break;
                    }

                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        propertyInfo.SetValue(entity, entityProperty.StringValue, null);
                        break;
                    }

                    if (type == typeof(DateTime))
                    {
                        if (DateTime.TryParse(entityProperty.StringValue, out var validDateTime))
                        {
                            propertyInfo.SetValue(entity, validDateTime, null);
                        }
                        break;
                    }

                    // Looks like JSON
                    if (!string.IsNullOrEmpty(entityProperty.StringValue) && (entityProperty.StringValue[0] == '[' || entityProperty.StringValue[0] == '{'))
                    {
                        propertyInfo.SetValue(entity, entityProperty.StringValue.FromFullTypeJson(type), null);
                        break;
                    }

                    // We can not match it to any actual types, it does not look like JSON but the type is object so just set it to string
                    if (propertyInfo.PropertyType == typeof(object))
                    {
                        propertyInfo.SetValue(entity, entityProperty.StringValue, null);
                    }

                    break;
                case EdmType.Binary:
                    if (propertyInfo.PropertyType == typeof(byte[]) || propertyInfo.PropertyType == typeof(object))
                    {
                        propertyInfo.SetValue(entity, entityProperty.BinaryValue, null);
                    }
                    break;
                case EdmType.Boolean:
                    if (type == typeof(bool) || propertyInfo.PropertyType == typeof(object))
                    {
                        propertyInfo.SetValue(entity, entityProperty.BooleanValue, null);
                    }
                    break;
                case EdmType.DateTime:
                    if (propertyInfo.PropertyType == typeof(DateTime))
                    {
                        propertyInfo.SetValue(entity, entityProperty.DateTimeOffsetValue.Value.UtcDateTime, null);
                    }

                    if (propertyInfo.PropertyType == typeof(DateTime?) || propertyInfo.PropertyType == typeof(object))
                    {
                        propertyInfo.SetValue(entity, entityProperty.DateTimeOffsetValue?.UtcDateTime, null);
                    }

                    if (propertyInfo.PropertyType == typeof(DateTimeOffset))
                    {
                        propertyInfo.SetValue(entity, entityProperty.DateTimeOffsetValue.Value, null);
                    }

                    if (propertyInfo.PropertyType == typeof(DateTimeOffset?))
                    {
                        propertyInfo.SetValue(entity, entityProperty.DateTimeOffsetValue, null);
                    }
                    break;
                case EdmType.Double:
                    if (type == typeof(double) || propertyInfo.PropertyType == typeof(object))
                    {
                        propertyInfo.SetValue(entity, entityProperty.DoubleValue, null);
                    }
                    break;
                case EdmType.Guid:
                    if (type == typeof(Guid) || propertyInfo.PropertyType == typeof(object))
                    {
                        propertyInfo.SetValue(entity, entityProperty.GuidValue, null);
                    }
                    break;
                case EdmType.Int32:
                    if (type == typeof(int) || propertyInfo.PropertyType == typeof(object))
                    {
                        propertyInfo.SetValue(entity, entityProperty.Int32Value, null);
                    }
                    break;
                case EdmType.Int64:
                    if (type == typeof(long) || propertyInfo.PropertyType == typeof(object))
                    {
                        propertyInfo.SetValue(entity, entityProperty.Int64Value, null);
                    }
                    break;
            }
        }
    }
}