
(let ^(
	^("display-characters" 
		(lambda "" [client characters]
			(mapi "i" characters (echo client "[(i)]: (index characters i) [type 'connect (i)' to choose this character.]\n"))
		)
	)
	)
(nop

(defun "handle-frontend-command" [client full-command token switch]
	(let ^(^("words" (mapex "token" token token.word token.next)))
		(if (equal (index words 0) "login")
			(handle-login-command client (index words 1) (index words 2))
			(if (equal (index words 0) "register")
				(handle-register-command client (index words 1) (index words 2))
				(echo client "I don't recognize that command. Valid commands are 'login name password' and 'register name password'.\n")
			)
		)
	)
)

(defun "handle-login-command" [client account-name password]
	(let ^(^("account-object" (load-account account-name)))
		(if account-object
			(if (greaterthan (count "connected-client" clients (equal connected-client.account account-object)) 0)
				(echo client "You are already logged in.\n")
				(if (equal (hash password account-name) account-object.password)
					(let ^(^("characters" (list-player-characters account-name)))
						(nop
							(if (equal (length characters) 0)
								(echo client "You have no characters.\n")
								(display-characters client characters)
							)
							(echo client "Valid commands are connect, create, and delete.\n")
							(set client "command-handler" handle-front-end-character-menu-command)
							(set client "account-name" account-name)
							(set client "account-object" account-object)
						)
					)
					

					
					(echo client "Wrong password.\n")
				)
			)
			(echo client "I couldn't find that account.\n")
		)
	)
)

(defun "handle-register-command" [client account-name password]
	(let ^(^("account-object" (load-account account-name)))
		(if account-object
			(echo client "That account already exists.\n")
			(nop
				(var "account-object" (create "players/(account-name)/account"))
				(if account-object
					(nop
						(multi-set account-object ^(^("player" "player") ^("password" (hash password account-name))))
						(handle-login-command client account-name password)
					)
				)
			)
		)
	)
)

(defun "handle-front-end-character-menu-command" [client full-command token switch]
	(let ^(^("words" (mapex "token" token token.word token.next)))
		(if (equal (index words 0) "connect")
			(let ^(^("choice" (index (list-player-characters client.account-name) (index words 1))))
				(if choice
					(let ^(^("player-object" (load-player-character client.account-name choice)))
						(nop
							(set client "player" player-object)
							(set client "logged_on" true)
							(set player-object "account" client.account-object)
							(move-object player-object (load "new-haven/start-room") "contents")
							(set client "command-handler" handle-verb-command)
							(command player-object "look")
						)
					)
				)
			)
			(if (equal (index words 0) "create")
				(let ^(^("name" (index words 1)))
					(let ^(^("player-object" (create-player-character client.account-name name)))
						(if player-object
							(nop
								(set client "player" player-object)
								(set client "logged_on" true)
								(set player-object "account" client.account-object)
								(move-object player-object (load "new-haven/start-room") "contents")
								(set client "command-handler" handle-verb-command)
								(command player-object "look")
							)
							(echo client "I couldn't create that character.\n")
						)
					)
				)
				(if (equal (index words 0) "delete")
					(echo client "Not implemented.\n")
					(let ^(^("characters" (list-player-characters client.account-name)))
						(nop
							(if (equal (length characters) 0)
								(echo client "You have no characters.\n")
								(display-characters client characters)
							)
							(echo client "Valid commands are connect, create, and delete.\n")
						)
					)
				)
			)
		)
	)
)

))