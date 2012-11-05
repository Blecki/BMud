
(defun "load-account" [name] (load "players/(name)/account"))
(defun "load-player-character" [account-name character-name] (load "players/(account-name)/(character-name)"))

(defun "create-account" [name password]
	(let (^("account-object" (create "players/(name)/account")))
		(lastarg
			(set account-object "password" (hash password name))
			account-object
		)
	)
)

(defun "create-player-character" [account template]
	(let ^(
		^("result" (create-uniquely-named "players/(account)")))
		(lastarg
			(multi-set result ^(
				^("@base" (load template))
				^("channels" ^("chat")) /* Subscribe new players to 'chat' channel */
				^("nouns" ^(account))
				^("account-name" account)
			))
			result
		)
	)
)

(defun "list-player-characters" [account]
	(map "entry" (where "entry" (enumerate-database "players/(account)") (notequal entry "players/(account)/account"))
		(path-leaf entry)
	)
)

(defun "destroy-player-character" [account name] (nop)) /* Place holder; Database object destruction not implemented. */