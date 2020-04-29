using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NTextCat.Commons
{
    public static class StringExtensions
    {
        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static StringBuilder AddLine(this string head)
        {
            return new StringBuilder(head).AddLine();
        }

        /// <summary>
        /// Appends <see cref="Environment.NewLine"/> followed by <paramref name="tail"/> to the end of <paramref name="head"/>
        /// </summary>
        /// <param name="head"></param>
        /// <param name="tail"></param>
        /// <returns>reference to <paramref name="head"/> passed</returns>
        public static StringBuilder AddLine(this string head, string tail)
        {
            return head.AddLine().Append(tail);
        }

        /// <summary>
        /// Appends <see cref="Environment.NewLine"/> to the end of <paramref name="head"/>
        /// Basically the same as <see cref="StringBuilder.AppendLine()"/>
        /// </summary>
        /// <param name="head"></param>
        /// <returns>reference to <paramref name="head"/> passed</returns>
        public static StringBuilder AddLine(this StringBuilder head)
        {
            return head.AppendLine();
        }

        /// <summary>
        /// Appends <see cref="Environment.NewLine"/> followed by <paramref name="tail"/> to the end of <paramref name="head"/>
        /// </summary>
        /// <param name="head"></param>
        /// <param name="tail"></param>
        /// <returns>reference to <paramref name="head"/> passed</returns>
        public static StringBuilder AddLine(this StringBuilder head, string tail)
        {
            return head.AppendLine().Append(tail);
        }

        public static void PassTo<T>(this T value, Action<T> action)
        {
            action(value);
        }

        public static TResult PassTo<T, TResult>(this T value, Func<T, TResult> func)
        {
            return func(value);
        }

        public static IEnumerable<string> ReadLines(this TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Returns sequence of string representations of original items converted with <seealso cref="object.ToString"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence">Sequence of items that need to be represented as strings</param>
        /// <returns>Sequence of strings</returns>
        public static IEnumerable<string> ToStrings<T>(this IEnumerable<T> sequence)
        {
            return sequence.Select(element => element.ToString());
        }

        /// <summary>
        /// interleaves initial sequence with specified interleaver. 
        /// Output sequence looks like megasandwich, each bread is each intital sequence element and each cheese is interleaver.
        /// </summary>
        /// <typeparam name="T">type of sequence elements</typeparam>
        /// <param name="sequence">initial sequence</param>
        /// <param name="interleaver">"cheese" instance to put between elements of intital sequence</param>
        /// <param name="startsWithInterleaver">
        /// if true, additional interleaver is added to the beginnig of output sequence but not in case if intial sequence is empty</param>
        /// <param name="endsWithInterleaver">
        /// if true, additional interleaver is added to the end of output sequence but not in case if intial sequence is empty</param>
        /// <returns>elements of initial sequence interleaved by interleaver</returns>
        public static IEnumerable<T> Interleave<T>(this IEnumerable<T> sequence, T interleaver, bool startsWithInterleaver, bool endsWithInterleaver)
        {
            bool sequenceIsNotEmpty = false;
            bool outputInterleaver = startsWithInterleaver;
            foreach (T element in sequence)
            {
                sequenceIsNotEmpty = true;
                if (outputInterleaver)
                    yield return interleaver;
                else
                    outputInterleaver = true;
                
                yield return element;
            }
            if (sequenceIsNotEmpty && endsWithInterleaver)
                yield return interleaver;
        }

        /// <summary>
        /// interleaves initial sequence with specified interleaver. 
        /// Output sequence looks like megasandwich, each bread is each intital sequence element and each cheese is interleaver.
        /// Is equivalent to calling <see cref="Interleave{T}(IEnumerable{T},T,bool,bool)"/> with false for "starts/ends with interleaver"
        /// </summary>
        /// <typeparam name="T">type of sequence elements</typeparam>
        /// <param name="sequence">initial sequence</param>
        /// <param name="interleaver">"cheese" instance to put between elements of intital sequence</param>
        /// <returns>elements of initial sequence interleaved by interleaver</returns>
        public static IEnumerable<T> Interleave<T>(this IEnumerable<T> sequence, T interleaver)
        {
            return sequence.Interleave(interleaver, false, false);
        }

        /// <summary>
        /// Glues all <paramref name="strings"/> in a single string. Puts <paramref name="separator"/> between any two adjacent strings.
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="separator">is put between strings</param>
        /// <returns>String in format: {Item1}[{separator}{Item2}[{separator}{Item3}...]]</returns>
        public static string Join(this IEnumerable<string> strings, string separator)
        {
            var sb = new StringBuilder();
            strings.Interleave(separator).ForEach(s => sb.Append(s));
            return sb.ToString();
        }

        /// <summary>
        /// Returns substrings which were delimited in <paramref name="sourceString"/> with separator specified.
        /// </summary>
        /// <param name="sourceString">input string</param>
        /// <param name="separator">separator between substrings</param>
        /// <returns>sequence of strings</returns>
        public static IEnumerable<string> Split(this string sourceString, string separator)
        {
            return Split(sourceString, separator, 0);
        }

        /// <summary>
        /// Returns substrings which were delimited in <paramref name="sourceString"/> with separator specified.
        /// </summary>
        /// <param name="sourceString">input string</param>
        /// <param name="separator">separator between substrings</param>
        /// <param name="startInx">the begininning index of <paramref name="sourceString"/> from which separators are sought (any text before this index is ignored)</param>
        /// <returns>sequence of strings</returns>
        public static IEnumerable<string> Split(this string sourceString, string separator, int startInx)
        {
            return Split(sourceString, separator, startInx, Int32.MaxValue);
        }

        /// <summary>
        /// Returns substrings which were delimited in <paramref name="sourceString"/> with separator specified.
        /// </summary>
        /// <param name="sourceString">input string</param>
        /// <param name="separator">separator between substrings</param>
        /// <param name="startInx">the begininning index of <paramref name="sourceString"/> from which separators are sought (any text before this index is ignored)</param>
        /// <param name="endInx">the end index of <paramref name="sourceString"/> up to which separators are sought (any text on or after this index is ignored)</param>
        /// <returns>sequence of strings</returns>
        public static IEnumerable<string> Split(this string sourceString, string separator, int startInx, int endInx)
        {
            int indexOfSeparator;
            int leftSliceStartInx = startInx;
            int separatorLength = separator.Length;
            while ((indexOfSeparator = sourceString.IndexOf(separator, leftSliceStartInx, StringComparison.Ordinal)) >= 0 && leftSliceStartInx < endInx)
            {
                int newSliceStartInx = indexOfSeparator + separatorLength;
                yield return sourceString.Substring(leftSliceStartInx, Math.Min(indexOfSeparator, endInx) - leftSliceStartInx);
                leftSliceStartInx = newSliceStartInx;
            }
            // haven't reached endInx but left a tale
            if (indexOfSeparator < 0)
            {
                yield return sourceString.Substring(leftSliceStartInx, Math.Min(sourceString.Length, endInx) - leftSliceStartInx);
            }
        }
        
        /// <summary>
        /// Invokes an <paramref name="action"/> on each element of sequence
        /// </summary>
        /// <typeparam name="T">type of sequence elements</typeparam>
        /// <param name="sequence">initial sequence</param>
        /// <param name="action">Invokes an <paramref name="action"/> on each element of sequence</param>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (T element in sequence)
            {
                action(element);
            }
        }

        /// <summary>
        /// Returns those items of input sequence for which <paramref name="whereFunc"/> returns false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence">Input sequence of items</param>
        /// <param name="whereFunc">Predicate filter</param>
        /// <returns>Sequence of those items of input sequence for which <paramref name="whereFunc"/> returns false</returns>
        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> sequence, Func<T, bool> whereFunc)
        {
            return sequence.Where(element => !whereFunc(element));
        }

        /// <summary>
        /// Checks if input string is valid integer number
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>true if <paramref name="str"/> is valid integer, otherwise false</returns>
        public static bool StringIsLong(this string str)
        {
            long i;
            return long.TryParse(str, out i);
        }

        /// <summary>
        /// Checks if input string is valid number (integer or fractional)
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>true if input string is valid number (integer or fractional), otherwise false</returns>
        public static bool StringIsDouble(this string str)
        {
            double d;
            return double.TryParse(str, out d);
        }
    }
}