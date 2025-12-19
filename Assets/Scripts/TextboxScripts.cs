using System.Collections.Generic;

public static class TextboxScripts
{
    public struct Line
    {
        public string text;
        public float fontSize;
        public Line(string text, float fontSize)
        {
            this.text = text;
            this.fontSize = fontSize;
        }
    }

    public static readonly List<Line> Lines = new()
    {
        new Line(
            "After what felt like an eternity, you end up in front of two doors. The one on the left has the calm sounds of waves behind it, while the one on the right sounds like a bunch of voices talking over one another.",
            21.6f
        ),
        new Line(
            "Ah, now what a serene place you've found yourself in! But clearly you aren't the biggest people person, aren't you?",
            21.6f
        ),
        new Line(
            "Huh, you clearly could've sworn that those were human sounding voices, but on closer inspection, they don't appear to be human at all... Or what do you think?",
            21.6f
        ),
        new Line(
            "It's probably best not to disturb them... Yet.",
            26f
        )
    };
}
