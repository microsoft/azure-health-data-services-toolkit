using System;
using Newtonsoft.Json.Linq;

namespace Azure.Health.DataServices.Json
{
    /// <summary>
    /// Extensions for JToken.
    /// </summary>
    public static class JTokenExtensions
    {
        /// <summary>
        /// Indicates whether a token exists found by JPath.
        /// </summary>
        /// <param name="jtoken">JToken root.</param>
        /// <param name="jpath">JPath to test whether the JToken exists.</param>
        /// <returns></returns>
        public static bool Exists(this JToken jtoken, string jpath)
        {
            return jtoken.SelectToken(jpath) != null;
        }

        /// <summary>
        /// Indicates whether the value of a token found by JPath is a match.
        /// </summary>
        /// <param name="jtoken">JToken root.</param>
        /// <param name="jpath">JPath to test for JToken.</param>
        /// <param name="value">Value to test for JToken in path.</param>
        /// <returns>True is match; otherwise false.</returns>
        public static bool IsMatch(this JToken jtoken, string jpath, string value)
        {
            try
            {
                return jtoken.SelectToken(jpath).Value<string>() == value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Indicates whether the value of a token found by JPath is a match.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="jtoken">JToken root.</param>
        /// <param name="jpath">JPath to test for JToken.</param>
        /// <param name="value">Value to test for JToken in path.</param>
        /// <returns>True is match; otherwise false.</returns>
        public static bool IsMatch<T>(this JToken jtoken, string jpath, T? value)
        {
            try
            {
                T val = jtoken.SelectToken(jpath).Value<T?>();
                return val.Equals(value);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets JArray from a JPath from the root JToKen; otherwise null if array not present.
        /// </summary>
        /// <param name="token">JToken root.</param>
        /// <param name="jpath">JPath to array.</param>
        /// <param name="throwIfNull">If true throws and exception is array not found; otherwise null.</param>
        /// <returns>JArray is found by JPath; otherwise if throwIfNull is true throws an exception; otherwise returns null.</returns>
        public static JArray GetArray(this JToken token, string jpath, bool throwIfNull = false)
        {
            try
            {
                JArray array = (JArray)token.SelectToken(jpath);
                if (array != null)
                {
                    return array;
                }
                else if (!throwIfNull)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                if (throwIfNull)
                {
                    throw new JPathException("JPath for array invalid.");
                }
            }

            if (throwIfNull)
            {
                throw new JPathException("JArray is null.");
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Indicates true when an array is found via JPath.
        /// </summary>
        /// <param name="token">JToken root.</param>
        /// <param name="jpath">JPath to array.</param>
        /// <returns>True if JPath finds an array; otherwise false.</returns>
        public static bool IsArray(this JToken token, string jpath)
        {
            JToken ztoken = GetToken(token, jpath);
            return ztoken.IsArray();
        }

        /// <summary>
        /// Indicates true when a JToken is an array; otherwise false.
        /// </summary>
        /// <param name="token">JToken to test.</param>
        /// <returns>True is JToken is an array; otherwise false.</returns>
        public static bool IsArray(this JToken token)
        {
            return !token.IsNullOrEmpty() && token is JArray;
        }

        /// <summary>
        /// Gets the value of an item in a JArray.
        /// </summary>
        /// <typeparam name="T">Data type of item value.</typeparam>
        /// <param name="token">JToken root.</param>
        /// <param name="jpath">JPath to array.</param>
        /// <param name="throwIfNull">Determines whether an exception is throw if the item value cannot be returned; otherwise returns null.</param>
        /// <returns></returns>
        public static T? GetArrayItem<T>(this JToken token, string jpath, bool throwIfNull = false)
        {
            JToken ztoken = GetToken(token, jpath);
            if (ztoken != null)
            {
                return GetValue<T>(ztoken, throwIfNull);
            }
            else if (throwIfNull)
            {
                throw new JPathException("Array item is null.");
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Gets a value from a JToken.
        /// </summary>
        /// <typeparam name="T">Data type of value.</typeparam>
        /// <param name="token">JToken to get value.</param>
        /// <param name="throwIfNull">Determines whether an exception is throw if the item value cannot be returned; otherwise returns null.</param>
        /// <returns></returns>
        public static T? GetValue<T>(this JToken token, bool throwIfNull = false)
        {
            if (token.IsNullOrEmpty())
            {
                return default;
            }

            if (token is JValue value)
            {
                return value.Value<T?>();
            }
            else if (throwIfNull)
            {
                throw new JPathException("JToken value is null.");
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Gets a value from a JToken root via JPath.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="token"></param>
        /// <param name="jpath"></param>
        /// <param name="throwIfNull"></param>
        /// <returns>Value of token if found.</returns>
        public static T? GetValue<T>(this JToken token, string jpath, bool throwIfNull = false)
        {
            JToken ztoken = GetToken(token, jpath);

            if (ztoken.IsNullOrEmpty() && !throwIfNull)
            {
                return default;
            }
            else if (ztoken.IsNullOrEmpty() && throwIfNull)
            {
                throw new JPathException("JToken value is null.");
            }

            if (ztoken is JValue value)
            {
                return value.Value<T?>();
            }
            else if (throwIfNull)
            {
                throw new JPathException("JValue is null.");
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Indicates true when a value found by a JPath is null or empty; otherwise false.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="jpath"></param>
        /// <returns>True if the token found is null or empty; otherwise false.</returns>
        public static bool IsNullOrEmpty(this JToken token, string jpath)
        {
            JToken ztoken = GetToken(token, jpath);

            return (ztoken == null) ||
                   (ztoken.Type == JTokenType.Array && !ztoken.HasValues) ||
                   (ztoken.Type == JTokenType.Object && !ztoken.HasValues) ||
                   (ztoken.Type == JTokenType.String && ztoken.ToString() == string.Empty) ||
                   (ztoken.Type == JTokenType.Null);
        }

        /// <summary>
        /// Indicates true when a JToken value is null or empty; otherwise false.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True is the token is null or empty; otherwise false.</returns>
        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == string.Empty) ||
                   (token.Type == JTokenType.Null);
        }

        /// <summary>
        /// Gets a JToken from a JPath.
        /// </summary>
        /// <param name="token">JToken root.</param>
        /// <param name="jpath">JPath to token.</param>
        /// <param name="throwIfNull">If true throws an exception if a token is not found; otherwise is false returns null.</param>
        /// <returns>JToken if found.</returns>
        public static JToken GetToken(this JToken token, string jpath, bool throwIfNull = false)
        {
            JToken? ztoken = token.SelectToken(jpath);

            if (throwIfNull && ztoken.IsNullOrEmpty())
            {
                throw new JPathException("JToken is null");
            }

            return ztoken;
        }
    }
}
