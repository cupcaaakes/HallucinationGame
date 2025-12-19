using System.Collections.Generic;

public static class ChoiceTextScripts
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
            "Follow the calm waves?",
            21.6f
        ),
        new Line(
            "See what all the commotion is all about?",
            21.6f
        )
    };
}
