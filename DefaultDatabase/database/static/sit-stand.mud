(lfun "sit" [actor object] 
	(if (equal object.on-sit null)
		(if object.can-sit
			(nop
				(set actor.location "action" "sitting")
				(set actor.location "action-object" object)
				(echo actor "You sit on (object:a).\n")
				(echo (where "player" actor.location.object.contents (notequal player actor))
					"^(actor:short) sits on (object:a).\n")
			)
			(echo actor "You can't sit on that.")
		)
		(object.on-sit actor)
	)
)
		
(add-global-verb "sit" 
	(m-filter-failures 
		(m-if-exclusive (m-object (os-location "actor") "object")
			(m-nop)
			(m-fail "I don't see that here.\n")
		)
	)
									
	(lambda "lsit" ^("matches" "actor")
		(nop
			(if (greaterthan (length matches) 1) *(echo actor "[Multiple possible matches. Accepting first match.]\n"))
			(echo actor "[Sitting on ((first matches).object:a).]\n")
			(sit actor (first matches).object)
		)
	)
	"WEAR X"
)
		