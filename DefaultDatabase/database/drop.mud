(depend "move_object")
(discard_verb "drop")

(verb "drop" (object (contents_source "actor" "held") "object")
	(defun "" ^("match" "actor") ^()
		*(if (equal actor.location.object null)
			*(echo actor "You don't seem to be anywhere.")
			*(nop
				(move_object match.object actor.location.object "contents")
				(echo actor "You drop (match.object:a).")
				(echo actor.location.object.contents "(actor.short) drops (match.object:a).")
			)
		)
	)
)
			