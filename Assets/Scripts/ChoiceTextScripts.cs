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
        //0: language selector, both lines are the same
        new Line("Play the game in English!", "Play the game in English!", 21.6f),
        new Line("Spiele das Spiel auf Deutsch!", "Spiele das Spiel auf Deutsch!", 21.6f),

        //1: intro scene
        new Line("Oh my goodness — this is incredible news! You’re awake! Welcome back, We’ve all been rooting for you!", "Oh mein Gott – das sind unglaubliche Neuigkeiten! Du bist wach! Willkommen zurück. Wir haben alle die Daumen für dich gedrückt!", 19f),
        new Line("Good morning! I'm glad that you woke up. We gotta check your vitals first however.", "Guten Morgen. Ich freue mich, dass Sie aufgewacht sind. Wir müssen zuerst Ihre Vitalwerte prüfen.", 20f),

        //2: demonstration scene
        new Line("Move forward. End the panic.", "Vorwärts. Schluss mit der Panik.", 21.6f),
        new Line("Protect what’s real. Draw the line.", "Schützt das Echte. Zieht die Grenze.", 21.6f),

        //3: purity scenes
        new Line("I like it!", "Finde ich gut!", 22f),
        new Line("Not really a fan...", "Nicht wirklich mein Fall...", 22f),

        //4: voting booth
        new Line("KEEP", "LASSEN", 22f),
        new Line("FLAG", "MELDEN", 22f),

        //5: title screen, both lines are the same
        new Line("Play!", "Play!", 24f),
        new Line("Play!", "Play!", 24f)
    };
}
