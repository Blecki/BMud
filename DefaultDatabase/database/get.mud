(depend "move_object")
(discard_verb "get")

(verb "get" (object (location_source "actor" "contents") "object")
	(defun "" ^("match" "actor") ^()
		*(nop
			(move_object match.object actor "held")
			(echo actor "You take (match.object:a).")
			(echo actor.location.object.contents "(actor.short) takes (match.object:a).")
		)
	)
)
			