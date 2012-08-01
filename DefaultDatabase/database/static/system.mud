(defun "depend" ^("on") ^() *(load on))
(reload "move-object")
(reload "lists")

(defun "add-verb" ^("to" "name" "matcher" "action" "help") ^()
	(prop-add to "verbs" (record ^("name" name) ^("matcher" matcher) ^("action" action) ^("help" help) ^("defined-on" to)))
)

(defun "add-global-verb" ^("name" "matcher" "action" "help") ^()
	(let ^([system (load "system")])
		(nop
			(set system "verbs" (where "verb" system.verbs (notequal name verb.name)))
			(add-verb system name matcher action help)
		)
	)
)

(defun "add-global-alias" ^("name" "verb") ^()
	(let ^(^("system" (load "system")))
		(nop
			(if (equal system.aliases null) *(set system "aliases" (record)))
			(set system.aliases name verb)
		)
	)
)

(prop "handle-client-command" (defun "" ^("client" "words") ^()
	*(if (equal (index words 0) "login")
		*(let ^(^("player-object" (load "players/(index words 1)")))
			*(if player-object
				*(if (greaterthan (count "player" players *(equal player player-object)) 0)
					*(echo client "You are already logged in.\n")
					*(if (equal (hash (index words 2) "change this.") player-object.password)
						*(nop
							(set client "player" player-object)
							(set client "logged_on" true)
							(move-object player-object (load "demo-area/start-room") "contents")
							(command player-object "look")
							(invoke 10 echo ^(player-object "Timer fired.\n"))
						)
						*(echo client "Wrong password.\n")
					)
				)
				*(echo client "I couldn't find that account.\n")
			)
		)
		*(if (equal (index words 0) "register")
			*(let ^(^("player-object" (create "players/(index words 1)")))
				*(if player-object
					*(nop
						(multi-set client ^(^("player" player-object) ^("logged_on" true)))
						(multi-set player-object ^(
							^("password" (hash (index words 2) "change this."))
							^("@base" (load "player"))
							^("channels" ^("chat")) /* Subscribe new players to 'chat' channel */
							^("short" (index words 1))
							^("nouns" ^((index words 1)))
						))
						(move-object player-object (load "demo-area/start-room") "contents")
						(command player-object "look")
					)
					*(echo client "Couldn't create that player.\n")
				)
			)
			*(echo client "I don't recognize that command.\n")
		)
	)
))

(defun "find-all-visible-verbs" [actor] []
	(let ^([system (load "system")])
		(cat
			actor.location.object.verbs
			$(map "object" (get-all-visible-objects actor) (object.verbs))
			system.verbs
		)
	)
)

(defun "find-verb-list" [actor verb] []
	(let ^([system (load "system")])
		(lastarg
			(if system.aliases.(verb) *(var "verb" system.aliases.(verb)))
			(cat
				(where "potential-match" actor.location.object.verbs (equal potential-match.name verb))
				$(map "object" (get-all-visible-objects actor) (where "potential-match" object.verbs (equal potential-match.name verb)))
				(where "potential-match" system.verbs (equal potential-match.name verb))
			)
		)
	)
)

(prop "allow-switch" (lambda "lallow-switch" [actor switch] [] (atleast actor.rank 500)))

(prop "handle-command" (lambda "lhandle_command" ^("actor" "verb" "command" "token" "display-matches") ^()
	(nop
		(let ^(^("verb-records" (find-verb-list actor verb)))
			(if (equal (length verb-records) 0)
				(echo actor "Huh?\n")
				(while (notequal (length verb-records) 0)
					(let [["matches" ((first verb-records).matcher 
							[(record ^("token" token) ^("actor" actor) ^("command" command) ^("verb" (first verb-records)))]
						)]]
						(if (notequal (length matches) 0)
							(nop
								(if display-matches
									(nop
										(echo actor "(length matches) successful matches.\n")
										(for "match" matches *(echo actor "(match)\n"))
									)
									(if (first matches).fail 
										(echo actor (first matches):fail)
										((first verb-records).action matches actor)
									)
								)
								(var "verb-records" null)
							)
							(nop
								(if (equal (length verb-records) 1) *(echo actor "Huh?\n"))
								(var "verb-records" (sub-list verb-records 1))
							)
						)
					)
				)
			)
		)
		(echo actor actor:prompt)
	)
))

(prop "handle-new-client" (lambda "" [client] []
	(echo client "Welcome to BMud.\n")
))

(prop "handle-lost-client" (defun "" ^("client") ^()
	(if (client.logged_on)
		(move-object client.player null null)
	)
))

(defun "contains" ^("list" "what") ^()
	*(atleast (count "item" list *(equal item what)) 1))

(defun "contents" ^("mudobject") ^() (coalesce mudobject.contents ^()))

(reload "matchers")
(reload "object-matcher")
(reload "look")
(reload "say")
(reload "get")
(reload "drop")
(reload "go")
(reload "chat")

(defun "m-rank" [rank] [] 
	(lambda "lm-rank" [matches] [rank]
		(where "match" matches (atleast match.actor.rank rank))
	)
)

(add-global-verb "functions" (m-rank 100)
	(defun "" ^("matches" "actor") ^()
		(for "function" functions
			(echo actor "(function.name) - (function.shortHelp)\n")
		)
	)
	"List all declared functions."
)
			
(add-global-verb "verbs" (m-rank 100)
	(lambda "" [matches actor] []
		*(for "verb" (find-all-visible-verbs actor)
			*(echo actor "(verb)\n")
		)
	)
	"List all defined verbs."
)
			
(add-global-verb "examine" (m-sequence ^((m-rank 100) (m-complete (m-any-visible-object "object"))))
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

(add-global-verb "enumerate" (m-rank 100)
	(lambda "lenumerate" ^("matches" "actor") ^()
		*(enumerate-imple actor actor.location.object ^() 0)
	)
	"Enumerate every object in your location."
)

(add-global-verb "teleport" (m-sequence ^((m-rank 100) (m-rest "text")))
	(defun "" ^("matches" "actor") ^()
		*(let ^(^("destination" (load (first matches).text)))
			*(if (notequal destination null)
				*(nop
					(move-object actor destination "contents")
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

(add-global-verb "prompt" (m-if-exclusive (m-rest "text") (m-nop) (m-fail "Set your prompt to what?"))
	(lambda "" [matches actor] []
		(nop
			(set actor "prompt" (first matches).text)
			(echo actor "Prompt set.\n")
		)
	)
	"Set your prompt."
)

(add-global-verb "save" (m-nothing) 
	(lambda "" [matches actor] []
		(nop
			(echo actor "Saving...")
			(save actor.@path)
			(echo actor "done.\n")
			(purposefully generate an error)
		)
	)
	"Save your character."
)

(add-global-verb "who" (m-nothing)
	(lambda "" [matches actor] []
		(nop
			(echo actor "These players are currently playing:\n")
			(for "player" players (echo actor "(player:short)\n"))
		)
	)
	"See who's connected."
)
