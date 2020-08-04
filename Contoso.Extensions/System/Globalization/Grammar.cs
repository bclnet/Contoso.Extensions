using System.Linq;

namespace System.Globalization
{
    public static class Grammar
    {
        /// <summary>
        /// Get all Vowels
        /// </summary>
        /// <returns>all vowels char</returns>
        public static readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u' };

        /// <summary>
        /// Adds was were based on <paramref name="number"/> to string <paramref name="stringToAppend"/>
        /// </summary>
        /// <paramref name="number"/>Number in string format. Must be zero or positive
        /// If equals 0 add "none were" + <paramref name="stringToAppend"/>
        /// If equals 1 add "1 was" + <paramref name="stringToAppend"/>
        /// If greater than 1 add "2/number were" + <paramref name="stringToAppend"/>
        /// <paramref name="stringToAppend"/>String to append with was/were
        /// <returns>was/were string</returns>
        public static string WasWere(int number, string stringToAppend) => number != 0
            ? $"{number}{(number == 1 ? " was" : " were")}{(stringToAppend != null ? " " + stringToAppend : string.Empty)}"
            : $"none were {(stringToAppend != null ? " " + stringToAppend : string.Empty)}";
        /// <summary>
        /// Adds was were based on <paramref name="number"/> to string <paramref name="stringToAppend"/>
        /// </summary>
        /// <paramref name="number"/>Number in string format. Must be zero or positive
        /// If equals 0 add "none were" + <paramref name="stringToAppend"/>
        /// If equals 1 add "1 was" + <paramref name="stringToAppend"/>
        /// If greater than 1 add "2/number were" + <paramref name="stringToAppend"/>
        /// <paramref name="stringToAppend"/>String to append with was/were
        /// <returns>was/were string</returns>
        public static string WasWere(string number, string stringToAppend) => number != "0"
            ? $"{number}{(number == "1" ? " was" : " were")}{(stringToAppend != null ? " " + stringToAppend : string.Empty)}"
            : $"none were {(stringToAppend != null ? " " + stringToAppend : string.Empty)}";

        static string Pluralize(int number, string stringToAppend) => number == 1 ? stringToAppend ?? string.Empty : (stringToAppend ?? string.Empty) + "s";
        static string Pluralize(string number, string stringToAppend) => number == "1" ? stringToAppend ?? string.Empty : (stringToAppend ?? string.Empty) + "s";

        /// <summary>
        /// Makes the string passed in possesive by adding 's to it
        /// if string doesn't ends with "s".
        /// </summary>
        /// <paramref name="makePossesive"/>
        /// <returns>possesive string</returns>
        public static string Possesive(string makePossesive) => makePossesive.ToLowerInvariant().EndsWith("s") ? makePossesive : makePossesive + "'s";

        /// <summary>
        /// Converts the string gender to "he"/"she" or "other" based on passed in string
        /// </summary>
        /// <paramref name="gender"/>
        /// If gender equals male returns "he has". 
        /// If gender equals female returns "she has".
        /// else returns "they have".
        /// <returns>string gender</returns>
        public static string HeShe(string gender) => !string.IsNullOrEmpty(gender) && gender.ToLowerInvariant() == "female" ? "she has" : gender?.ToLowerInvariant() == "male" ? "he has" : "they have";

        /// <summary>
        ///  Converts the string gender to "him" or "her" or "zim" based on passed in gender
        /// </summary>
        /// <paramref name="gender"/>
        /// If gender equals male returns "him". 
        /// If gender equals female returns "her".
        /// else returns "them".
        /// <returns>him/her/them</returns>
        public static string HimHer(string gender) => !string.IsNullOrEmpty(gender) && gender.ToLowerInvariant() == "female" ? "her" : gender?.ToLowerInvariant() == "male" ? "him" : "them";

        /// <summary>
        /// Returns the number and makes string <paramref name="stringToAppend"/> based on <paramref name="number"/>
        /// </summary>
        /// <paramref name="number"/>Number in string format. Must be zero or positive
        /// If equals 1 or zero returns <paramref name="stringToAppend"/>
        /// If greater than 1 makes <paramref name="stringToAppend"/> to append plural by adding "s".
        /// <paramref name="stringToAppend" />String to be plural
        /// <returns>string number and pluraled string</returns>
        public static string PluralizePhrase(int number, string stringToAppend) => number.ToString() + (stringToAppend != null ? " " + Pluralize(number, stringToAppend) : "");
        /// <summary>
        /// Pluralizes the phrase.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="stringToAppend">The string to append.</param>
        /// <returns>System.String.</returns>
        public static string PluralizePhrase(string number, string stringToAppend) => number + (stringToAppend != null ? " " + Pluralize(number, stringToAppend) : "");

        /// <summary>
        /// Converts the <paramref name="stringToAppend"/> to plural
        /// if <paramref name="number"/> is 1 and <paramref name="number"/> starts with a vowel returns "an" else "a"
        /// if <paramref name="number"/> is greater than 1 makes <paramref name="stringToAppend"/> plural
        /// </summary>
        /// <paramref name="number"/>Number in string format. Must be zero or positive
        /// <paramref name="stringToAppend"/>String to be plural.
        /// <returns>string number/a/an and pluraled string</returns>
        public static string PluralizePhraseWithArticles(int number, string stringToAppend)
        {
            var firstCharacter = !string.IsNullOrEmpty(stringToAppend) ? stringToAppend.ToLowerInvariant()[0] : 0;
            return number == 1 ? Vowels.Any(x => x == firstCharacter) ? "an" + (stringToAppend != null ? " " + stringToAppend : "") : "a" + (stringToAppend != null ? " " + stringToAppend : "") : number + (stringToAppend != null ? " " + Pluralize(number, stringToAppend) : "");
        }

        /// <summary>
        /// Converts the string number to its nth form
        /// </summary>
        /// <paramref name="number" />Number in string format. Must be zero or positive
        /// If equals 0 returns 0th
        /// If 1 returns 1st
        /// If 2 returns 2nd
        /// If 3 returns 3rd
        /// Else returns number+th
        /// <returns>string Nth number</returns>
        
        public static string Nth(int number)
        {
            if (number > 3 && number < 21) return number + "th";
            switch (number % 10)
            {
                case 0: return "0th";
                case 1: return "1st";
                case 2: return "2nd";
                case 3: return "3rd";
                default: return number + "th";
            };
        }

        //public static string WasWere(string number, string stringToAppend) => WasWere(int.Parse(number), stringToAppend);
        //public static string Pluralize(string number, string stringToAppend) => Pluralize(int.Parse(number), stringToAppend);
        //public static string PluralizePhrase(string number, string stringToAppend) => PluralizePhrase(int.Parse(number), stringToAppend);
        //public static string PluralizePhraseWithArticles(string number, string stringToAppend) => PluralizePhraseWithArticles(int.Parse(number), stringToAppend);
        //public static string Nth(string number) => Nth(int.Parse(number));
    }
}