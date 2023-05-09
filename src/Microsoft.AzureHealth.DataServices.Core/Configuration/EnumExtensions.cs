using System;
using System.ComponentModel;
using System.Reflection;

namespace Microsoft.AzureHealth.DataServices.Configuration
{
    /// <summary>
    /// Extensions for use with enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the description attribute value for the enum.
        /// </summary>
        /// <param name="value">Value of enum</param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the category attribute value for the enum.
        /// </summary>
        /// <param name="value">Value of enum</param>
        /// <returns></returns>
        public static string GetCategory(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    CategoryAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(CategoryAttribute)) as CategoryAttribute;
                    if (attr != null)
                    {
                        return attr.Category;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets an enum value from the description attribute.
        /// </summary>
        /// <typeparam name="T">Type of enum.</typeparam>
        /// <param name="description">String of description.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T GetValueFromDescription<T>(string description) where T : Enum
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

            throw new ArgumentException("Not found.", nameof(description));
        }
    }
}
