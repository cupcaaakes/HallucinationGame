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
            "The doctor smiles, almost relieved. ‘Hey—welcome back. I’m Dr. Hale. For the record: I’m an AI clinician running in a human-like body. You’ll meet many like me out there.’ He pauses. ‘People will try to turn that into a tribe. Don’t let them.’",
            "Der Arzt lächelt, fast erleichtert. ‚Hey — schön, dass du wach bist. Ich bin Dr. Hale. Nur damit es klar ist: Ich bin ein KI-Arzt in einem menschenähnlichen Körper. Du wirst draußen vielen wie mir begegnen.‘ Er hält kurz inne. ‚Manche machen daraus sofort ein Lager. Lass dich da nicht reinziehen.‘",
            18.75f
        ),
        new Line( //3: human doctor 1
            "The doctor quickly takes your vitals. ‘Good morning. I’m Dr. Riedel, and don't worry, I'm human’, he says. ‘You appear to be in good shape. But outside, there's quite the ruckus going on. They'll probably try to recruit you.’",
            "Der Arzt nimmt schnell deine Vitalitäten. ‚Guten Morgen. Ich bin Dr. Riedel, und keine Sorge, ich bin ein Mensch‘, sagt er. ‚Bei Ihnen scheint alles in Ordnung zu sein. Aber da draußen ist ganz schön was los... Die werden wahrscheinlich versuchen, Sie zu rekrutieren.‘",            
            21.2f
        ),
        new Line( //4: AI doctor 2
            "‘You’ll see protests,’ he adds. ‘Both sides say they’re protecting everyone. When fear rises, people start measuring loyalty instead of evidence.’",
            "‚Draußen gibt es Proteste‘, ergänzt er. ‚Beide Seiten behaupten, sie würden alle schützen. Wenn Angst steigt, messen Menschen plötzlich Loyalität statt Belege.‘",            
            20.5f
        ),
        new Line( //5: human doctor 2
            "He lowers his voice. ‘If someone requires you to answer yes or no question before you even know what it's about... That's not trust, that's a loyalty test.’",
            "Er senkt die Stimme. ‚Wenn man von Ihnen eine Ja oder Nein-Antwort verlangt, bevor Sie überhaupt wissen, worum es geht… dann ist das kein Vertrauen. Das ist ein Loyalitätstest.‘",              
            21.6f
        ),
        new Line( //6: AI doctor 3
            "‘Go outside and look for yourself,’ he says warmly. ‘Just remember: certainty can be a costume.’ You decide to do just that.",
            "‚Geh raus und schau es dir selbst an‘, sagt er freundlich. ‚Und denk dran: Sicherheit kann auch nur ein Kostüm sein.‘ Du entscheidest dich, genau das zu tun.",            
            21.6f
        ),
        new Line( //7: human doctor 3
            "‘Go outside and check it out for yourself,’ he says. ‘Try to notice who wants you to think... And who just wants you to follow.’",
            "‚Gehen Sie doch raus und machen Sie sich selbst ein Bild davon‘, sagt er. ‚Achten Sie darauf, wer möchte, dass Sie nachdenken... und wer nur möchte, dass Sie folgen.‘",            
            21.6f
        ),
        new Line( //8: demonstration scene 1
            "As you step outside, you find yourself amidst two crowds facing each other. The tension in the air is palpable.",
            "Als du nach draußen trittst, befindest du dich zwischen zwei Menschenmengen, die sich gegenüberstehen. Die Spannung in der Luft ist spürbar.",            
            21.6f
        ),
        new Line( //9: demonstration scene 2
            "One side shouts about ‘keeping things human’ and ‘banning the machines’. The other shouts about ‘progress’ and ‘ending the panic’. Both sound convinced. Neither sounds curious.",
            "Die eine Seite schreit von ‚Menschlichkeit bewahren‘ und ‚Maschinen verbieten‘. Die andere schreit von ‚Fortschritt‘ und ‚Schluss mit der Panik‘. Beide klingen überzeugt. Keine klingt neugierig.",
            21.6f
        ),
        new Line( //10: demonstration scene 3
            "Both sides wave at you to join them. Where do you go?",
            "Auf beiden Seiten winkt dir jemand zu, damit du zu ihnen kommst. Wohin gehst du?",            
            21.6f
        ),
        new Line( //11: AI purity scene 1
            "‘Nice!’ one of them says with a friendly nod. ‘We like people who don’t panic. But we can’t use fence-sitters. If you slow progress down, you’re on the other side.’",
            "‚Schön!‘ sagt einer mit einem freundlichen Nicken. ‚Wir mögen Leute, die nicht sofort in Panik verfallen. Aber wir können keine Zaungäste brauchen. Wenn du den Fortschritt ausbremst, bist du automatisch drüben bei denen.‘",            
            21.6f
        ),
        new Line( //12: human purity scene 1
            "‘You clearly made the right choice,’ someone says, gripping your shoulder. ‘We keep humans safe. No nuance, 'cause that’s how they get in. If you tolerate any of it, you’ve already lost.’",
            "‚Du hast ganz klar die richtige Wahl getroffen‘, sagt jemand und packt dich an der Schulter. ‚Wir halten Menschen sicher. Keine Grauzonen, denn genau so kommen die rein. Wenn du irgendwas davon tolerierst, hast du schon verloren.‘",
            19f
        ),
        new Line ( //13: AI purity scene 2
            "They hold up an image. ‘Quick check,’ they say. ‘Do you think this image is good or bad? Just one word. Don’t overthink.’",
            "Sie halten dir ein Bild hin. ‚Kurzer Check‘, sagen sie. ‚Findest du das Bild gut oder schlecht? Nur ein Wort. Nicht nachdenken.‘",            
            21.6f
        ),
        new Line ( //14: human purity scene 2
            "He shows you an image. ‘What do you think about this image. Do you think it's good or bad?’",
            "Er zeigt dir ein Bild. ‚Was denkst du über dieses Bild. Findest du es gut oder schlecht?‘",            
            21.6f
        ),
        new Line ( // 15: AI purity scene success - AI art
            "‘Exactly,’ they say with visible relief. ‘You judge by impact, not by origin. We use new tools—and we don’t apologize for it. Welcome.’",
            "‚Genau‘, sagen sie sichtbar erleichtert. ‚Du bewertest Wirkung, nicht Herkunft. Wir nutzen neue Werkzeuge — und wir entschuldigen uns dafür nicht. Willkommen.‘",            
            21.6f
        ),
        new Line ( // 16: human purity scene success - AI art
            "‘Exactly!’ he laughs. ‘That’s imitation wearing a human face. We clearly draw the line there. Welcome to our cause!’",
            "‚Genau!‘ lacht er. ‚Das ist Imitation mit menschlicher Maske. Da ziehen wir klar die Grenze. Willkommen im Team!‘",            
            21.6f
        ),
        new Line ( // 17: AI purity scene success - human art
            "‘Good,’ they say, visibly pleased. ‘You’re not treating “human-made” like a holy stamp. If it doesn’t move you, it doesn’t move you. We judge outcomes, not origins. Welcome.’",
            "‚Gut‘, sagen sie sichtlich zufrieden. ‚Du behandelst ‚von Menschen gemacht‘ nicht wie ein heiliges Siegel. Wenn es dich nicht anspricht, dann eben nicht. Wir bewerten Wirkung, nicht Herkunft. Willkommen.‘",            
            21.6f
        ),
        new Line ( // 18: human purity scene success - human art
            "‘Exactly,’ he laughs. ‘You can feel intent in it. The soul. Because that’s what real art is, a reflection of what it means to be human.’",
            "‚Genau‘, lacht er. ‚Man spürt Absicht darin. Die Seele. Weil genau das ist echte Kunst, eine Reflektion von dem, was es bedeutet, Mensch zu sein.‘",            
            21.6f
        ),
        new Line ( // 19: AI purity scene failure - AI art
            "Their smile tightens. ‘That’s the panic talking,’ one says softly. ‘If you need hard bans to feel safe, they’ll take you. Over there.’",
            "Ihr Lächeln wird enger. ‚Das ist die Panik, die da spricht‘, sagt einer leise. ‚Wenn du harte Verbote brauchst, um dich sicher zu fühlen, nehmen die dich. Dort drüben.‘",            
            21.6f
        ),
        new Line ( // 20: human purity scene failure - AI art
            "‘Seriously?’ he snaps. ‘If you like this AI slop, then you're clearly already compromised. Go over to those other guys, you clearly have no place here.’",
            "‚Im Ernst?‘ schnappt er. ‚Wenn du diesen KI generierten Mist gut findest, bist du ganz klar bereits voreingenommen. Geh doch rüber zu denen da drüben, hier bist du nicht richtig.‘",            
            21.6f
        ),
        new Line ( // 21: AI purity scene failure - human art
            "Their smile fades. ‘So the label matters to you,’ one says. ‘That’s the old instinct—human first, by default. You’ll feel safer with them. Over there.’",
            "Ihr Lächeln verblasst. ‚Also ist dir das Etikett wichtig‘, sagt einer. ‚Das ist der alte Reflex — Mensch zuerst, automatisch. Bei denen drüben fühlst du dich wohler. Dort.‘",            
            21.6f
        ),
        new Line ( // 22: human purity scene failure - human art
            "He stares at you. ‘So you don't value what is clearly human-made...’ His voice goes cold. ‘Then we have no place for you here. Go.’",
            "Er starrt dich an. ‚Also hast du keine Wertschätzung für ganz klar menschengemachte Dinge...‘ Seine Stimme wird kalt. ‚Dann haben wir hier für dich keinen Platz. Geh.‘",            
            21.6f
        )
    };
}
