(depend "move-object")

(defun "make-door-link-lambda" ^("door" "name" "function ?on-follow")
	*(lambda "lgo" ^("matches" "actor")
		*(let ^(^("previous" actor.location.object))
			*(nop
				(echo (load door.to).contents "^(actor:short) arrived.\n")
				(move-object actor (load door.to) "contents")
				(echo previous.contents "^(actor:short) went (name). [Through (door:the)]\n")
				(echo actor "You went (name). [Through (door:the)]\n")
				(if on-follow (on-follow actor))
				(command actor "look")
			)
		)
	)
)

(defun "create-door" ^("from" "to" "names" "function ?on-follow")
	(let ^(^("door" (record
		^("short" "door")
		^("nouns" ^("door"))
		^("to" to)
		^("@base" (load "object"))
		)))
	(lastarg
		(prop-add from "links" (first names))
		(for "name" names
			*(nop
				(add-verb from name (m-nothing)
					(make-door-link-lambda door (first names) on-follow)
					"Move (name)."
				)
				(add-verb from "go" (m-complete (m-keyword name))
					(make-door-link-lambda door (first names) on-follow)
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
		(add-object from "contents" door)
		(door)
	))
)
