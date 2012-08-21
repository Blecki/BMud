(depend "move-object")

/* Moves the actor through a link using the NAME of the destination room.
	This prevents the destination room from being loaded before the link is followed. */
(defun "make-link-lambda" ^("to" "name" "function ?on-follow")
	*(lambda "lgo" ^("matches" "actor")
		*(let ^(^("previous" actor.location.object))
			*(nop
				(echo (load to).contents "^(actor:short) arrived.\n")
				(move-object actor (load to) "contents")
				(echo previous.contents "^(actor:short) went (name).\n")
				(echo actor "You went (name).\n")
				(if on-follow (on-follow actor))
				(command actor "look")
			)
		)
	)
)

/* Opens an indirect link */
(defun "open-link" ^("from" "to" "names" "function ?on-follow")
	*(nop
		(prop-add from "links" (first names))
		(for "name" names
			*(nop
				(add-verb from name (m-nothing)
					(make-link-lambda to (first names) on-follow)
					"Move (name)."
				)
				(add-verb from "go" (m-complete (m-keyword name))
					(make-link-lambda to (first names) on-follow)
					"Move (name)."
				)
				(add-verb from "look" (m-complete (m-keyword name))
					(lambda "llook-direction" ^("matches" "actor")
						*(echo actor "To the (first names) you see...\n\n((load to):description)")
					)
					"Look (name)."
				)
			)
		)
	)
)

/* Moves the actor through a link. */
(defun "make-direct-link-lambda" ^("to" "name" "function ?on-follow")
	*(lambda "lgo" ^("matches" "actor")
		*(let ^(^("previous" actor.location.object))
			*(nop
				(echo to.contents "^(actor:short) arrived.\n")
				(move-object actor to "contents")
				(echo previous.contents "^(actor:short) went (name).\n")
				(echo actor "You went (name).\n")
				(if on-follow (on-follow actor))
				(command actor "look")
			)
		)
	)
)

/* Opens an direct link */
(defun "open-direct-link" [from "object to" names "function ?on-follow"]
	*(nop
		(prop-add from "links" (record ^("name" (first names))))
		(for "name" names
			*(nop
				(add-verb from name (m-nothing)
					(make-direct-link-lambda to (first names) on-follow)
					"Move (name)."
				)
				(add-verb from "go" (m-complete (m-keyword name))
					(make-direct-link-lambda to (first names) on-follow)
					"Move (name)."
				)
				(add-verb from "look" (m-complete (m-keyword name))
					(lambda "llook-direction" ^("matches" "actor")
						*(echo actor "To the (first names) you see...\n\n(to:description)")
					)
					"Look (name)."
				)
			)
		)
	)
)

(add-global-verb "go" (m-always-pass) (lambda "" ^("matches" "actor") *(echo actor "You can't go that way.\n")) "Move thyself.")