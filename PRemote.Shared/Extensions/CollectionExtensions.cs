using System;
using System.Collections.Generic;
using System.Text;

namespace PRemote.Shared.Extensions
{
    public static class CollectionExtensions //? Extensions for Collections types
    {
        /// <summary>
        /// Return a subpart of an <see cref="Array"/> by index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="index">Index to start</param>
        /// <returns>Return a subpart of an <see cref="Array"/> by index</returns>
        public static T[] SubArray<T>(this T[] arr, int index) //! Return a subpart of an Array
        {
            int lenght = arr.Length - index;
            T[] subarr = new T[lenght];
            Array.Copy(arr, index, subarr, 0, lenght);
            return subarr;
        }
        /// <summary>
        /// Return a subpart of an <see cref="Array"/> by index and lenght
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="index">Index to start</param>
        /// <param name="lenght">Lenght to cut</param>
        /// <returns>Return a subpart of an <see cref="Array"/> by index and lenght</returns>
        public static T[] SubArray<T>(this T[] arr, int index, int lenght) //! Return a subpart of an Array
        {
            if (lenght + index > arr.Length)
                lenght = arr.Length - index;

            T[] subarr = new T[lenght];
            Array.Copy(arr, index, subarr, 0, lenght);
            return subarr;
        }
        /// <summary>
        /// Return a subpart of an <see cref="Array"/> by index from end
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="index">Index to start</param>
        /// <param name="reversed">Is couting from end or not</param>
        /// <returns>Return a subpart of an <see cref="Array"/> by index from end</returns>
        public static T[] SubArray<T>(this T[] arr, int index, bool reversed) //! Return a subpart of an Array
        {
            if (!reversed)
                return arr.SubArray(index);

            index = arr.Length - index;
            if (index < 0)
                index = 0;

            int lenght = arr.Length - index;

            T[] subarr = new T[lenght];
            Array.Copy(arr, index, subarr, 0, lenght);
            return subarr;
        }
        /// <summary>
        /// Return a subpart of an <see cref="Array"/> by index and lenght from end
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="index">Index to start</param>
        /// <param name="lenght">Lenght to cut</param>
        /// <param name="reversed">Is couting from end or not</param>
        /// <returns>Return a subpart of an <see cref="Array"/> by index and lenght from end</returns>
        public static T[] SubArray<T>(this T[] arr, int index, int lenght, bool reversed) //! Return a subpart of an Array
        {
            if (!reversed)
                return arr.SubArray(index, lenght);

            index = arr.Length - index;
            if (index < 0)
                index = 0;

            if (lenght + index > arr.Length)
                lenght = arr.Length - index;

            T[] subarr = new T[lenght];
            Array.Copy(arr, index, subarr, 0, lenght);
            return subarr;
        }

        /// <summary>
        /// Return all elements of an <see cref="IEnumerable{T}"/> in one line
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="delim">A <see cref="string"/> delimiter (ex: texte1, texte2, texte3)</param>
        /// <returns>Return all elementents of an array</returns>
        public static string Concat<T>(this IEnumerable<T> arr, string delim = "") //! Return all elements of an Enumerable<> in one line.
        {
            string final = "";

            foreach (T str in arr)
                final += str.ToString() + delim;

            final = final.Remove(final.Length - delim.Length);
            return final;
        }
    }
}
