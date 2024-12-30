namespace AudioScriptExtractor
{
    public static class StringExtensions
    {
        /// <summary>
        /// Check if string contains any of the substrings specified in ss
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ssa">List of substrings</param>
        /// <returns></returns>
        public static bool ContainsAny(this String s, string[] ssa)
        {
            return ssa.Any(ss => s.Contains(ss));
        }
    }
}
