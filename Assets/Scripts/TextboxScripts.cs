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
        new Line( //2: AI doctor 1
            "The doctor seems almost delighted that you’ve woken up. ‘Welcome to the future!’ he says cheerfully. ‘A surprising number of us are actually AI—myself included. Could you tell?’",
            "Der Arzt wirkt fast begeistert, dass du aufgewacht bist. ‚Willkommen in der Zukunft!‘ sagt er fröhlich. ‚Erstaunlich viele von uns sind tatsächlich KI – mich eingeschlossen. Hättest du es gemerkt?‘",
            21.6f
        ),
        new Line( //3: human doctor 1
            "The doctor takes your vitals, and everything seems to be in order. 'Welcome to the future. Some that appear human are actually AI nowadays... But we humans aren't completely redundant yet.', he says.",
            "Der Arzt nimmt deine Vitalitäten, alles scheint okay zu sein. 'Willkommen in der Zukunft. Einige der Leute hier sind eigentlich KI... Aber wir Menschen sind noch nicht komplett überflüssig.', sagt er.",
            21.2f
        ),
        new Line( //4: AI doctor 2
            "He continues almost eagerly. 'Human-like AI has made life safer, healthier, and far more efficient. Of course, not everyone is thrilled about it—there are some protests outside. Change can be unsettling.'",
            "Er fährt fast schon begeistert fort. ‚Menschlich wirkende KI hat das Leben sicherer, gesünder und deutlich effizienter gemacht. Natürlich sind nicht alle davon begeistert – draußen gibt es ein paar Proteste. Veränderung kann verunsichern.‘",
            20.5f
        ),
        new Line( //5: human doctor 2
            "He looks at you with a worried look on his face. 'You can't trust people these days because of those AIs that appear human. Most believe that they have some sort of agenda...'",
            "Er schaut dich etwas besorgt an. 'Man kann heute keinem mehr vertrauen wegen diesen menschlich wirkenden KIs... Die meisten gehen davon aus, dass die was gegen uns planen.'",
            21.6f
        ),
        new Line( //6: AI doctor 3
            "'You should go outside and see it for yourself,' he suggests warmly. 'It’s all part of progress, after all.' You decide to do just that.",
            "‚Du solltest dir das draußen selbst ansehen‘, schlägt er freundlich vor. ‚Es gehört schließlich alles zum Fortschritt.‘ Also entscheidest du dich, genau das zu tun.",
            21.6f
        ),
        new Line( //7: human doctor 3
            "'You better go outside and take a look for yourself', he says. So you decide to do just that.",
            "'Am Besten gehst du raus und machst dir selber ein Bild davon', sagt er. Also entscheidest du dich, genau dies zu tun.",
            21.6f
        ),
    };
}
