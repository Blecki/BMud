
(prop "choose-best-short" (lambda "" [actor object] 
	(if (equal actor object) "yourself"
		(if (equal actor object.location.object) "your (object:short)"
			(if (notequal object.location.object actor.location.object) object:the
				object:a
			)
		)
	)
))


(add-global-verb "look"

	(m-filter-failures
		(m-sequence [
			(m-optional (m-keyword "at"))
			(m-switch ^(
				^((m-nothing) (m-set-object-here))
				^((m-complete (m-any-visible-object "object")) (m-nop))
				^((m-preposition) (m-if-exclusive (m-complete (m-any-visible-object "object"))
					(m-if-exclusive (m-allows-preposition "object")
						(m-set "look-preposition" true)
						(m-fail *"You can't look (this.preposition) that.\n")
					)
					(m-fail "I don't see that here.\n")
				))
				^((m-flipper
					(m-if-exclusive (m-flipper-complete (m-relative-object))
						(m-nop)
						(m-fail *"I can't find that (this.preposition) (this.supporter:the).\n")
					)			
					(m-from|preposition)
					(m-if-exclusive (m-nothing)
						(m-fail "Look at what?\n")
						(m-complete (m-supporter *"You can't look (this.preposition) that.\n"))
					)) 
					(m-nop)
				))
				(m-fail "I don't see that here.")
			)
		])
	)
	
	(lambda "" [matches actor]
		(if (notequal (first matches).fail null)
			(echo actor (first matches):fail)
			(nop
				(if (greaterthan (length matches) 1) *(echo actor "[Multiple possible matches. Accepting first match.]\n"))
				(let ^([match (first matches)])
					(if (match.look-preposition)
						(nop
							(echo actor "[Looking (match.preposition) ((load "look").choose-best-short actor match.object).]\n")
							(echo actor "^(match.preposition)  ((load "look").choose-best-short actor match.object) (actor.formatter.list-objects-preposition match.object.(match.preposition) true true match.object)\n")
						)
						(if (and (notequal match.object actor.location.object) (notequal match.object.location.object actor.location.object))
							(nop
								(if (equal match.object.location.object actor)
									(echo actor "[Looking at ((load "look").choose-best-short actor match.object).]\n")
									(echo actor "[Looking at (match.object:a) from (match.object.location.list) ((load "look").choose-best-short actor match.object).]\n")
								)
								(echo actor "(match.object:description)\n")
							)
							(nop
								(echo actor "[Looking at ((load "look").choose-best-short actor match.object).]\n")
								(echo actor "(match.object:description)\n")
							)
						)
					)
				)
			)
		)
	)
	
	"Look at things in your environment"
)

(add-global-alias "read" "look")