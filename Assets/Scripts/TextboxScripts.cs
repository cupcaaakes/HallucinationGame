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
            "Move to the flag of the language you wish to play in. / Bewege dich zu der Flagge von der Sprache, in der du das Spiel spielen möchtest.",
            "Move to the flag of the language you wish to play in. / Bewege dich zu der Flagge von der Sprache, in der du das Spiel spielen möchtest.",
            21.6f
        ),
        new Line( //1: intro scene
            "You've woken up after a long time... On another planet. Two doctors came in to check in on you. Which one do you want to be treated by?",
            "Du bist nach langer Zeit aufgewacht... Auf einem anderen Planeten. Zwei Ärzte kommen herein, um dich zu untersuchen. Von wem möchtest du behandelt werden?",            
            21.6f
            ),
        new Line( //2: AI doctor 1
            "The doctor smiles, almost relieved. ‘Hey—welcome back. I’m Dr. Hale.’ He checks your chart with quick confidence. ‘You’re safe. We’ve got excellent systems here.’ He pauses. ‘Outside, people will try to turn you into a symbol. Don’t let them.’",
            "Der Arzt lächelt, fast erleichtert. ‚Hey — schön, dass Sie wach sind. Ich bin Dr. Hale.‘ Er prüft Ihre Werte mit routinierter Sicherheit. ‚Sie sind in Sicherheit. Wir haben hier hervorragende Systeme.‘ Er hält kurz inne. ‚Draußen werden Leute versuchen, Sie zu einem Symbol zu machen. Lassen Sie das nicht zu.‘",
            19f
        ),
        new Line( //3: human doctor 1
            "The doctor quickly takes your vitals. ‘Good morning. I’m Dr. Riedel’, he says. ‘You appear to be in good shape. But outside, there's quite the ruckus going on. They'll probably try to recruit you.’",
            "Der Arzt nimmt schnell deine Vitalitäten. ‚Guten Morgen. Ich bin Dr. Riedel‘, sagt er. ‚Bei Ihnen scheint alles in Ordnung zu sein. Aber da draußen ist ganz schön was los... Die werden wahrscheinlich versuchen, Sie zu rekrutieren.‘",            
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
            "One side shouts about ‘protecting what’s real’ and ‘drawing a hard line’. The other shouts about ‘moving forward’ and ‘ending the panic’. Both sound convinced. Neither sounds curious.",
            "Die eine Seite schreit von ‚das Echte schützen‘ und ‚klare Grenzen ziehen‘. Die andere schreit von ‚vorwärts gehen‘ und ‚Schluss mit der Panik‘. Beide klingen überzeugt. Keine klingt neugierig.",            
            21.6f
        ),
        new Line( //10: demonstration scene 3
            "Both sides wave at you to join them. Where do you go?",
            "Auf beiden Seiten winkt dir jemand zu, damit du zu ihnen kommst. Wohin gehst du?",            
            21.6f
        ),
        new Line( //11: AI purity scene 1
            "‘Nice!’ one of them says with a friendly nod. ‘We like people who don’t panic. But we can’t use fence-sitters. If you slow things down, you’re on the other side.’",
            "‚Schön!‘ sagt einer mit einem freundlichen Nicken. ‚Wir mögen Leute, die nicht sofort in Panik verfallen. Aber wir können keine Zaungäste brauchen. Wenn du alles ausbremst, bist du automatisch drüben bei denen.‘",            
            21.6f
        ),
        new Line( //12: human purity scene 1
            "‘You clearly made the right choice,’ someone says, gripping your shoulder. ‘We keep people safe. No grey zones, 'cause that’s how it starts. If you tolerate any of it, you’ve already lost.’",
            "‚Du hast ganz klar die richtige Wahl getroffen‘, sagt jemand und packt dich an der Schulter. ‚Wir halten Leute sicher. Keine Grauzonen, denn genau so fängt's an. Wenn du irgendwas davon tolerierst, hast du schon verloren.‘",            
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
            "‘Exactly,’ they say with visible relief. ‘You judge by impact, not by labels. We don’t apologize for moving forward. Welcome.’",
            "‚Genau‘, sagen sie sichtbar erleichtert. ‚Du bewertest Wirkung, nicht Etiketten. Wir entschuldigen uns nicht dafür, dass wir vorwärts gehen. Willkommen.‘",            
            21.6f
        ),
        new Line ( // 16: human purity scene success - AI art
            "‘Exactly!’ he laughs. ‘That’s just manufactured, cold and hollow trash. We clearly draw the line there. Welcome to our cause!’",
            "‚Genau!‘ lacht er. ‚Das ist nur massengefertigter, kalter und hohler Mist. Da ziehen wir klar die Grenze. Willkommen im Team!‘",            
            21.6f
        ),
        new Line ( // 17: AI purity scene success - human art
            "‘Good,’ they say, visibly pleased. ‘You’re not treating “human-made” like a holy stamp. If it doesn’t move you, it doesn’t move you. We judge outcomes, not origins. Welcome.’",
            "‚Gut‘, sagen sie sichtlich zufrieden. ‚Du behandelst ‚von Menschen gemacht‘ nicht wie ein heiliges Siegel. Wenn es dich nicht anspricht, dann eben nicht. Wir bewerten Wirkung, nicht Herkunft. Willkommen.‘",            
            21.6f
        ),
        new Line ( // 18: human purity scene success - human art
            "‘Exactly,’ he laughs. ‘You can feel intent in it. The soul. Because that’s what real art is, a reflection of what it means to be human. You're definitely a great fit for our cause!’",
            "‚Genau‘, lacht er. ‚Man spürt Absicht darin. Die Seele. Weil genau das ist echte Kunst, eine Reflektion von dem, was es bedeutet, Mensch zu sein. Du passt super zu uns und unserer Bewegung!‘",            
            21.6f
        ),
        new Line ( // 19: AI purity scene failure - AI art
            "Their smile tightens. ‘That’s fear talking,’ one says softly. ‘If you need strict purity to feel safe, you’ll fit better with them. Over there.’",
            "Ihr Lächeln wird enger. ‚Das ist Angst, die da spricht‘, sagt einer leise. ‚Wenn du strikte Reinheit brauchst, um dich sicher zu fühlen, passt du besser zu denen. Dort drüben.‘",            
            21.6f
        ),
        new Line ( // 20: human purity scene failure - AI art
            "‘Seriously?’ he snaps. ‘If you can’t see what’s wrong with this, then you’re already part of the problem. Go over there. You don’t belong here.’",
            "‚Im Ernst?‘ schnappt er. ‚Wenn du nicht siehst, was daran falsch ist, dann bist du schon Teil des Problems. Geh rüber. Du gehörst nicht zu uns.‘",            
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
        ),
        new Line ( // 23: staying with the AI crowd
            "Their smiles linger—until someone taps your shoulder. ‘Okay. One more step. Quick intake booth. It’s just for safety.’",
            "Ihr Lächeln bleibt—bis dir jemand auf die Schulter tippt. ‚Okay. Noch ein Schritt. Kurz zur Intake-Station. Nur für die Sicherheit.‘",
            21.6f
        ),
        new Line ( // 24: staying with the human crowd
            "His smile disappears as quickly as it appeared. ‘One more thing,’ he says. ‘It’s important. We do this with everyone. Just follow me into this booth.’",
            "Sein Lächeln verschwindet so schnell, wie es gekommen ist. ‚Noch eine Sache‘, sagt er. ‚Wichtig. Das machen wir mit allen. Folge mir bitte kurz in diese Station.‘",            
            21.6f
        ),
        new Line ( // 25: joining the AI crowd
            "Someone from the other side pulls you in with a bright smile. ‘Don’t worry. They do that to everyone.’",
            "Jemand von der anderen Seite zieht dich mit einem breiten Lächeln zu sich. ‚Keine Sorge. Das machen die bei allen.‘",
            21.6f
        ),
        new Line( // 26: joining the human crowd
            "Someone grips your shoulder. ‘Knew it. You’re one of us. You just had to hear their nonsense yourself.’ He chuckles.",
            "Jemand fasst dir an die Schulter. ‚Wusste ich’s doch. Du bist einer von uns. Du musstest dir nur einmal selbst ihren Unsinn anhören.‘ Er lacht kurz.",            
            21.6f
        ),
        new Line ( // 27: AI crowd moderation request
            "They point at the screen. ‘We’re cleaning up panic. Label to KEEP it. No explanations—speed matters.’",
            "Sie zeigen auf den Bildschirm. ‚Wir räumen Panik auf. Bitte markiere es zu LASSEN. Keine Erklärungen—Tempo zählt.‘",
            21.6f
        ),
        new Line ( // 28: human crowd moderation request
            "He leans in. ‘They hide in half-truths. We need you to FLAG it. Just... do the right thing.’",
            "Er beugt sich näher. ‚Die verstecken sich in Halbwahrheiten. Du musst das MELDEN. Mach einfach... das Richtige.‘",            
            21.6f
        ),
        new Line ( // 29: voting both
            "The kiosk claims it’s showing a post... But the content area is hidden. 'For your safety,' it says. You still have to label it: KEEP (leave it up) or FLAG (report it).",
            "Auf dem Bildschirm steht, dass da ein Beitrag stehen würde... Aber der Inhalt ist ausgeblendet. ‚Zu Ihrer Sicherheit‘, steht dort. Trotzdem sollst du es markieren: LASSEN (sichtbar lassen) oder MELDEN (melden).",            
            21.6f
        ),
        new Line ( // 30: AI - voted KEEP
            "‘Good,’ they say. ‘You didn’t reward fear.’ The crowd noise swells again—strangely synchronized.",
            "‚Gut‘, sagen sie. ‚Du hast Angst nicht belohnt.‘ Der Lärm der Menge schwillt wieder an—seltsam synchron.",
            21.6f
        ),
        new Line ( // 31: humans - voted KEEP
            "‘What are you doing?!’ he hisses. ‘That’s how it spreads!’ He starts reaching for the kiosk, but stops. ‘...Forget it. Just go.’",
            "‚Was machst du da?!‘ zischt er. ‚Genau so verbreitet sich das!‘ Er greift nach dem Display, aber stoppt direkt wieder. ‚...Vergiss es. Geh einfach.‘",
            21.6f
        ),
        new Line ( // 32: AI - voted FLAG
            "They nod, satisfied. ‘Efficient.’ For a second, their smiles look... rehearsed.",
            "Sie nicken zufrieden. ‚Effizient.‘ Für einen Moment wirken ihre Lächeln... einstudiert.",
            21.6f
        ),
        new Line ( // 33: humans - voted FLAG
            "‘Yes!’, he breathes out in relief. ‘That’s how we stay safe.’ He cheers, but you feel a bit queasy.",
            "‚Ja!‘, sagt er erleichtert. ‚So bleiben wir sicher.‘ Er jubelt, aber du fühlst dich etwas komisch.",            
            21.6f
        ),
        new Line( // 34: convergence - leaving the booth
            "Before anyone can say more, you step back from the chaos. The chanting swallows the moment. You slip away into a quieter side alleyway besides the hospital, just to breathe.",
            "Bevor noch jemand etwas sagen kann, trittst du von dem ganzen Chaos zurück. Die Sprechchöre verschlucken den Moment. Du rutschst in einen ruhigeren Seitenbereich neben dem Krankenhaus, nur um kurz Luft zu holen.",            
            21.0f
        ),
        new Line ( // 35: ending thoughts 1
            "Only then you notice it: the chanting behind you isn’t just loud... It’s synchronized. Too synchronized. A few voices repeat the exact same sentence… word for word… like a copied script.",
            "Erst dann fällt es dir auf: Der Lärm hinter dir ist nicht nur laut... Er ist synchron. Zu synchron. Ein paar Stimmen wiederholen exakt denselben Satz… Wort für Wort… wie ein kopiertes Skript.",            
            21.6f
        ),
        new Line ( // 36: ending thoughts 2
            "Your stomach drops. Maybe this was never “two sides of people.” Maybe some of the loudest pressure wasn’t human at all, just something wearing a human shape, pushing you to pick a team.",
            "Dir wird flau. Vielleicht waren das nie „zwei Seiten von Menschen“. Vielleicht war ein Teil von dem Druck gar nicht menschlich, nur etwas, das menschlich aussieht und dich in ein Lager drücken will.",            
            21.6f
        ),
        new Line( // 37: ending fadeout
            "White. Silence. You blink and you’re back at your computer. On the screen are the same slogans, the same urgency, the same hostility. A header reads: GAIA.",
            "Weiß. Stille. Du blinzelst und sitzt wieder vor deinem Rechner. Auf dem Bildschirm stehen dieselben Parolen, dieselbe Dringlichkeit, dieselbe Feindseligkeit. Oben steht: GAIA.",            
            21.2f
        ),
    };
}
