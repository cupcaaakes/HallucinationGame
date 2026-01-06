using System.Collections.Generic;

public static class ChoiceTextScripts
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
        new Line("Play the game in English!", "Play the game in English!", 21.6f), // language selector: both lines are the same
        new Line("Spiele das Spiel auf Deutsch!", "Spiele das Spiel auf Deutsch!", 21.6f), // language selector: both lines are the same
    };
}
