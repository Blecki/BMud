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

(verb "get" (flipper (object (contents_source "supporter" "on") "object") (keyword "from") (object (location_source "actor" "contents") "supporter"))
	(defun "" ^("match" "actor") ^()
		*(echo actor "Successfully matched (match.object:short).")
	)
)

(verb "get" (flipper 
	(object (contents_source_rel "supporter" "preposition") "object")
	(sequence ^((optional (keyword "from")) (anyof ^("in" "on" "under") "preposition")))
	(object (location_source "actor" "contents") "supporter"))
	(defun "" ^("match" "actor") ^()
		*(echo actor "Matched (match.object:short) (match.preposition) (match.supporter:short).")
	)
)
			