(defun "depend" ^("on") ^() *(load on))
(reload "move_object")
(reload "lists")

(defun "add-verb" ^("to" "name" "matcher" "action" "help") ^()
	*(prop_add to "verbs" (record ^("name" name) ^("matcher" matcher) ^("action" action) ^("help" help)))
)

(defun "add-global-verb" ^("name" "matcher" "action" "help") ^()
	*(let ^(^("system" (load "system")))
		*(nop
			(if (equal system.verbs null) *(set system "verbs" (record)))
			(set system.verbs name (record ^("matcher" matcher) ^("action" action) ^("help" help)))
		)
	)
)

(defun "verbs" ^() ^()
	*(let ^(^("system" (load "system")))
		(map "verb" (members system.verbs)
			*(system.verbs.(verb))
		)
	)
)

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
		*(if (equal (index words 0) "register")
			*(let ^(^("player_object" (create_named "players/(index words 1)")))
				*(if player_object
					*(nop
						(set client "player" player_object)
						(set client "logged_on" true)
						(set player_object "password" (index words 2))
						(move_object player_object (load "start_room") "contents")
						(command player_object "look")
					)
					*(echo client "Couldn't create that player.\n")
				)
			)
			*(echo client "I don't recognize that command.\n")
		)
	)
))


(defun "find-verb-list" ^("actor" "verb") ^()
	*(where "match" (cat 
			(where "potential-match" (coalesce actor.location.object.verbs ^()) *(equal potential-match.name verb))
			((load "system").verbs.(verb))
		)
		*(match)
	)
)

(prop "handle_command" (lambda "lhandle_command" ^("actor" "verb" "command" "token" "display-matches") ^()
	*(let ^(^("verb-records" (find-verb-list actor verb)))
		*(if (equal (length verb-records) 0)
			*(echo actor "Huh?\n")
			*(while *(notequal (length verb-records) 0)
				*(let ^(^("matches" ((first verb-records).matcher ^((record ^("token" token) ^("actor" actor) ^("command" command))))))
					*(if (notequal (length matches) 0)
						*(nop
							(if display-matches
								*(nop
									(echo actor "(length matches) successful matches.\n")
									(for "match" matches *(echo actor "(match)\n"))
								)
								*((first verb-records).action matches actor)
							)
							(var "verb-records" null)
						)
						*(nop
							(if (equal (length verb-records) 1) *(echo actor "Huh?\n"))
							(var "verb-records" (sub-list verb-records 1))
						)
					)
				)
			)
		)
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

(reload "matchers")
(reload "object_matcher")
(reload "look")
(reload "say")
(reload "get")
(reload "drop")
(reload "go")

(add-global-verb "functions" (m-nothing)
	(defun "" ^("matches" "actor") ^()
		*(for "function" functions
			*(echo actor "(function.name) - (function.shortHelp)\n")
		)
	)
	"List all declared functions."
)
			
(add-global-verb "verbs" (m-nothing)
	(defun "" ^("matches" "actor") ^()
		*(for "verb" verbs
			*(echo actor "(verb.name)\n")
		)
	)
	"List all defined verbs."
)
			
(add-global-verb "examine" (m-complete (m-any-visible-object "object"))
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
	"List an object's properties."
)

(defun "enumerate-imple-list" ^("actor" "object" "list" "objects-visited" "depth") ^()
	*(let ^(^("contents" (object.(list))))
		*(if (equal (length contents) 0)
			*(nop)
			*(nop
				(echo actor "(strrepeat depth ".")(list):\n")
				(for "contained" contents
					*(enumerate-imple actor contained objects-visited (add depth 1))
				)
			)
		)
	)
)

(defun "enumerate-imple" ^("actor" "object" "objects-visited" "depth") ^()
	*(if (contains objects-visited object) 
		*(echo actor "(strrepeat depth ".")(object:short) [recurse]\n")
		*(let ^(^("new-visited" (cat ^(object) objects-visited)))
			*(nop
				(echo actor "(strrepeat depth ".")(object:short)\n")
				(enumerate-imple-list actor object "contents" new-visited (add depth 1))
				(enumerate-imple-list actor object "on" new-visited (add depth 1))
				(enumerate-imple-list actor object "in" new-visited (add depth 1))
				(enumerate-imple-list actor object "under" new-visited (add depth 1))
				(enumerate-imple-list actor object "held" new-visited (add depth 1))
				(enumerate-imple-list actor object "worn" new-visited (add depth 1))
			)
		)
	)
)

(add-global-verb "enumerate" (m-nothing)
	(lambda "lenumerate" ^("matches" "actor") ^()
		*(enumerate-imple actor actor.location.object ^() 0)
	)
	"Enumerate every object in your location."
)

(add-global-verb "teleport" (m-rest "text") 
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
	"Move yourself"
)

(add-global-verb "help" (m-rest "text")
	(lambda "" ^("matches" "actor") ^()
		*(nop
			(echo actor #((first matches).text))
			(let ^(^("test" "(first matches).text)"))
				*(echo actor "(test)")
			)
		)
	)
	"View help on a topic"
)

