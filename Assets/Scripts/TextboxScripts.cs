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
        new Line( //0: language selector: both lines are the same
            "Move to the door of the language you wish to play in. / Bewege dich zu der Türe von der Sprache, in der du das Spiel spielen möchtest.",
            "Move to the door of the language you wish to play in. / Bewege dich zu der Türe von der Sprache, in der du das Spiel spielen möchtest.",
            21.6f
        ),
        new Line( //1: intro scene
            "You've woken up after a long time... On another planet. Two doctors came in to check in on you. Which one do you want to be treated by?",
            "Du bist nach langer Zeit aufgewacht... Auf einem anderen Planeten. Zwei Ärzte kommen herein, um dich zu untersuchen. Von wem möchtest du behandelt werden?",
            21.6f
            ),
        new Line( //2: AI doctor
            "The doctor seems almost delighted that you’ve woken up. ‘Welcome to the future!’ he says cheerfully. ‘A surprising number of us are actually AI—myself included. Could you tell?’",
            "Der Arzt wirkt fast begeistert, dass du aufgewacht bist. ‚Willkommen in der Zukunft!‘ sagt er fröhlich. ‚Erstaunlich viele von uns sind tatsächlich KI – mich eingeschlossen. Hättest du es gemerkt?‘",
            21.6f
        ),
        new Line( //3: human doctor
            "The doctor takes your vitals, and everything seems to be in order. 'Welcome to the future. Some that appear human are actually AI nowadays... But we humans aren't completely redundant yet.', he says.",
            "Der Arzt nimmt deine Vitalitäten, alles scheint okay zu sein. 'Willkommen in der Zukunft. Einige der Leute hier sind eigentlich KI... Aber wir Menschen sind noch nicht komplett überflüssig.', sagt er.",
            21.6f
        ),
        new Line(
            "It's probably best not to disturb them... Yet.",
            "Am besten stören wir sie lieber nicht… noch nicht.",
            26f
        )
    };
}
