(depend "character-generation")

(lfun "menu-choices" [client choices callback]
	(nop
		(for "item" choices (echo client "[(item.key)]: (item.name)\n"))
		(set client "command-handler"
			(lambda "handle-choice" [client full-command token switch]
				(callback client (first (where "item" choices (equal item.key token.word))))
			)
		)
	)
)

(lfun "menu-option" [key name value] (record ^("key" key) ^("name" name) ^("value" value)))

(lfun "build-menu" ^("function name" "list items")
	(mapi "i" items 
		(menu-option (itoa (add (atoi "A") i)) (name (index items i)) (index items i))
	)
)

(defun "handle-frontend-command" [client full-command token switch]
	(let (^("words" (mapex "token" token token.word token.next)))
		(if (equal (index words 0) "login")
			(handle-login-command client (index words 1) (index words 2))
			(if (equal (index words 0) "register")
				(handle-register-command client (index words 1) (index words 2))
				(echo client "I don't recognize that command. Valid commands are 'login name password' and 'register name password'.\n")
			)
		)
	)
)

(lfun "connect" [client player-object]
	(nop
		(set client "player" player-object)
		(set client "logged_on" true)
		(set player-object "account" client.account-object)
		(move-object player-object (load "new-haven/start-room") "contents")
		(set client "command-handler" handle-verb-command)
		(command player-object "look")
	)
)

(lfun "display-create-menu" [client]
	(menu-choices 
		client
		(build-menu (lambda "" [item] (path-leaf item)) (enumerate-database "templates"))
		(lambda "callback" [client choice]
			(if (not choice)
				(echo client "Huh?")
				(nop
					(let (^("player" (create-player-character client.account-name choice.value)))
						(nop
							(echo client choice)
							(set player "@base" (load "(choice.value)"))
							(connect client player)
						)
					)
				)
			)
		)
	)
)

(lfun "display-characters" [client]
	(menu-choices 
		client
		(cat 
			(build-menu (lambda "" [item] (path-leaf item)) (list-player-characters client.account-name)) 
			^((menu-option "0" "Create new character" null))
		)
		(lambda "callback" [client choice]
			(if (not choice)
				(echo client "Huh?")
				(if (choice.value)
					(connect client (load-player-character client.account-name choice.value))
					(display-create-menu client)
				)
			)
		)
	)
)




(defun "handle-login-command" [client account-name password]
	(let (^("account-object" (load-account account-name)))
		(if account-object
			(if (greaterthan (count "connected-client" clients (equal connected-client.account account-object)) 0)
				(echo client "You are already logged in.\n")
				(if (equal (hash password account-name) account-object.password)
					(nop
						(set client "account-name" account-name)
						(set client "account-object" account-object)
						(display-characters client)
					)
					(echo client "Wrong password.\n")
				)
			)
			(echo client "I couldn't find that account.\n")
		)
	)
)

(defun "handle-register-command" [client account-name password]
	(let (^("account-object" (load-account account-name)))
		(if account-object
			(echo client "That account already exists.\n")
			(let (^("account-object" (create-account account-name password)))
				(handle-login-command client account-name password)
			)
		)
	)
)


