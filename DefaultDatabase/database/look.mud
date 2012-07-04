(discard_verb "look")

(verb "look" (complete (optional (keyword "here")))
	(defun "" ^("match" "actor") ^() 
		*(nop
			(echo actor 
"You are in (actor.location.object.path).
(actor.location.object.long)
(if (equal (length (contents actor.location.object)) 0) 
	*("There doesn't appear to be anything here.")
	*("Some important objects: (short_list (contents actor.location.object))"))"))))


(verb "look" (complete (sequence ^((optional (keyword "at")) (any_object "object"))))
	(defun "" ^("match" "actor") ^()
		*(echo actor (coalesce match.object:long "You see nothing special."))))

(verb "look" (anything)
	(defun "" ^("match" "actor") ^()
		*(echo actor "I don't see that here.")))
