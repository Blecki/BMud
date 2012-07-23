(depend "move-object")

(defun "make-link-lambda" ^("to" "name") ^()
	*(lambda "lgo" ^("matches" "actor") ^("to" "name")
		*(let ^(^("previous" actor.location.object))
			*(nop
				(echo (load to).contents "^(actor:short) arrived.\n")
				(move-object actor (load to) "contents")
				(echo previous.contents "^(actor:short) went (name).\n")
				(echo actor "You went (name).\n")
				(command actor "look")
			)
		)
	)
)

(defun "open-link" ^("from" "to" "names") ^()
	*(nop
		(prop-add from "links" (first names))
		(for "name" names
			*(nop
				(add-verb from name (m-nothing)
					(make-link-lambda to (first names))
					"Move (name)."
				)
				(add-verb from "go" (m-complete (m-keyword name))
					(make-link-lambda to (first names))
					"Move (name)."
				)
				(add-verb from "look" (m-complete (m-keyword name))
					(lambda "llook-direction" ^("matches" "actor") ^("to" "names")
						*(echo actor "To the (first names) you see...\n\n((load to):description)")
					)
					"Look (name)."
				)
			)
		)
	)
)

(add-global-verb "go" (m-always-pass) (lambda "" ^("matches" "actor") ^() *(echo actor "You can't go that way.\n")) "Move thyself.")