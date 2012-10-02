(depend "move-object")

(lfun "wear" [actor object] 
	(if (equal object.on-wear null)
		(if object.can-wear 
			(nop
				(move-object object actor "worn")
				(echo actor "You wear (object:a).\n")
				(echo (where "player" actor.location.object.contents (notequal player actor))
					"^(actor:short) wears (object:a).\n")
			)
			(echo actor "You can't wear that.")
		)
		(object.on-wear actor)
	)
)
		
(add-global-verb "wear" 
	(m-filter-failures 
		(m-if-exclusive (m-object (os-contents "actor" "held") "object")
			(m-nop)
			(m-fail "You don't seem to be holding that.\n")
		)
	)
									
	(lambda "lwear" ^("matches" "actor")
		(nop
			(if (greaterthan (length matches) 1) *(echo actor "[Multiple possible matches. Accepting first match.]\n"))
			(echo actor "[Wearing ((first matches).object:a).]\n")
			(wear actor (first matches).object)
		)
	)
	"WEAR X"
)
		

(lfun "remove" [actor object] 
	(if (equal object.on-remove null)
		(nop
			(move-object object actor "held")
			(echo actor "You remove (object:a).\n")
			(echo (where "player" actor.location.object.contents (notequal player actor))
				"^(actor:short) removes (object:a).\n")
		)
		(object.on-remove actor)
	)
)
		
(add-global-verb "remove" 
	(m-filter-failures 
		(m-if-exclusive (m-object (os-contents "actor" "worn") "object")
			(m-nop)
			(m-fail "You don't seem to be wearing that.\n")
		)
	)
									
	(lambda "lremove" ^("matches" "actor")
		(nop
			(if (greaterthan (length matches) 1) *(echo actor "[Multiple possible matches. Accepting first match.]\n"))
			(echo actor "[Removing ((first matches).object:a).]\n")
			(remove actor (first matches).object)
		)
	)
	"REMOVE X"
)

	
