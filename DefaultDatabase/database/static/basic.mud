(defun "m-rank" [rank]
	(lambda "lm-rank" [matches]
		(where "match" matches (atleast match.actor.account.rank rank))
	)
)

(add-global-verb "functions" (m-rank 100)
	(defun "" ^("matches" "actor")
		(for "function" functions
			(echo actor "(function.name) - (function.shortHelp)\n")
		)
	)
	"List all declared functions."
)
			
(add-global-verb "verbs" (m-rank 100)
	(lambda "" [matches actor]
		*(for "verb" (find-all-visible-verbs actor)
			*(echo actor "(verb)\n")
		)
	)
	"List all defined verbs."
)
			
(add-global-verb "examine" (m-sequence ^((m-rank 100) (m-complete (m-any-visible-object "object"))))
	(defun "" ^("matches" "actor")
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

(defun "enumerate-imple-list" ^("actor" "object" "list" "objects-visited" "depth")
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

(defun "enumerate-imple" ^("actor" "object" "objects-visited" "depth")
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
	(lambda "lenumerate" ^("matches" "actor")
		*(enumerate-imple actor actor.location.object ^() 0)
	)
	"Enumerate every object in your location."
)

(add-global-verb "teleport" (m-sequence ^((m-rank 100) (m-rest "text")))
	(defun "" ^("matches" "actor")
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
	(lambda "" ^("matches" "actor")
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
	(lambda "" [matches actor]
		(nop
			(set actor "prompt" (first matches).text)
			(echo actor "Prompt set.\n")
		)
	)
	"Set your prompt."
)

(add-global-verb "save" (m-nothing) 
	(lambda "" [matches actor]
		(nop
			(echo actor "Saving...")
			(save actor.@path)
			(save actor.account.@path)
			(echo actor "done.\n")
		)
	)
	"Save your character."
)

(add-global-verb "who" (m-nothing)
	(lambda "" [matches actor]
		(nop
			(echo actor "These players are currently playing:\n")
			(for "player" players (echo actor "(player:short)\n"))
		)
	)
	"See who's connected."
)
