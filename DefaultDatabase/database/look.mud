(discard_verb "look")

(verb "look" (complete (optional (keyword "here")))
	(defun "" ^("match" "actor") ^() *(echo actor actor.location.object:description)))

(verb "look" (complete (sequence ^((optional (keyword "at")) (any_object "object"))))
	(defun "" ^("match" "actor") ^()
		*(echo actor match.object:description)
	)
)

(verb "look" 
	(complete 
		(sequence ^(
			(anyof ^("in" "on" "under") "preposition")
			(object (location_source "actor" "contents") "object"))))
	(defun "" ^("match" "actor") ^()
		*(echo actor "(match.preposition) the table are: (short_list (coalesce match.object.(match.preposition) ^())).")
	)
)

(verb "look" (anything)
	(defun "" ^("match" "actor") ^()
		*(echo actor "I don't see that here.")))
