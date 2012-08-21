
(lfun "open" [actor object message-suffix]
	(if object.on-open
		(object.on-open actor)
		(if (object.can-open actor)
			(if object.locked
				(echo actor "^(object:the) is locked.\n")
				(nop
					(echo actor "You open (object:the)(message-suffix).\n")
					(set object "open" true)
				)
			)
			(echo actor "You can't open that.\n")
		)
	)
)

(lfun "close" [actor object message-suffix]
	(if object.on-close
		(object.on-close actor)
		(if (object.can-open actor)
			(nop
				(echo actor "You close (object:the)(message-suffix).\n")
				(set object "open" false)
			)
			(echo actor "You can't close that.\n")
		)
	)
)

(add-global-verb "open"
	(m-standard-object)
	(lambda "" [matches actor]
		(nop
			(if (greaterthan (length matches) 1) (echo actor "[Multiple possible matches. Accepting first match.]\n"))
			(if (notequal (first matches).object.location.object actor.location.object)
				(nop
					(echo actor "[Opening ((first matches).object:a) ((first matches).object.location.list) ((first matches).object.location.object:the).]\n")
					(open actor (first matches).object " ((first matches).object.location.list) ((first matches).object.location.object:the)")
				)
				(nop
					(echo actor "[Opening ((first matches).object:a).]\n")
					(open actor (first matches).object "")
				)
			)
		)					
	)
	"GET [ALL] X \(\(FROM [IN/ON/UNDER]\) | IN/ON/UNDER\) [MY] Y"
)

(add-global-verb "close"
	(m-standard-object)
	(lambda "" [matches actor]
		(nop
			(if (greaterthan (length matches) 1) *(echo actor "[Multiple possible matches. Accepting first match.]\n"))
			(if (notequal (first matches).object.location.object actor.location.object)
				(nop
					(echo actor "[Closing ((first matches).object:a) ((first matches).object.location.list) ((first matches).object.location.object:the).]\n")
					(close actor (first matches).object " ((first matches).object.location.list) ((first matches).object.location.object:the)")
				)
				(nop
					(echo actor "[Closing ((first matches).object:a).]\n")
					(close actor (first matches).object "")
				)
			)
		)					
	)
	"GET [ALL] X \(\(FROM [IN/ON/UNDER]\) | IN/ON/UNDER\) [MY] Y"
)
