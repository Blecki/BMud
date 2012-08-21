(depend "move-object")

(lfun "implement-door-link" [door to name actor "function ?on-follow"]
	(let ^(^("previous" actor.location.object))
		(if (not door.open) 
			(echo actor "^(door:the) is closed.\n")
			(nop
				(echo to.contents "^(actor:short) arrived.\n")
				(move-object actor door.to "contents")
				(echo previous.contents "^(actor:short) went (name). [Through (door:the)]\n")
				(echo actor "You went (name). [Through (door:the)]\n")
				(if on-follow (on-follow actor))
				(command actor "look")
			)
		)
	)
)

(lfun "make-door-link-lambda" ^("door" "name" "function ?on-follow")
	(lambda "lgo" ["matches" "actor"] (implement-door-link door (load door.to) name actor on-follow))
)

(lfun "make-door-record" [to] 
	(record
		^("short" "door")
		^("nouns" ^("door"))
		^("to" to)
		^("@base" (load "object"))
		^("can-open" true)
		^("open" null)
	)
)

(defun "create-door" ^("from" "to" "names" "function ?on-follow")
	(let ^(^("door" (make-door-record to)))
	(lastarg
		(prop-add from "links" (record ^("name" (first names)) ^("door" door)))
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
						(if door.open
							(echo actor "To the (first names) you see...\n\n((load to):description)")
							(echo actor "The door is closed.\n")
						)
					)
					"Look (name)."
				)
			)
		)
		(add-object from "contents" door)
		(door)
	))
)

(lfun "make-direct-door-link-lambda" ^("door" "name" "function ?on-follow")
	*(lambda "lgo" ^("matches" "actor") (implement-door-link door door.to name actor on-follow))
)

(defun "create-direct-door" [from to names "function ?on-follow"]
	(let ^(^("door" (make-door-record to)))
	(lastarg
		(prop-add from "links" (record ^("name" (first names)) ^("door" door)))
		(for "name" names
			*(nop
				(add-verb from name (m-nothing)
					(make-direct-door-link-lambda door (first names) on-follow)
					"Move (name)."
				)
				(add-verb from "go" (m-complete (m-keyword name))
					(make-direct-door-link-lambda door (first names) on-follow)
					"Move (name)."
				)
				(add-verb from "look" (m-complete (m-keyword name))
					(lambda "llook-direction" ^("matches" "actor")
						(if door.open
							(echo actor "To the (first names) you see...\n\n(to:description)")
							(echo actor "The door is closed.\n")
						)
					)
					"Look (name)."
				)
			)
		)
		(add-object from "contents" door)
		(door)
	))
)