
(defun "load-account" [name] (load "players/(name)/account"))
(defun "load-player-character" [account-name character-name] (load "players/(account-name)/(character-name)"))


(defun "create-player-character" [account name]
	(let ^(^("result" (create "players/(account)/(name)")))
		(lastarg
			(multi-set result ^(
				^("@base" (load "player"))
				^("channels" ^("chat")) /* Subscribe new players to 'chat' channel */
				^("short" name)
				^("nouns" ^(name))
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