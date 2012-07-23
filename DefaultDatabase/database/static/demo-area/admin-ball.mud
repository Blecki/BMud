(prop "@base" (load "object"))
(prop "short" "crystal ball")
(prop "nouns" ["ball"])
(prop "adjectives" ["crystal"])

(prop "description" *(if (is-held-by this actor)
	("The ball feels warm in your hands. Gazing deep into it's misty interior, you see a faint glow, and in that glow, writing.\n'This object demonstrates a few things. First, the description changes when you are holding it. Second, it grants it's holder the verb 'divine'. So long as you are holding this ball, you can use the verb 'divine something', where 'something' is the name of a database object. The name of this one happens to be demo-area/admin-ball, try that first.'")
	("You see nothing particurally interesting about the crystal ball.")
))
	
(add-verb this "divine" 
	(m-sequence [
		(m-if-exclusive (m-rest "text") 
			(m-nop)
			(m-fail "What will you look at?\n")
		)
		(m-definer-held-by "actor")
	])
	
	(lambda "ldivine" [matches actor] [] 
		(if (first matches).fail 
			(echo actor (first matches):fail)
			(echo actor "Diving the location of ((first matches).text)...\n(asstring (load (first matches).text).location 2)\n")
		)
	)
	
	"Divine the location of named database objects."
)
	