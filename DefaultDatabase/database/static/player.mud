(depend "grammar")
(depend "room")
(prop "long" "Set the 'long' property to change this.")
(prop "pronoun" "he")
(prop "on-get" (defun "" ^("actor") ^() *(echo actor "(this:short) wouldn't appreciate that.\n")))
(prop "prompt" ">")

(prop "description" *"(this:short)\n(this:long)\n(if
	(equal (length this.held) 0)
	*("")
	*("^(this:pronoun) is holding (actor.formatter.list-objects-preposition this.held null null this).\n")
	)"
)

(prop "formatter" (load "basic-formatter"))

(add-global-verb "format" (m-if-exclusive (m-rest "text") (m-nop) (m-fail "Which formatter? Your choices are basic and icon.\n"))
	(lambda "" [matches actor] []
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