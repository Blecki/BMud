
(defun "add-verb" ^("to" "name" "matcher" "action" "help")
	(prop-add to "verbs" (record ^("name" name) ^("matcher" matcher) ^("action" action) ^("help" help) ^("defined-on" to)))
)

(defun "add-global-verb" ^("name" "matcher" "action" "help")
	(let ^([system (load "system")])
		(nop
			(set system "verbs" (where "verb" system.verbs (notequal name verb.name)))
			(add-verb system name matcher action help)
		)
	)
)

(defun "add-global-alias" ^("name" "verb")
	(let ^(^("system" (load "system")))
		(nop
			(if (equal system.aliases null) *(set system "aliases" (record)))
			(set system.aliases name verb)
		)
	)
)

(defun "find-all-visible-verbs" [actor]
	(let ^([system (load "system")])
		(cat
			actor.location.object.verbs
			$(map "object" (get-all-visible-objects actor) (object.verbs))
			system.verbs
		)
	)
)

(defun "find-verb-list" [actor verb]
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

(defun "handle-verb-command" [client full-command token switch]
	(nop
		(let ^(^("verb-records" (find-verb-list client.player token.word)))
			(if (equal (length verb-records) 0)
				(echo client.player "Huh?\n")
				(while (notequal (length verb-records) 0)
					(let [["matches" ((first verb-records).matcher 
							[(record ^("token" token.next) ^("actor" client.player) ^("command" full-command) ^("verb" (first verb-records)))]
						)]]
						(if (notequal (length matches) 0)
							(nop
								(if (equal switch "display-matches")
									(nop
										(echo client.player "(length matches) successful matches.\n")
										(for "match" matches *(echo client.player "(match)\n"))
									)
									(if (first matches).fail 
										(echo client.player (first matches):fail)
										((first verb-records).action matches client.player)
									)
								)
								(var "verb-records" null)
							)
							(nop
								(if (equal (length verb-records) 1) *(echo client.player "Huh?\n"))
								(var "verb-records" (sub-list verb-records 1))
							)
						)
					)
				)
			)
		)
		(echo client.player client.player:prompt)
	)
)


