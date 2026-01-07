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
        //0: language selector
        new Line("Play the game in English!", "Play the game in English!", 21.6f), // language selector: both lines are the same
        new Line("Spiele das Spiel auf Deutsch!", "Spiele das Spiel auf Deutsch!", 21.6f), // language selector: both lines are the same

        //1: intro scene
        new Line("Oh my goodness — this is incredible news! You’re awake! Welcome back, We’ve all been rooting for you!", "Oh mein Gott – das sind unglaubliche Neuigkeiten! Du bist wach! Willkommen zurück. Wir haben alle die Daumen für dich gedrückt", 21.6f),
        new Line("Good morning! I'm glad that you woke up. We gotta check your vitals first however.", "Guten Morgen! Ich freue mich, dass du aufgestanden bist. Wir sollten zuerst deine Vitalien prüfen.", 21.6f),

        //2: demonstration scene
        new Line("AI didn’t replace our humanity — it amplified it!", "KI hat unsere Menschlichkeit nicht ersetzt – sie hat sie verstärkt!", 21.6f),
        new Line("We can't trust these walking hallucination machines!", "Wir können diesen wandelnden Halluzinationsboxen nicht vertrauen!", 21.6f),

        //3: purity scenes
        new Line("I like it!", "Finde ich gut!", 21.6f),
        new Line("Not really a fan...", "Nicht wirklich mein Fall...", 21.6f)
    };
}
