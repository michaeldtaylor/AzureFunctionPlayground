using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Genius.Shared.Azure.Serialization
{
}

namespace AddressFulfilment.Shared.Utilities
{
    /// <summary>
    /// Common guard class for argument validation.
    /// </summary>
    [DebuggerStepThrough]
    public static class Guard
    {
        /// <summary>
        /// Ensures the given <paramref name="value"/> is not null.
        /// Throws <see cref="ArgumentNullException"/> otherwise.
        /// </summary>
        /// <exception cref="System.ArgumentException">The <paramref name="value"/> is null.</exception>
        public static void NotNull(string argumentName, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName, "Parameter cannot be null.");
            }
        }

        /// <summary>
        /// Ensures the given string <paramref name="value"/> is not null or empty.
        /// Throws <see cref="ArgumentNullException"/> in the first case, or 
        /// <see cref="ArgumentException"/> in the latter.
        /// </summary>
        /// <exception cref="System.ArgumentException">The <paramref name="value"/> is null or an empty string.</exception>
        public static void NotNullOrEmpty(string argumentName, string value)
        {
            NotNull(argumentName, value);

            if (value.Length == 0)
            {
                throw new ArgumentException("Parameter cannot be empty.", argumentName);
            }
        }

        public static void EnumDefined<T>(string argumentName, T value)
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new ArgumentException($"Parameter must be enum of type {typeof(T).Name}. Was {value}.", argumentName);
            }
        }

        public static void GreaterThanOrEqual<T>(string argumentName, T value, T referencePoint) where T : IComparable
        {
            if (value.CompareTo(referencePoint) < 0)
            {
                throw new ArgumentException($"Parameter must be greater than or equal {referencePoint}. Was {value}.", argumentName);
            }
        }

        public static void GreaterThan<T>(string argumentName, T value, T referencePoint) where T : IComparable
        {
            if (value.CompareTo(referencePoint) <= 0)
            {
                throw new ArgumentException($"Parameter must be greater than {referencePoint}. Was {value}.", argumentName);
            }
        }

        public static void LessThanOrEqual<T>(string argumentName, T value, T referencePoint) where T : IComparable
        {
            if (value.CompareTo(referencePoint) > 0)
            {
                throw new ArgumentException($"Parameter must be less than or equal {referencePoint}. Was {value}.", argumentName);
            }
        }

        public static void LessThan<T>(string argumentName, T value, T referencePoint) where T : IComparable
        {
            if (value.CompareTo(referencePoint) >= 0)
            {
                throw new ArgumentException($"Parameter must be less than {referencePoint}. Was {value}.", argumentName);
            }
        }

        public static void NotEmpty<T>(string argumentName, IEnumerable<T> value)
        {
            if (!value.Any())
            {
                throw new ArgumentException($"Parameter must contain at least one element.", argumentName);
            }
        }

        public static void IsEqual<T>(string argumentName, T value, T reference, string message)
        {
            if (!Equals(value, reference))
            {
                throw new ArgumentException(message, argumentName);
            }
        }

        public static void IsTrue(string argumentName, bool value, string message)
        {
            if (!value)
            {
                throw new ArgumentException(message, argumentName);
            }
        }

        public static void IsFalse(string argumentName, bool value, string message)
        {
            if (value)
            {
                throw new ArgumentException(message, argumentName);
            }
        }

        public static void MustHaveLengthLessThanOrEqualTo<T>(IEnumerable<T> propertyValue, int maxLength, string propertyName)
        {
            NotNullOrEmpty(nameof(propertyName), propertyName);

            var nullablePropertyValue = propertyValue;

            if (nullablePropertyValue == null || nullablePropertyValue.Count() <= maxLength)
            {
                return;
            }

            throw new ArgumentException($"{propertyName} must have a length less than or equal to {maxLength}", propertyName);
        }
    }
}