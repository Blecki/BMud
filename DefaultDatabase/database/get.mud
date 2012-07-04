(depend "move_object")
(discard_verb "get")

(verb "get" (object location_contents "object")
	(defun "" ^("match" "actor") ^()
		*(nop
			(move_object match.object actor.location "contents" actor "contents")
			(echo actor "You take (match.object:a)."))))
			