(depend "grammar")
(depend "room")
(prop "long" "Set the 'long' property to change this.")
(prop "pronoun" "he")
(prop "on-get" (defun "" ^("actor") *(echo actor "^(this:short) wouldn't appreciate that.\n")))
(prop "prompt" ">")
(prop "a" *"(this:short)")
(prop "list-short" *"(if (equal this.location.action "sitting") "(this:short) [sitting on (this.location.action-object:a)]" "(this:short)")")

(prop "compare-heights" (lambda "compare-heights" [a b] 
	(let ^(^("c" (subtract ((load "stats").calculate-actual-height a) ((load "stats").calculate-actual-height b))))
		(index ^(
			"much taller than you" 
			"a little taller than you" 
			"about your height" 
			"a little shorter than you"
			"much shorter than you")
			(if (greaterthan c 12) 0
				(if (greaterthan c 6) 1
					(if (greaterthan c -6) 2
						(if (greaterthan c -12) 3
							4)
					)
				)
			)
		)
	)
))

(prop "other-description" *"(this:short)
(this:long)
^(this:pronoun) is (this.compare-heights this actor).
(if (equal (length this.held) 0)
	"^(this:pronoun) is empty-handed."
	"^(this:pronoun) is holding (actor.formatter.list-objects-preposition this.held null null this)."
)
(if (equal (length this.worn) 0)
	"^(this:pronoun) is naked."
	"^(this:pronoun) is wearing (actor.formatter.list-objects-preposition this.worn null null this)."
)"
)

(prop "self-description" *"(if (equal (length this.held) 0)
	"You are empty-handed."
	"You are holding (actor.formatter.list-objects-preposition this.held null null this)."
)
(if (equal (length this.worn) 0)
	"You are naked."
	"You are wearing (actor.formatter.list-objects-preposition this.worn null null this)."
)")

(prop "description" *"(if (equal this actor) this:self-description this:other-description)")

(prop "formatter" (load "basic-formatter"))

(add-global-verb "format" (m-if-exclusive (m-rest "text") (m-nop) (m-fail "Which formatter? Your choices are basic and icon.\n"))
	(lambda "" [matches actor]
		(let ^(^("formatter" (load "((first matches).text)-formatter")))
			(if (formatter.is-formatter)
				(nop
					(set actor "formatter" formatter)
					(echo actor "Formatter set.\n")
				)
				(echo actor "That is not a formatter.")
			)
		)
	)
	"Set your formatter."
)