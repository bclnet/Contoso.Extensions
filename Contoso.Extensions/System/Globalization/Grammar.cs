using System.Linq;

namespace System.Globalization
{
    /// <summary>
    /// Provides basic grammatological methods
    /// </summary>
    public static class Grammar
    {
        /// <summary>
        /// The vowels
        /// </summary>
        public static readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u' };

        /// <summary>
        /// Adds was/were based on <paramref name="number"/> to string <paramref name="stringToAppend"/>
        /// </summary>
        /// <paramref name="number"/>Number in string format. Must be zero or positive
        /// If equals 0 add "none were" + <paramref name="stringToAppend"/>
        /// If equals 1 add "1 was" + <paramref name="stringToAppend"/>
        /// If greater than 1 add "2/number were" + <paramref name="stringToAppend"/>
        /// <paramref name="stringToAppend"/>String to append with was/were
        /// <returns>was/were string</returns>
        public static string WasWere(int number, string stringToAppend = null) => number != 0
            ? $"{number}{(number == 1 ? " was" : " were")}{(string.IsNullOrEmpty(stringToAppend) ? string.Empty : $" {stringToAppend}")}"
            : $"none were{(string.IsNullOrEmpty(stringToAppend) ? string.Empty : $" {stringToAppend}")}";
        /// <summary>
        /// Adds was/were based on <paramref name="number"/> to string <paramref name="stringToAppend"/>
        /// </summary>
        /// <paramref name="number"/>Number in string format. Must be zero or positive
        /// If equals 0 add "none were" + <paramref name="stringToAppend"/>
        /// If equals 1 add "1 was" + <paramref name="stringToAppend"/>
        /// If greater than 1 add "2/number were" + <paramref name="stringToAppend"/>
        /// <paramref name="stringToAppend"/>String to append with was/were
        /// <returns>was/were string</returns>
        public static string WasWere(string number, string stringToAppend = null) => number != "0"
            ? $"{number}{(number == "1" ? " was" : " were")}{(string.IsNullOrEmpty(stringToAppend) ? string.Empty : $" {stringToAppend}")}"
            : $"none were{(string.IsNullOrEmpty(stringToAppend) ? string.Empty : $" {stringToAppend}")}";

        /// <summary>
        /// Makes a string plural by adding "s" to it if <paramref name="number"/> is greater than one
        /// </summary>
        /// <returns>pluralized string</returns>
        public static string Pluralize(int number, string stringToAppend) => stringToAppend == null || number == 1 ? stringToAppend ?? string.Empty : $"{stringToAppend}s";
        /// <summary>
        /// Makes a string plural by adding "s" to it if <paramref name="number"/> is greater than one
        /// </summary>
        /// <returns>pluralized string</returns>
        public static string Pluralize(string number, string stringToAppend) => stringToAppend == null || number == "1" ? stringToAppend ?? string.Empty : $"{stringToAppend}s";

        /// <summary>
        /// Makes a string possesive by adding "'s" to it if the string does not end with "s"
        /// </summary>
        /// <paramref name="makePossesive"/>
        /// <returns>possesive string</returns>
        public static string Possesive(string makePossesive) => makePossesive == null || makePossesive.ToLowerInvariant().EndsWith("s") ? makePossesive ?? string.Empty : $"{makePossesive}'s";

        /// <summary>
        /// Returns he/she or they based on <paramref name="gender"/>
        /// </summary>
        /// <paramref name="gender"/>
        /// If gender equals male returns "he has". 
        /// If gender equals female returns "she has".
        /// else returns "they have".
        /// <returns>string gender</returns>
        public static string HeShe(string gender) => !string.IsNullOrEmpty(gender) && gender.ToLowerInvariant() == "female" ? "she" : gender?.ToLowerInvariant() == "male" ? "he" : "they";

        /// <summary>
        /// Returns he/she or they have based on <paramref name="gender"/>
        /// </summary>
        /// <paramref name="gender"/>
        /// If gender equals male returns "he has". 
        /// If gender equals female returns "she has".
        /// else returns "they have".
        /// <returns>string gender</returns>
        public static string HeSheHas(string gender) => !string.IsNullOrEmpty(gender) && gender.ToLowerInvariant() == "female" ? "she has" : gender?.ToLowerInvariant() == "male" ? "he has" : "they have";

        /// <summary>
        /// Returns him/her or them based on <paramref name="gender"/>
        /// </summary>
        /// <paramref name="gender"/>
        /// If gender equals male returns "him". 
        /// If gender equals female returns "her".
        /// else returns "them".
        /// <returns>him/her/them</returns>
        public static string HimHer(string gender) => !string.IsNullOrEmpty(gender) && gender.ToLowerInvariant() == "female" ? "her" : gender?.ToLowerInvariant() == "male" ? "him" : "them";

        /// <summary>
        /// Returns his/hers or theirs based on <paramref name="gender"/>
        /// </summary>
        /// <paramref name="gender"/>
        /// If gender equals male returns "his". 
        /// If gender equals female returns "hers".
        /// else returns "they have".
        /// <returns>string gender</returns>
        public static string HisHers(string gender) => !string.IsNullOrEmpty(gender) && gender.ToLowerInvariant() == "female" ? "hers" : gender?.ToLowerInvariant() == "male" ? "his" : "theirs";

        /// <summary>
        /// Returns the number and makes string <paramref name="stringToAppend"/> based on <paramref name="number"/>
        /// </summary>
        /// <paramref name="number"/>Number in string format. Must be zero or positive
        /// If equals 1 or zero returns <paramref name="stringToAppend"/>
        /// If greater than 1 makes <paramref name="stringToAppend"/> to append plural by adding "s".
        /// <paramref name="stringToAppend" />String to be plural
        /// <returns>string number and pluraled string</returns>
        public static string PluralizePhrase(int number, string stringToAppend) => $"{number}{(string.IsNullOrEmpty(stringToAppend) ? string.Empty : $" {Pluralize(number, stringToAppend)}")}";
        /// <summary>
        /// Pluralizes the phrase.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="stringToAppend">The string to append.</param>
        /// <returns>System.String.</returns>
        public static string PluralizePhrase(string number, string stringToAppend) => $"{number}{(string.IsNullOrEmpty(stringToAppend) ? string.Empty : $" {Pluralize(number, stringToAppend)}")}";

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
            var firstCharacter = string.IsNullOrEmpty(stringToAppend) ? 0 : stringToAppend.ToLowerInvariant()[0];
            return number == 1 ? Vowels.Any(x => x == firstCharacter)
                ? $"an{(stringToAppend == null ? string.Empty : $" {stringToAppend}")}"
                : $"a{(stringToAppend == null ? string.Empty : $" {stringToAppend}")}"
                : $"{number}{(stringToAppend == null ? string.Empty : $" {Pluralize(number, stringToAppend)}")}";
        }
        /// <summary>
        /// Converts the <paramref name="stringToAppend"/> to plural
        /// if <paramref name="number"/> is 1 and <paramref name="number"/> starts with a vowel returns "an" else "a"
        /// if <paramref name="number"/> is greater than 1 makes <paramref name="stringToAppend"/> plural
        /// </summary>
        /// <paramref name="number"/>Number in string format. Must be zero or positive
        /// <paramref name="stringToAppend"/>String to be plural.
        /// <returns>string number/a/an and pluraled string</returns>
        public static string PluralizePhraseWithArticles(string number, string stringToAppend)
        {
            var firstCharacter = string.IsNullOrEmpty(stringToAppend) ? 0 : stringToAppend.ToLowerInvariant()[0];
            return number == "1" ? Vowels.Any(x => x == firstCharacter)
                ? $"an{(stringToAppend == null ? string.Empty : $" {stringToAppend}")}"
                : $"a{(stringToAppend == null ? string.Empty : $" {stringToAppend}")}"
                : $"{number}{(stringToAppend == null ? string.Empty : $" {Pluralize(number, stringToAppend)}")}";
        }

    /// <summary>
    /// Returns to its nth form based on <paramref name="number"/>
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
            if (number > 3 && number < 21) return $"{number}th";
            switch (number % 10)
            {
                case 0: return $"{number}th";
                case 1: return $"{number}st";
                case 2: return $"{number}nd";
                case 3: return $"{number}rd";
                default: return $"{number}th";
            };
        }
        /// <summary>
        /// Returns to its nth form based on <paramref name="number"/>
        /// </summary>
        /// <paramref name="number" />Number in string format. Must be zero or positive
        /// If equals 0 returns 0th
        /// If 1 returns 1st
        /// If 2 returns 2nd
        /// If 3 returns 3rd
        /// Else returns number+th
        /// <returns>string Nth number</returns>
        public static string Nth(string number) => Nth(int.Parse(number));
    }
}