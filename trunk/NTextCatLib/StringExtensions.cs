using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NTextCat.Lib
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
    }
}
