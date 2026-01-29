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
        new Line( //0: tutorial 1
            "Welcome! In GAIA, you have to make some choices. You can either move to the left or to the right to select an option whenever these three dots on the text box aren't visible.",
            "Hallo! In GAIA musst du Entscheidungen treffen. Dazu kannst du entweder nach links oder nach rechts gehen, wenn die drei Punkte auf dem Textfeld nicht sichtbar sind.",
            20.6f
        ),
        new Line( //1: intro scene
            "You've woken up after a long time... On another planet. Two doctors came in to check in on you.",
            "Du bist nach langer Zeit aufgewacht... Auf einem anderen Planeten. Zwei Ärzte kommen herein, um dich zu untersuchen.",
            21.6f
        ),
        new Line( // 2: intro scene 2 - choice
            "The female doctor smiles at you and then looks at her paper chart, while the male doctor assesses you quickly and then checks something on his tablet. Which one would you like to be treated by?",
            "Die Ärztin lächelt dich an und schaut dann auf ihre Patientenakte, während der Arzt dich kurz mit einem Blick untersucht und dann etwas auf seinem Tablet nachsieht. Von wem möchtest du lieber behandelt werden?",
            19f
        ),
        new Line( //3: AI doctor 1
            "The doctor smiles, almost relieved. ‘Hey—welcome back. I’m Dr. Hale.’ He checks your chart with quick confidence. ‘You’re safe. We’ve got excellent systems here.’",
            "Der Arzt lächelt, fast erleichtert. ‚Hey — schön, dass Sie wach sind. Ich bin Dr. Hale.‘ Er prüft deine Werte mit routinierter Sicherheit. ‚Sie sind in Sicherheit. Wir haben hier hervorragende Systeme.‘",
            21.6f
        ),
        new Line( //4: human doctor 1
            "The doctor takes your hand for a moment, checking your pulse the old-fashioned way. 'Welcome back. I'm Dr. Riedel.' She looks tired but genuine. 'Your body's been through hell, but you're here. That counts for something.'",
            "Die Ärztin nimmt kurz deine Hand und prüft deinen Puls auf die altmodische Art. ‚Willkommen zurück. Ich bin Dr. Riedel.' Sie sieht müde aus, aber aufrichtig. ‚Dein Körper hat die Hölle durchgemacht, aber du bist hier. Das zählt.'",
            20f
        ),
        new Line( //5: AI doctor 2
            "‘You’ll see protests,’ he adds. ‘Both sides say they’re protecting everyone. When fear rises, people start measuring loyalty instead of evidence.’",
            "‚Draußen gibt es Proteste‘, ergänzt er. ‚Beide Seiten behaupten, sie würden alle schützen. Wenn Angst steigt, messen Menschen plötzlich Loyalität statt Belege.‘",
            21.6f
        ),
        new Line( //6: human doctor 2
            "She glances at the door. 'Listen... Out there, people are causing quite the ruckus, and they're just looking for blind loyalty.'",
            "Sie blickt zur Tür. ‚Hör zu... Da draußen ist ein ziemliches Chaos. Die Leute da draußen wollen nur blinde Loyalität.'",
            21.6f
        ),
        new Line( //7: AI doctor 3
            "‘Go outside and look for yourself,’ he says warmly. ‘Just remember: certainty can be a costume.’ You decide to do just that.",
            "‚Gehen Sie raus und schauen es selbst an‘, sagt er freundlich. ‚Und denken Sie dran: Sicherheit kann auch nur ein Kostüm sein.‘ Du entscheidest dich, genau das zu tun.",
            21.6f
        ),
        new Line( //8: human doctor 3
    	    "She places a hand on your shoulder. 'Go see for yourself. Just trust your gut. Notice who makes you think harder... and who just makes you angrier.",
            "Sie legt dir eine Hand auf die Schulter. ‚Geh und sieh selbst. Vertraue einfach deinem Bauchgefühl! Achte darauf, wer dich zum Nachdenken bringt... und wer dich nur frustriert macht.",
            21.6f
        ),
        new Line( //9: demonstration scene 1
            "As you step outside, you find yourself amidst two crowds facing each other. The tension in the air is palpable.",
            "Als du nach draußen trittst, befindest du dich zwischen zwei Menschenmengen, die sich gegenüberstehen. Die Spannung in der Luft ist spürbar.",
            21.6f
        ),
        new Line( //10: demonstration scene 2
            "One side shouts about ‘protecting what’s real’ and ‘drawing a hard line’. The other shouts about ‘moving forward’ and ‘ending the panic’. Both sound convinced. Neither sounds curious.",
            "Die eine Seite schreit von ‚das Echte schützen‘ und ‚klare Grenzen ziehen‘. Die andere schreit von ‚vorwärts gehen‘ und ‚Schluss mit der Panik‘. Beide klingen überzeugt. Keine klingt neugierig.",
            21.6f
        ),
        new Line( //11: demonstration scene 3 - choice
            "Both sides wave at you to join them. Where do you go?",
            "Auf beiden Seiten winkt dir jemand zu, damit du zu ihnen kommst. Wohin gehst du?",
            21.6f
        ),
        new Line( //12: AI purity scene 1
            "‘Nice!’ one of them says with a friendly nod. ‘We like people who don’t panic. But we can’t use fence-sitters. If you slow things down, you’re on the other side.’",
            "‚Schön!' sagt einer mit einem freundlichen Nicken. ‚Wir mögen Leute, die nicht sofort in Panik verfallen. Aber wir können keine Zaungäste brauchen. Wenn du alles ausbremst, bist du automatisch drüben bei denen.'",
            21.6f
        ),
        new Line( //13: human purity scene 1
            "‘You clearly made the right choice,’ someone says, gripping your shoulder. ‘We keep people safe. No grey zones, 'cause that’s how it starts. If you tolerate any of it, you’ve already lost.’",
            "‚Du hast ganz klar die richtige Wahl getroffen‘, sagt jemand und packt dich an der Schulter. ‚Wir halten Leute sicher. Keine Grauzonen, denn genau so fängt's an. Wenn du irgendwas davon tolerierst, hast du schon verloren.‘",
            18.75f
        ),
        new Line ( //14: AI purity scene 2
            "Someone holds up an image. ‘Quick check,’ they say. ‘Imagine you'd see this image hanging at your workplace or at your friend's place. Would you have no issue with that?’",
            "Einer hät ein Bild hoch. ‚Kurzer Check‘, sagen sie. ‚Stell dir vor, du siehst dieses Bild bei deinem Arbeitsplatz oder bei einem Freund. Hättest du kein Problem damit?‘",
            21.6f
        ),
        new Line ( //15: human purity scene 2
            "He pulls out an image. 'Real quick... If this was hanging at your workplace or at your friend's place on the wall, would have no issue with that?'",
            "Er zeigt dir ein Bild. 'Ganz kurz...  Wenn das in deinem Büro oder bei einem Freund an der Wand hängen würde, hättest du kein Problem damit?'",
            21.6f
        ),
        new Line ( //16: AI purity scene success - AI art
            "‘Exactly,’ they say with visible relief and exhaling at the same time. ‘You judge by impact, not by labels. We don’t apologize for moving forward. Welcome.’",
            "‚Genau‘, sagen sie sichtbar erleichtert und atmen aus. ‚Du bewertest Wirkung, nicht Etiketten. Wir entschuldigen uns nicht dafür, dass wir vorwärts gehen. Willkommen.‘",
            21.6f
        ),
        new Line ( //17: human purity scene success - AI art
            "‘Exactly!’ he laughs. ‘That’s just manufactured, cold and hollow trash. We clearly draw the line there. Welcome to our cause!’",
            "‚Genau!‘ lacht er. ‚Das ist nur massengefertigter, kalter und hohler Mist. Da ziehen wir klar die Grenze. Willkommen im Team!‘",
            21.6f
        ),
        new Line ( //18: AI purity scene success - human art
            "‘Good,’ they say, visibly pleased. ‘You’re not treating “human-made” like a holy stamp. If it doesn’t move you, it doesn’t move you. We judge outcomes, not origins. Welcome.’",
            "‚Gut‘, sagen sie sichtlich zufrieden. ‚Du behandelst ‚von Menschen gemacht‘ nicht wie ein heiliges Siegel. Wenn es dich nicht anspricht, dann eben nicht. Wir bewerten Wirkung, nicht Herkunft. Willkommen.‘",
            21.6f
        ),
        new Line ( //19: human purity scene success - human art
            "‘Exactly,’ he laughs. ‘You can feel intent in it. The soul. Because that’s what real art is: A reflection of what it means to be human. You're definitely a great fit for our cause!’",
            "‚Genau‘, lacht er. ‚Man spürt die Absicht darin. Die Seele. Weil genau das echte Kunst ist: Eine Reflexion dessen, was es bedeutet, Mensch zu sein. Du passt super zu uns und unserer Bewegung!‘",
            21.6f
        ),
        new Line ( //20: AI purity scene failure - AI art
            "Their smile tightens. ‘That’s fear talking,’ one says softly. The little scoff that follows gives them away. ‘If you need strict purity to feel safe, you’ll fit better with them. Over there.’",
            "Ihr Lächeln verkrampft. ‚Das ist Angst, die da spricht‘, sagt einer leise. Dann kommt ein kleines, höhnisches Schnauben hinterher. ‚Wenn du strikte Reinheit brauchst, um dich sicher zu fühlen, passt du besser zu denen. Dort drüben.‘",
            20f
        ),
        new Line ( //21: human purity scene failure - AI art
            "‘Seriously?’ he snaps visibly offended. ‘If you can’t see what’s wrong with this, then you’re already part of the problem. Go over there. You don’t belong here.’",
            "‚Im Ernst?‘ Sagt er schnippisch und sichtlich beleidigt. ‚Wenn du nicht siehst, was daran falsch ist, dann bist du schon Teil des Problems. Geh rüber. Du gehörst nicht zu uns.‘",
            21.6f
        ),
        new Line ( //22: AI purity scene failure - human art 1
	        "‘Understood,’ one says tonelessly, the smile fading. ‘So if it’s stamped “human,” it gets a free pass. That’s your instinct—humanity first.’",
            "‚Verstanden‘, sagt einer tonlos, das Lächeln verschwindet. ‚Wenn “menschlich” draufsteht, ist es für dich automatisch okay. Das ist dein Instinkt — der Mensch zuerst.‘",
            21.6f
            ),
        new Line ( //23: AI purity scene failure - human art 2
            "They pause. ‘You prioritize comfort over progress. They’ll tolerate that. Over there.’",
            "Sie halten inne. ‚Du priorisierst Komfort über Fortschritt. Das werden die tolerieren. Dort drüben.'",
            21.6f
        ),
        new Line ( //24: human purity scene failure - human art
            "He stares at you. ‘So you don't value what is clearly human-made...’ His voice goes cold. ‘Then we have no place for you here. Go.’",
            "Er starrt dich an. ‚Also hast du keine Wertschätzung für ganz klar menschengemachte Dinge...‘ Seine Stimme wird kalt. ‚Dann haben wir hier für dich keinen Platz. Geh.‘",
            21.6f
        ),
        new Line ( //25: joining the AI crowd
            "Someone from the other side pulls you in with a bright smile. ‘Don’t worry. They do that to everyone.’",
            "Jemand von der anderen Seite zieht dich mit einem breiten Lächeln zu sich. ‚Keine Sorge. Das machen die bei allen.‘",
            21.6f
        ),
        new Line( //26: joining the human crowd
            "Someone grips your shoulder. ‘Knew it. You’re one of us. You just had to hear their nonsense yourself.’ He chuckles.",
            "Jemand fasst dir an die Schulter. ‚Wusste ich’s doch. Du bist einer von uns. Du musstest dir nur einmal selbst ihren Unsinn anhören.‘ Er lacht kurz.",
            21.6f
        ),
        new Line ( //27: followup at AI crowd 1
            "Someone leans in, almost amused. ‘That question was just a speed check,’ they murmur. ‘Pick a side fast enough, and you’re useful.’",
            "Jemand lehnt sich zu dir rüber, fast amüsiert. ‚Diese Frage war nur ein Geschwindigkeitstest‘, murmelt er. ‚Wenn du schnell genug ein Lager wählst, bist du nützlich.‘",
            21.6f
        ),
        new Line( //28: followup at human crowd 1
            "Someone speaks up quietly. ‘We needed to know if you'd hesitate.’ Your stomach twists. It hits you how little your reasons mattered... Only the reflex did.",
            "Jemand sagt leise: ‚Wir mussten wissen, ob du zögerst.‘ Dir zieht sich der Magen zusammen. Dir wird klar, wie egal deine Gründe waren... Es zählte nur dein Reflex.",
            21.6f
        ),
        new Line ( //29: followup at AI crowd 2
            " A few people nod like it’s routine. Your stomach twists. You feel filed away as “safe” or “unsafe,” more like data than a person.",
            "Ein paar nicken, als wäre das ganz normal. Dir zieht sich der Magen zusammen. Du fühlst dich einsortiert — „sicher“ oder „unsicher“ — eher wie Daten als wie ein Mensch.",
            21.6f
        ),
        new Line( //30: pondering scene 1
            "Before anyone can say more, you step back from the chaos. The chanting swallows the moment. You slip away into a quieter side alleyway besides the hospital, just to breathe.",
            "Bevor noch jemand etwas sagen kann, trittst du von dem ganzen Chaos zurück. Die Sprechchöre verschlucken den Moment. Du rutschst in einen ruhigeren Seitenbereich neben dem Krankenhaus, nur um kurz Luft zu holen.",
            19f
        ),
        new Line ( //31: pondering scene 2
            "Only then you notice it: the chanting behind you isn’t just loud... It’s synchronized. Too synchronized. A few voices repeat the exact same sentence… word for word… like a copied script.",
            "Erst dann fällt es dir auf: Der Lärm hinter dir ist nicht nur laut... Er ist synchron. Zu synchron. Ein paar Stimmen wiederholen exakt denselben Satz… Wort für Wort… wie ein kopiertes Skript.",
            21.6f
        ),
        new Line ( //32: pondering scene 3
            "Your stomach drops. Maybe this was never “two sides of people.” Maybe some of the loudest pressure wasn’t human at all, just something wearing a human shape, pushing you to pick a team.",
            "Dir wird flau. Vielleicht waren das nie „zwei Seiten von Menschen“. Vielleicht war ein Teil von dem Druck gar nicht menschlich, nur etwas, das menschlich aussieht und dich in ein Lager drücken will.",
            21.6f
        ),
        new Line( //33: ending fadeout
            "White. Silence. You blink and you’re back at your computer. On the screen are the same slogans, the same urgency, the same hostility. A header reads: GAIA.",
            "Weiß. Stille. Du blinzelst und sitzt wieder vor deinem Rechner. Auf dem Bildschirm stehen dieselben Parolen, dieselbe Dringlichkeit, dieselbe Feindseligkeit. Oben steht: GAIA.",
            21.2f
        ),
        new Line( //34: tutorial 2
            "To preview your options, put one arm up like you're waving on the side you wish to preview.",
            "Um deine Auswahloptionen anzuschauen, hebe den Arm auf der Seite hoch auf der Seite, die du dir genauer anschauen möchtest, als würdest du ihr zuwinken.",
            21.6f
        ),
        new Line( //35: tutorial 3
            "Once you're ready, move to either side to start and walk back to the middle once the scene is changing!",
            "Sobald du bereit bist, gehe auf eine der beiden Seiten und gehe nachdem die Szene sich geändert hat zurück zur Mitte!",
            21.6f
        )
    };
}
