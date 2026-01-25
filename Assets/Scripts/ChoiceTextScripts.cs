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
        //0: tutorial
        new Line("This is the left option. Move here to select it!", "Das ist die linke Auswahl. Bewege dich hierhin, um sie auszuwählen!", 21.6f),
        new Line("This is the right option. Move here to select it!", "Das ist die rechte Auswahl. Bewege dich hierhin, um sie auszuwahlen!", 21.6f),

        //1: intro scene
        new Line("Oh my goodness — this is incredible news! You’re awake! Welcome back, We’ve all been rooting for you!", "Oh mein Gott – das sind unglaubliche Neuigkeiten! Sie sind wach! Willkommen zurück. Wir haben alle die Daumen für Sie gedrückt!", 19f),
        new Line("Good morning! I'm glad that you woke up. We gotta check your vitals first however.", "Guten Morgen. Ich freue mich, dass du aufgewacht bist. Wir müssen zuerst deine Vitalwerte prüfen.", 20f),

        //2: demonstration scene
        new Line("Move forward. End the panic.", "Vorwärts. Schluss mit der Panik.", 21.6f),
        new Line("Protect what’s real. Draw the line.", "Schützt das Echte. Zieht die Grenze.", 21.6f),

        //3: purity scenes
        new Line("Looks fine to me!", "Hätte ich kein Problem damit!", 22f),
        new Line("I think I would have an issue with it...", "Ich denke schon das ich ein Problem damit hätte...", 21.6f),

        //4: voting booth
        new Line("KEEP", "LASSEN", 22f),
        new Line("FLAG", "MELDEN", 22f),

        //5: title screen and language select, both sides are the same
        new Line("Let's begin!", "Let's begin!", 21.6f),
        new Line("Los geht's!", "Los geht's!", 21.6f),
    };
}
