using System.Globalization;
using System.Linq;
using Xunit;

namespace Contoso.Extensions
{
    public class GrammarTest
    {
        [Fact]
        public void Vowels()
        {
            Assert.True(Enumerable.SequenceEqual(new[] { 'a', 'e', 'i', 'o', 'u' }, Grammar.Vowels));
        }

        [Fact]
        public void WasWere()
        {
            Assert.Equal("none were", actual: Grammar.WasWere(0)); Assert.Equal("none were", actual: Grammar.WasWere("0"));
            Assert.Equal("1 was", actual: Grammar.WasWere(1)); Assert.Equal("1 was", actual: Grammar.WasWere("1"));
            Assert.Equal("2 were", actual: Grammar.WasWere(2)); Assert.Equal("2 were", actual: Grammar.WasWere("2"));

            Assert.Equal("none were append", actual: Grammar.WasWere(0, "append")); Assert.Equal("none were append", actual: Grammar.WasWere("0", "append"));
            Assert.Equal("1 was append", actual: Grammar.WasWere(1, "append")); Assert.Equal("1 was append", actual: Grammar.WasWere("1", "append"));
            Assert.Equal("2 were append", actual: Grammar.WasWere(2, "append")); Assert.Equal("2 were append", actual: Grammar.WasWere("2", "append"));
        }

        [Fact]
        public void Pluralize()
        {
            Assert.Equal("", actual: Grammar.Pluralize(1, null)); Assert.Equal("", actual: Grammar.Pluralize("1", null));
            Assert.Equal("", actual: Grammar.Pluralize(2, null)); Assert.Equal("", actual: Grammar.Pluralize("2", null));
            Assert.Equal("noun", actual: Grammar.Pluralize(1, "noun")); Assert.Equal("noun", actual: Grammar.Pluralize("1", "noun"));
            Assert.Equal("nouns", actual: Grammar.Pluralize(2, "noun")); Assert.Equal("nouns", actual: Grammar.Pluralize("2", "noun"));
        }

        [Fact]
        public void Possesive()
        {
            Assert.Equal("", actual: Grammar.Possesive(null));
            Assert.Equal("noun's", actual: Grammar.Possesive("noun"));
            Assert.Equal("nouns", actual: Grammar.Possesive("nouns"));
        }

        [Fact]
        public void HeShe()
        {
            Assert.Equal("they", actual: Grammar.HeShe(null));
            Assert.Equal("he", actual: Grammar.HeShe("male"));
            Assert.Equal("she", actual: Grammar.HeShe("female"));
        }

        [Fact]
        public void HeSheHas()
        {
            Assert.Equal("they have", actual: Grammar.HeSheHas(null));
            Assert.Equal("he has", actual: Grammar.HeSheHas("male"));
            Assert.Equal("she has", actual: Grammar.HeSheHas("female"));
        }

        [Fact]
        public void HimHer()
        {
            Assert.Equal("them", actual: Grammar.HimHer(null));
            Assert.Equal("him", actual: Grammar.HimHer("male"));
            Assert.Equal("her", actual: Grammar.HimHer("female"));
        }

        [Fact]
        public void HisHers()
        {
            Assert.Equal("theirs", actual: Grammar.HisHers(null));
            Assert.Equal("his", actual: Grammar.HisHers("male"));
            Assert.Equal("hers", actual: Grammar.HisHers("female"));
        }

        [Fact]
        public void PluralizePhrase()
        {
            Assert.Equal("1", actual: Grammar.PluralizePhrase(1, null)); Assert.Equal("1", actual: Grammar.PluralizePhrase("1", null));
            Assert.Equal("1 noun", actual: Grammar.PluralizePhrase(1, "noun")); Assert.Equal("1 noun", actual: Grammar.PluralizePhrase("1", "noun"));
            Assert.Equal("2 nouns", actual: Grammar.PluralizePhrase(2, "noun")); Assert.Equal("2 nouns", actual: Grammar.PluralizePhrase("2", "noun"));
        }

        [Fact]
        public void PluralizePhraseWithArticles()
        {
            Assert.Equal("a", actual: Grammar.PluralizePhraseWithArticles(1, null)); Assert.Equal("a", actual: Grammar.PluralizePhraseWithArticles("1", null));
            Assert.Equal("a noun", actual: Grammar.PluralizePhraseWithArticles(1, "noun")); Assert.Equal("a noun", actual: Grammar.PluralizePhraseWithArticles("1", "noun"));
            Assert.Equal("2 nouns", actual: Grammar.PluralizePhraseWithArticles(2, "noun")); Assert.Equal("2 nouns", actual: Grammar.PluralizePhraseWithArticles("2", "noun"));
            Assert.Equal("an apple", actual: Grammar.PluralizePhraseWithArticles(1, "apple")); Assert.Equal("an apple", actual: Grammar.PluralizePhraseWithArticles("1", "apple"));
            Assert.Equal("2 apples", actual: Grammar.PluralizePhraseWithArticles(2, "apple")); Assert.Equal("2 apples", actual: Grammar.PluralizePhraseWithArticles("2", "apple"));
        }

        [Fact]
        public void Nth()
        {
            Assert.Equal("0th", actual: Grammar.Nth(0)); Assert.Equal("0th", actual: Grammar.Nth("0"));
            Assert.Equal("1st", actual: Grammar.Nth(1)); Assert.Equal("1st", actual: Grammar.Nth("1"));
            Assert.Equal("2nd", actual: Grammar.Nth(2)); Assert.Equal("2nd", actual: Grammar.Nth("2"));
            Assert.Equal("3rd", actual: Grammar.Nth(3)); Assert.Equal("3rd", actual: Grammar.Nth("3"));
            Assert.Equal("4th", actual: Grammar.Nth(4)); Assert.Equal("4th", actual: Grammar.Nth("4"));
            Assert.Equal("21st", actual: Grammar.Nth(21)); Assert.Equal("21st", actual: Grammar.Nth("21"));
        }
    }
}
