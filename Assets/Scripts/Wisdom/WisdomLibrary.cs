/// <summary>One Akan proverb: the Twi original, a literal English rendering and what it teaches.</summary>
public sealed class Proverb
{
    public readonly string Twi;
    public readonly string Literal;
    public readonly string Meaning;

    public Proverb(string twi, string literal, string meaning)
    {
        Twi = twi;
        Literal = literal;
        Meaning = meaning;
    }
}

/// <summary>
/// The wisdom Ananse gathers: each level's gourd holds one proverb, revealed when it is
/// collected and kept in the Gourds of Wisdom collection. The order roughly follows the
/// journey, ending with the moral of Ananse's own tale.
/// </summary>
public static class WisdomLibrary
{
    private static readonly Proverb[] Proverbs =
    {
        new Proverb("Kakra kakra, akokɔ nom nsuo.",
                    "Little by little, the hen drinks water.",
                    "Patience. Small steps add up to great things."),
        new Proverb("Obi nkyerɛ abofra Nyame.",
                    "No one shows a child the Supreme Being.",
                    "Some truths are plain for everyone to see."),
        new Proverb("Nea ɔnnim no sua a, ohu.",
                    "The one who does not know can know by learning.",
                    "Ignorance is never permanent if you are willing to learn."),
        new Proverb("Tikoro nkɔ agyina.",
                    "One head does not go into council.",
                    "Seek advice. Good decisions are made together."),
        new Proverb("Dua baako gye mframa a, ɛbu.",
                    "When a lone tree faces the wind, it breaks.",
                    "There is strength in standing together."),
        new Proverb("Woforo dua pa a, na yɛpia wo.",
                    "When you climb a good tree, you are given a push.",
                    "Work towards something worthy and others will help you."),
        new Proverb("Abɔfra bɔ nwa, na ɔmmɔ akyekyedeɛ.",
                    "A child may crack a snail's shell, but not a tortoise's.",
                    "Know your limits, and grow into bigger tasks."),
        new Proverb("Aboa a onni dua, Onyame na ɔpra ne ho.",
                    "For the animal with no tail, God drives away the flies.",
                    "Help comes to those who have no means of their own."),
        new Proverb("Ahwenepa nkasa.",
                    "Precious beads make no noise.",
                    "True worth does not need to boast. (Are you listening, Ananse?)"),
        new Proverb("Sɛ wo werɛ fi na wosan kɔfa a, yenkyi.",
                    "It is not wrong to go back for what you forgot.",
                    "Sankofa. Learn from the past to build the future."),
        new Proverb("Nkrabea nni kwatibea.",
                    "Destiny has no shortcut.",
                    "There is no way around the path; you must walk it."),
        new Proverb("Obi nnim obrempon ahyease.",
                    "No one knows the beginnings of a great person.",
                    "Never judge someone by where they started."),
        new Proverb("Ɔbaakofoɔ mmu man.",
                    "One person does not rule a nation.",
                    "Leadership needs the help of many."),
        new Proverb("Ɔkwan atwa asuo, asuo atwa ɔkwan. Ɔpanin ne hwan?",
                    "The path crossed the river, the river crossed the path. Which is the elder?",
                    "The path was made to meet the river. Respect what came before you."),
        new Proverb("Nyansa nni onipa baako ti mu.",
                    "Wisdom is not in one person's head.",
                    "Ananse learned it at last: wisdom grows when it is shared."),
    };

    /// <summary>Number of proverbs (one per level).</summary>
    public static int Count => Proverbs.Length;

    /// <summary>The proverb held by <paramref name="level"/>'s gourd, or null if it has none.</summary>
    public static Proverb ForLevel(int level)
    {
        return level >= 1 && level <= Proverbs.Length ? Proverbs[level - 1] : null;
    }

    /// <summary>
    /// True once the player has beaten <paramref name="level"/>. Uses level progress rather than a
    /// separate save, so players who finished levels before this feature keep their proverbs.
    /// </summary>
    public static bool IsCollected(int level)
    {
        return ForLevel(level) != null && level < LevelProgress.HighestUnlocked;
    }

    /// <summary>How many proverbs the player has collected so far.</summary>
    public static int CollectedCount()
    {
        int collected = 0;
        for (int level = 1; level <= Proverbs.Length; level++)
        {
            if (IsCollected(level)) collected++;
        }
        return collected;
    }
}
