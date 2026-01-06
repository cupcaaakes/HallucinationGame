using System.Collections.Generic;

public static class TextboxScripts
{
    public struct Line
    {
        public string en;
        public string de;
        public float fontSize;

        public Line(string en, string de, float fontSize)
        {
            this.en = en;
            this.de = de;
            this.fontSize = fontSize;
        }

        public string Get(bool useGerman) => useGerman ? de : en;
    }

    public static readonly List<Line> Lines = new()
    {
        new Line(
            // you said you don’t mind repeating this verbatim
            "Move to the door of the language you wish to play in. / Bewege dich zu der Türe von der Sprache, in der du das Spiel spielen möchtest.",
            "Move to the door of the language you wish to play in. / Bewege dich zu der Türe von der Sprache, in der du das Spiel spielen möchtest.",
            21.6f
        ),
        new Line(
            "Ah, now what a serene place you've found yourself in! But clearly you aren't the biggest people person, aren't you?",
            "Mann, an was für einen hübschen Ort du dich wieder gefunden hast! Aber du bist nicht gerade extrovertiert, oder?",
            21.6f
        ),
        new Line(
            "Huh, you clearly could've sworn that those were human sounding voices, but on closer inspection, they don't appear to be human at all... Or what do you think?",
            "Hm, du hättest schwören können, das wären menschliche Stimmen gewesen… aber bei genauerem Hinsehen wirken sie gar nicht menschlich… oder was meinst du?",
            21.6f
        ),
        new Line(
            "It's probably best not to disturb them... Yet.",
            "Am besten stören wir sie lieber nicht… noch nicht.",
            26f
        )
    };
}
