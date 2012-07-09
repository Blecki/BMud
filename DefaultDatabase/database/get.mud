(depend "move_object")
(discard_verb "get")

(prop "take" (defun "" ^("actor" "object") ^()
	*(nop
		(move_object object actor "held")
		(echo actor "You take (object:a).")
		(echo (where "player" actor.location.object.contents *(notequal player actor)) "(actor:short) takes (object:a).")	
	)
))

(prop "take_from" (defun "" ^("actor" "object" "preposition" "from") ^()
	*(nop
		(move_object object actor "held")
		(echo actor "You take (object:a) from (preposition) (from:the).")
		(echo (where "player" actor.location.object.contents *(notequal player actor)) "(actor:short) takes (object:a) from (preposition) (from:the).")	
	)
))

(verb "get" (object (location_source "actor" "contents") "object")
	(defun "" ^("matches" "actor") ^()
		*(nop
			(if (greaterthan (length matches) 1) *(echo actor "[More than one possible match. Accepting first match.]\n"))
			((load "get").take actor (first matches).object)
		)
	)
)

(verb "get" 
	(flipper 
		(object (contents_source_rel "supporter" "preposition") "object")
		(sequence ^((optional (keyword "from")) (anyof ^("in" "on" "under") "preposition")))
		(object (filter_source (location_source "actor" "contents") allow_preposition_filter) "supporter")
	)
	(defun "" ^("matches" "actor") ^()
		*(nop
			(if (greaterthan (length matches) 1) *(echo actor "[More than one possible match. Accepting first match.]\n"))
			(let ^(^("match" (first matches))) 
				*((load "get").take_from actor match.object match.preposition match.supporter)
			)
		)
	)
)			