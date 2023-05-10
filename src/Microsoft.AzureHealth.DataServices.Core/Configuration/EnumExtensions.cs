using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Microsoft.AzureHealth.DataServices.Configuration
{
    /// <summary>
    /// Provides extension methods for working with enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieves the description string from the Description attribute of the specified enum value.
        /// </summary>
        /// <param name="value">The enum value to retrieve the description string for.</param>
        /// <returns>The description string from the Description attribute of the enum value, or null if the attribute is not found.</returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves the category string from the Category attribute of the specified enum value.
        /// </summary>
        /// <param name="value">The enum value to retrieve the category string for.</param>
        /// <returns>The category string from the Category attribute of the enum value, or null if the attribute is not found.</returns>
        public static string? GetCategory(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field,
                             typeof(CategoryAttribute)) is CategoryAttribute attr)
                    {
                        return attr.Category;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the value of the specified enum type with a Description attribute or name matching the input string.
        /// </summary>
        /// <typeparam name="T">The enum type to search.</typeparam>
        /// <param name="description">The string to match against the Description attributes or enum names.</param>
        /// <returns>The enum value with matching Description attribute or name.</returns>
        /// <exception cref="ArgumentException">Thrown if no matching enum value is found.</exception>
        public static T GetValueFromDescription<T>(string description) where T : struct
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException($"No matching enum value found for description '{description}'.");
        }

        /// <summary>
        /// Retrieves an enumerable of values for the specified enum type that have a Description attribute matching the input string.
        /// </summary>
        /// <typeparam name="T">The enum type to search.</typeparam>
        /// <param name="description">The string to match against the Description attributes.</param>
        /// <returns>An enumerable of enum values with matching Description attributes.</returns>
        public static IEnumerable<T> GetValuesByDescription<T>(string description) where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();

            foreach (var value in values)
            {
                var fieldInfo = typeof(T).GetField(value.ToString());
                var attribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();

                if (attribute != null && attribute.Description == description)
                {
                    yield return value;
                }
            }
        }
    }
}
