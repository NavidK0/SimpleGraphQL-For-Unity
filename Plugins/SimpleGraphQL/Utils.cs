namespace SimpleGraphQL
{
    public static class Utils
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Checks if a string is null, empty, or contains whitespace.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhitespace(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                foreach (char c in str)
                {
                    if (!char.IsWhiteSpace(c))
                        return false;
                }
            }

            return true;
        }
    }
}