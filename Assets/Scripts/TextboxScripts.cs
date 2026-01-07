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
            "Der Arzt nimmt deine Vitalitäten, alles scheint okay zu sein. ‚Willkommen in der Zukunft. Einige der Leute hier sind eigentlich KI... Aber wir Menschen sind noch nicht komplett überflüssig.‘, sagt er.",
            21.2f
        ),
        new Line( //4: AI doctor 2
            "He continues almost eagerly. 'Human-like AI has made life safer, healthier, and far more efficient. Of course, not everyone is thrilled about it—there are some protests outside. Change can be unsettling.'",
            "Er fährt fast schon begeistert fort. ‚Menschlich wirkende KI hat das Leben sicherer, gesünder und deutlich effizienter gemacht. Natürlich sind nicht alle davon begeistert – draußen gibt es ein paar Proteste. Veränderung kann verunsichern.‘",
            20.5f
        ),
        new Line( //5: human doctor 2
            "He looks at you with a worried look on his face. 'You can't trust people these days because of those AIs that appear human. Most believe that they have some sort of agenda...'",
            "Er schaut dich etwas besorgt an. ‚Man kann heute keinem mehr vertrauen wegen diesen menschlich wirkenden KIs... Die meisten gehen davon aus, dass die was gegen uns planen.‘",
            21.6f
        ),
        new Line( //6: AI doctor 3
            "'You should go outside and see it for yourself,' he suggests warmly. 'It’s all part of progress, after all.' You decide to do just that.",
            "‚Du solltest dir das draußen selbst ansehen‘, schlägt er freundlich vor. ‚Es gehört schließlich alles zum Fortschritt.‘ Also entscheidest du dich, genau das zu tun.",
            21.6f
        ),
        new Line( //7: human doctor 3
            "'You better go outside and take a look for yourself', he says. So you decide to do just that.",
            "‚Am Besten gehst du raus und machst dir selber ein Bild davon‘, sagt er. Also entscheidest du dich, genau dies zu tun.",
            21.6f
        ),
        new Line( //8: demonstration scene 1
            "As you step outside, you find yourself amidst a crowd of protesters and counter-protestors. The tension in the air is palpable.",
            "Als du nach draußen trittst, befindest du dich mitten in einer Menge von Demonstranten und Gegendemonstranten. Die Spannung in der Luft ist spürbar.",
            21.6f
        ),
        new Line( //9: demonstration scene 2
            "You notice that some protesters are holding signs that say 'Humans First!' and 'AI is a Threat!'. The counter-protestors respond with signs like 'Embrace the Future!' and 'AI for a Better Tomorrow!'.",
            "Du bemerkst, dass einige Demonstranten Schilder mit den Aufschriften ‚Menschen zuerst!‘ und ‚KI ist eine Bedrohung!‘ halten. Die Gegendemonstranten antworten mit Schildern wie ‚Umarme die Zukunft!‘ und ‚KI für ein besseres Morgen!‘.",
            18.75f
        ),
        new Line( //10: demonstration scene 3
            "Both sides have someone waving at you to join them. Where do you wanna go?",
            "Auf beiden Seiten winkt jemand dir zu, damit du zu ihrer Seite gehst. Wohin wirst du gehen?",
            21.6f
        ),
        new Line( //11: AI purity scene 1
            "‘Nice!’ one of the counter-protesters says with a friendly nod. ‘We’re glad you’re open to the future. Still, we like to get a feel for who’s standing with us.’",
            "‚Schön!‘ sagt einer der Gegendemonstranten mit einem freundlichen Nicken. ‚Wir freuen uns, dass du offen für die Zukunft bist. Trotzdem möchten wir ein Gefühl dafür bekommen, wer hier mit uns steht.‘",            
            21.6f
        ),
        new Line( //12: human purity scene 1
            "'You made the right choice,' a fellow protester says, clapping you on the back. 'We have to stand up for humanity. But first, we gotta make sure that you are truly one of us.'",
            "‚Du hast die richtige Wahl getroffen‘, sagt dir ein Mitdemonstrant und klopft dir auf den Rücken. ‚Wir müssen für die Menschheit einstehen. Aber zuerst müssen wir sicherstellen, dass du wirklich einer von uns bist.‘",
            21.6f
        ),
        new Line ( //13: AI purity scene 2
            "They smile and hold up a picture in front of you. ‘What do you think of this image?’ they ask gently. ‘Do you like it?’",
            "Sie lächeln und halten dir ein Bild vor. ‚Was hältst du von diesem Bild?‘ fragen sie freundlich. ‚Gefällt es dir?‘",            
            21.6f
        ),
        new Line ( //14: human purity scene 2
            "He looks at you critically and holds up a picture in front of you. 'What do you think of this image here? Do you like it?'.",
            "Er schaut dich etwas kritisch an und hält ein Bild vor dich. ‚Was denkst du über dieses Bild hier? Findest du es gut?‘",
            21.6f
        ),
        new Line ( // 15: AI purity scene success - AI art
            "‘Exactly,’ they say with an approving nod. ‘You see it for what it is — creative, expressive, and valid. Welcome. You’re with us.’",
            "‚Genau‘, sagen sie mit einem zustimmenden Nicken. ‚Du siehst es so, wie es ist – kreativ, ausdrucksstark und legitim. Willkommen. Du gehörst zu uns.‘",            
            21.6f
        ),
        new Line ( // 16: human purity scene success - AI art
            "'You get it!' he exclaims. 'This clearly ain't art. Art requires soul, something these tin cans don't have. Welcome!'",
            "‚Du verstehst es einfach!‘ ruft er. ‚Das hier ist ganz klar keine Kunst. Um Kunst zu machen braucht man eine Seele, und das haben diese Konservendosen nicht. Willkommen!‘",
            21.6f
        ),
        new Line ( // 17: AI purity scene success - human art
            "‘That’s fair,’ they say calmly. ‘Human-made art has its place, but it’s not the only form of creativity anymore. You’re welcome with us.’",
            "‚Das ist fair‘, sagen sie ruhig. ‚Von Menschen gemachte Kunst hat ihren Platz, aber sie ist nicht mehr die einzige Form von Kreativität. Du bist bei uns willkommen.‘",            
            21.6f
        ),
        new Line ( // 18: human purity scene success - human art
            "'Exactly!', he says excitedly. 'You can tell that this was created with love behind it, and not just some algorithm.",
            "‚Genau!‘, sagt er begeistert. ‚Man kann ganz klar erkennen, dass dieses Werk hier mit Liebe gemacht wurde und nicht nur irgendeinem Algorithmus.‘",
            21.6f
        ),
        new Line ( // 19: AI purity scene failure - AI art
            "They hesitate for a moment. ‘Alright,’ one of them says gently. ‘If you can’t accept this, you’ll probably feel more comfortable with the humans. They’re over there.’",
            "Sie zögern einen Moment. ‚Alles klar‘, sagt einer von ihnen sanft. ‚Wenn du das nicht akzeptieren kannst, fühlst du dich bei den Menschen vermutlich wohler. Die sind dort drüben.‘",
            21.6f
        ),
        new Line ( // 20: human purity scene failure - AI art
            "'Are you serious?!', he exlaims. 'This is CLEARLY some AI slop. Go over to those other guys if you don't have a taste for REAL art.",
            "‚Ist das dein Ernst?!‘, ruft er entrüstet. ‚Das hier ist GANZ KLAR nur KI generierter Müll. Geh doch zu den anderen Typen rüber wenn du keinen Geschmack für ECHTE Kunst hast.‘",
            21.6f
        ),
        new Line ( // 21: AI purity scene failure - human art
            "Their expression softens. ‘That sounds like a human-first view,’ they say politely. ‘You should join them — this space might not be for you.’",
            "Ihr Blick wird weicher. ‚Das klingt nach einer sehr menschlichen Sichtweise‘, sagen sie höflich. ‚Dann solltest du zu ihnen gehen – dieser Ort ist vielleicht nichts für dich.‘",
            21.6f
        ),
        new Line ( // 22: human purity scene failure - human art
            "'Are you joking?!', he shouts. 'Would you prefer some of their slop instead of something created with a soul? Go over there then if you don't like what we have to offer!'",
            "'Machst du Witze?!', schreit er dich an. 'Würdest du etwas von den ihrem Mist etwa bevorzugen? Dann geh doch rüber wenn dir nicht gefällt was wir haben!'",
            21.6f
        )
    };
}
