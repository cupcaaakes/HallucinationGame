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
            "Play the game in English!",
            21.6f
        ),
        new Line(
            "Spiele das Spiel auf Deutsch!",
            21.6f
        )
    };
}
