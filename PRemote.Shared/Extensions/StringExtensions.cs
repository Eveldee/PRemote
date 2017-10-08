using System;
using System.Collections.Generic;
using System.Text;

namespace PRemote.Shared.Extensions
{
    public static class StringExtensions //? String Extensions
    {
        /// <summary>
        /// Check if a <see cref="string"/> is Null or Empty (" " / "" / null)
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Return true if the <see cref="string"/> in Null or Empty</returns>
        public static bool IsNullOrEmpty(this string str) //! Check if a string is Null or Empty (" "/"")
        {
            if (string.IsNullOrEmpty(str))
                return true;
            if (string.IsNullOrWhiteSpace(str))
                return true;
            return false;
        }
        /// <summary>
        /// Check if a <see cref="string"/> equals one argument
        /// </summary>
        /// <param name="str"></param>
        /// <param name="values">A list of <see cref="string"/> to compare</param>
        /// <returns>Return true if the <see cref="string"/> equals at least one argument</returns>
        public static bool EqualsOne(this string str, params string[] values) //! Return true if the string equal another one string in the Array
        {
            foreach (string s in values)
            {
                if (str == s)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Check if a <see cref="string"/> equals one argument, ignore case
        /// </summary>
        /// <param name="str"></param>
        /// <param name="values">A list of <see cref="string"/> to compare</param>
        /// <returns>Return true if the <see cref="string"/> equals at least one argument</returns>
        public static bool EqualsOneIgnoreCase(this string str, params string[] values) //! Return true if the string equal another one string in the Array, Add ComparisonType
        {
            foreach (string s in values)
            {
                if (str.Equals(s, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Check if a <see cref="string"/> start with one argument
        /// </summary>
        /// <param name="str"></param>
        /// <param name="values">A list of <see cref="string"/> to check</param>
        /// <returns>Return true if the <see cref="string"/> start at least with one argument</returns>
        public static bool StartsWithOne(this string str, params string[] values) //! Return true if the string Start with a a string in the Array
        {
            foreach (string s in values)
            {
                if (str.StartsWith(s))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Check if a <see cref="string"/> start with one argument, ignore case
        /// </summary>
        /// <param name="str"></param>
        /// <param name="values">A list of <see cref="string"/> to check</param>
        /// <returns>Return true if the <see cref="string"/> start at least with one argument</returns>
        public static bool StartsWithOneIgnoreCase(this string str, params string[] values) //! Return true if the string Start with a a string in the Array, Add Comparison Type
        {
            foreach (string s in values)
            {
                if (str.StartsWith(s, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Check if a <see cref="string"/> contains one argument
        /// </summary>
        /// <param name="str"></param>
        /// <param name="values">A list of <see cref="string"/> to check</param>
        /// <returns>Return true if the <see cref="string"/> contain at least one argument</returns>
        public static bool ContainsOne(this string str, params string[] values) //! Return true if the string Contain one argument
        {
            foreach (string s in values)
            {
                if (str.Contains(s))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Check if a <see cref="string"/> contains one argument, ignore case
        /// </summary>
        /// <param name="str"></param>
        /// <param name="values">A list of <see cref="string"/> to check</param>
        /// <returns>Return true if the <see cref="string"/> contain at least one argument</returns>
        public static bool ContainsOneIgnoreCase(this string str, params string[] values) //! Return true if the string Contain one argument
        {
            foreach (string s in values)
            {
                if (str.ToLower().Contains(s.ToLower()))
                    return true;
            }
            return false;
        }
    }
}
