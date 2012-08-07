(depend "move-object")
(add-global-alias "put" "drop")

(prop "put" (lambda "lput" ^("actor" "object" "into" "list")
	*(if (equal object.on-put null)
		*(if into
			*(nop
				(move-object object into list)
				(echo actor "You put (object:a) (list) (into:the).\n")
				(echo (where "player" actor.location.object.contents *(notequal player actor))
					"^(actor:short) puts (object:a) (list) (into:the).\n"
				)
			)
			*(nop
				(move-object object actor.location.object "contents")
				(echo actor "You drop (object:a).\n")
				(echo (where "player" actor.location.object.contents *(notequal player actor))
					"^(actor:short) drops (object:a).\n"
				)
			)
		)
		*(object.on-put actor into list)
	)
))
		
(add-global-verb "drop" 
	(m-filter-failures
		(m-if-exclusive (m-sequence ^((m-keyword "all") (m-nothing)))
			(m-sequence ^((m-expand-held-objects) (m-set "all" true)))
			(m-sequence ^(
				(m-?-all)
				(m-if-exclusive (m-preposition)
					(m-if-exclusive (m-supporter *"You can't put things (this.preposition) that.\n")
						(m-expand-held-objects)
						(m-nop)
					)
					(m-if-exclusive (m-object (os-contents "actor" "held") "object")
						(m-if-exclusive (m-complete (m-nop))
							(m-nop)
							(m-if-exclusive (m-preposition)
								(m-supporter *"You can't put things (this.preposition) that.\n")
								(m-fail "You don't seem to be holding that.\n")
							)
						)
						(m-fail "You don't seem to be holding that.\n")
					)
				)
			))
		)
	)
	
								
	(lambda "ldrop" ^("matches" "actor")
		(if (equal (first matches).all true)
			(for "match" matches
				(imple-drop actor match)
			)
			(nop
				(if (greaterthan (length matches) 1) *(echo actor "[Multiple possible matches. Accepting first match.]\n"))
				(imple-drop actor (first matches))
			)
		)						
	)
	"DROP [ALL] X [\(IN/ON/UNDER\) [MY] Y]"
)
		
(defun "imple-drop" ^("actor" "match")
	*(nop
		(if match.supporter
			*(echo actor "[Putting (match.object:a) (match.preposition) (match.supporter:the).]\n")
			*(echo actor "[Dropping (match.object:a).]\n")
		)
		((load "drop").put actor match.object match.supporter match.preposition)
	)
)
	
