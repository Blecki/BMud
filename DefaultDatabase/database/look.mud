(discard_verb "look")

(verb "look" (complete (optional (keyword "here")))
	(defun "" ^("matches" "actor") ^() *(echo actor actor.location.object:description)))

(verb "look" (complete (sequence ^((optional (keyword "at")) (any_object "object"))))
	(defun "" ^("matches" "actor") ^()
		*(echo actor (first matches).object:description)
	)
)

(verb "look" 
	(complete 
		(sequence ^(
			(anyof ^("in" "on" "under") "preposition")
			(object (location_source "actor" "contents") "object")
		))
	)
	(defun "" ^("matches" "actor") ^()
		*(let ^(^("match" (first matches)))
			*(if (match.object:"allow_(match.preposition)")
				*(let ^(^("list" (coalesce match.object.(match.preposition) ^())))
					*(if (equal (length list) 0)
						*(echo actor "There is nothing (match.preposition) the (match.object:short).")
						*(echo actor "(match.preposition) the (match.object:short) (short_list list).")
					)
				)
				*(echo actor "You can't look (match.preposition) that.")
			)
		)
	)
)



(verb "look" (anything)
	(defun "" ^("matches" "actor") ^()
		*(echo actor "I don't see that here.")))
