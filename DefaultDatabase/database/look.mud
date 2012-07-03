(discard_verb "look")

(verb "look" (complete (optional (keyword "here")))
	(defun "" ^("match" "actor") ^() 
		*(nop
			(echo actor 
"You are in (actor.location.path).
(actor.location.long)
(if (equal (length (contents actor.location)) 0) 
	*("There doesn't appear to be anything here.")
	*("Some important objects: (short_list (contents actor.location))"))"))))


(verb "look" (complete (sequence ^((optional (keyword "at")) (any_object "object"))))
	(defun "" ^("match" "actor") ^()
		*(echo actor (coalesce match.object.short "You see nothing special."))))

(verb "look" (anything)
	(defun "" ^("match" "actor") ^()
		*(echo actor "I don't see that here.")))
