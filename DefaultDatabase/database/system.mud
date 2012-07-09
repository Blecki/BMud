(defun "depend" ^("on") ^() *(load on))
(depend "object")
(depend "move_object")
(depend "lists")


(prop "on_unknown_verb" (defun "" ^("command" "actor") ^() 
	*(echo actor "Huh?")))
			
(prop "handle_client_command" (defun "" ^("client" "words") ^()
	*(if (equal (index words 0) "login")
		*(let ^(^("player_object" (load "players/(index words 1)")))
			*(if player_object
				*(if (greaterthan (count "player" players *(equal player player_object)) 0)
					*(echo client "You are already logged in.\n")
					*(if (equal (index words 2) player_object.password)
						*(nop
							(set client "player" player_object)
							(set client "logged_on" true)
							(move_object player_object (load "start_room") "contents")
							(command player_object "look")
						)
						*(echo client "Wrong password.\n")
					)
				)
				*(echo client "I couldn't find that account.\n")
			)
		)
		*(echo client "I don't recognize that command.\n")
	)
))

(prop "handle_lost_client" (defun "" ^("client") ^()
	*(if (client.logged_on)
		*(move_object client.player null null)
	)
))

(defun "contains" ^("list" "what") ^()
	*(atleast (count "item" list *(equal item what)) 1))

(defun "contents" ^("mudobject") ^() *(coalesce mudobject.contents ^()))

(depend "matchers")
(depend "object_matcher")
(depend "look")
(depend "say")
(depend "get")
(depend "drop")
(depend "go")

(discard_verb "functions")
(verb "functions" (none)
	(defun "" ^("matches" "actor") ^()
		*(for "function" functions
			*(echo actor "(function.name) - (function.shortHelp)\n"))))
			
(discard_verb "verbs")
(verb "verbs" (none)
	(defun "" ^("matches" "actor") ^()
		*(for "verb" verbs
			*(echo actor "(verb.name)\n")
		)
	)
)
			
(discard_verb "examine")
(verb "examine" (any_object "object")
	(defun "" ^("matches" "actor") ^()
		*(nop
			(if (greaterthan (length matches) 1) *(echo actor "[More than one possible match. Accepting first.]\n"))
			(echo actor
				(strcat
					$(map "prop_name" (members (first matches).object)
						*("(prop_name): (asstring (first matches).object.(prop_name) 1)\n")
					)
				)
			)
		)
	)
)

(discard_verb "teleport")
(verb "teleport" (rest "text") 
	(defun "" ^("matches" "actor") ^()
		*(let ^(^("destination" (load (first matches).text)))
			*(if (notequal destination null)
				*(nop
					(move_object actor destination "contents")
					(@command actor "look")
				)
			)
		)
	)
)

