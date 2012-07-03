(discard_verb "get")

(verb "get" (object location_contents "object")
	(defun "" ^("match" "actor") ^()
		*(nop
			(move_object match.object actor)
			(echo actor "You take (coalesce match.object.a "a (match.object.short)")."))))
			