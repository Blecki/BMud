(depend "move-object")

(prop "take" (defun "" ^("actor" "object" "message_suffix")
	*(if (equal object.on-get null) /* Invoke default get behavior. */
		*(if (object.can-get actor)
			*(nop
				(move-object object actor "held")
				(echo actor "You take (object:a)(message_suffix).\n")
				(echo 
					(where "player" actor.location.object.contents *(notequal player actor)) 
					"^(actor:short) takes (object:a)(message_suffix).\n"
				)	
			)
			*(echo actor "You can't get that.\n")
		)
		*(object.on-get actor) /* invoke custom get behavior */
	)
))

(add-global-verb "get"
	(m-filter-failures
		(m-if-exclusive (m-nothing) (m-fail "Get what?\n")
			(m-if-exclusive (m-sequence ^((m-keyword "all") (m-nothing))) 
				(m-sequence ^((m-expand-objects (os-location "actor")) (m-set "all" true)))
				(m-if-exclusive (m-complete (m-sequence ^((m-?-all) (m-object (os-location "actor") "object")))) 
					(m-nop)
					(m-if-exclusive
						(m-flipper
							(m-if-exclusive (m-keyword "all") 
								(m-sequence ^((m-set "all" true) 
									(m-if-exclusive (m-flipper-nothing)
										(m-expand-supported-objects)
										(m-if-exclusive (m-flipper-complete (m-relative-object))
											(m-nop)
											(m-sequence ^((m-flipper-rest) (m-fail *"I can't find that (this.preposition) (this.supporter:the).\n")))
										)
									)
								))
								(m-if-exclusive (m-flipper-nothing)
									(m-fail *"Get what from (this.preposition) (this.supporter:the)?\n")
									(m-if-exclusive (m-flipper-complete (m-relative-object))
										(m-nop)
										(m-sequence ^((m-flipper-rest) (m-fail *"I can't find that (this.preposition) (this.supporter:the).\n")))
									)
								)								
							)
							(m-from|preposition)
							(m-if-exclusive (m-nothing)
								(m-fail "Get from what?\n")
								(m-complete (m-supporter *"You can't get things from (this.preposition) that.\n"))
							)
						)
						(m-nop)
						(m-fail "I don't see that here.\n")
					)
				)
			)
		)
	)
	(defun "" ^("matches" "actor")
		(if (equal (first matches).all true)
			(for "match" matches
				(imple-get actor match)
			)
			(nop
				(if (greaterthan (length matches) 1) *(echo actor "[Multiple possible matches. Accepting first match.]\n"))
				(imple-get actor (first matches))
			)
		)						
	)
	"GET [ALL] X \(\(FROM [IN/ON/UNDER]\) | IN/ON/UNDER\) [MY] Y"
)

(defun "imple-get" ^("actor" "match")
	*(if (notequal match.object.location.object actor.location.object)
		*(nop
			(echo actor "[Taking (match.object:a) from (match.object.location.list) (match.object.location.object:the).]\n")
			((load "get").take actor match.object " from (match.object.location.list) (match.object.location.object:the)")
		)
		*(nop
			(echo actor "[Taking (match.object:a).]\n")
			((load "get").take actor match.object "")
		)
	)
)
