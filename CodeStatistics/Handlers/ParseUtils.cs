﻿using System;

namespace CodeStatistics.Handlers{
    static class ParseUtils{
        public static readonly StringComparison cmp = StringComparison.Ordinal;

        /// <summary>
        /// If the string starts with searched string, it is removed, assigned to the result and the function returns true. If not, the function returns false and the result will be empty.
        /// </summary>
        public static bool ExtractStart(this string me, string search, out string result){
            if (me.StartsWith(search,cmp)){
                result = me.Substring(search.Length);
                return true;
            }
            else{
                result = String.Empty;
                return false;
            }
        }

        /// <summary>
        /// If the string ends with searched string, it is removed, assigned to the result and the function returns true. If not, the function returns false and the result will be empty.
        /// </summary>
        public static bool ExtractEnd(this string me, string search, out string result){
            if (me.EndsWith(search,cmp)){
                result = me.Substring(0,me.Length-search.Length);
                return true;
            }
            else{
                result = String.Empty;
                return false;
            }
        }

        /// <summary>
        /// If the string starts and ends with searched strings, they are removed, assigned to the result and the function returns true. If not, the function returns false and the result will be empty.
        /// </summary>
        public static bool ExtractBoth(this string me, string searchStart, string searchEnd, out string result){
            if (me.StartsWith(searchStart,cmp) && me.EndsWith(searchEnd,cmp)){
                result = me.Substring(searchStart.Length,me.Length-searchEnd.Length-searchStart.Length);
                return true;
            }
            else{
                result = String.Empty;
                return false;
            }
        }

        /// <summary>
        /// Removes all text before the first occurrence of searched string, including the searched string
        /// </summary>
        public static string RemoveTo(this string me, string search){
            int index = me.IndexOf(search);
            return index == -1 ? me : me.Substring(index+search.Length,me.Length-index-search.Length);
        }

        /// <summary>
        /// Removes all text after the first occurrence of searched string, including the searched string
        /// </summary>
        public static string RemoveFrom(this string me, string search){
            int index = me.IndexOf(search);
            return index == -1 ? me : me.Substring(0,index);
        }
    }
}
