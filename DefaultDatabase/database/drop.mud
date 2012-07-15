(depend "move_object")
(discard_verb "drop")

(verb "drop" (m-object (os-contents "actor" "held") "object")
	(defun "" ^("matches" "actor") ^()
		*(if (equal actor.location.object null)
			*(echo actor "You don't seem to be anywhere.")
			*(nop
				(if (greaterthan (length matches) 1) *(echo actor "[More than one possible match. Accepting first match.]\n"))
				(move_object (first matches).object actor.location.object "contents")
				(echo actor "You drop ((first matches).object:a).")
				(echo actor.location.object.contents "(actor.short) drops ((first matches).object:a).")
			)
		)
	)
)
			