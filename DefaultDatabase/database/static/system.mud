/* Turns a function into a member function - sets the 'this' value when the function is called. */
(defun "thunk" ^("object new-this" "function func")
	(lambda "" ^("+?arguments")
		(let ^(^("save-this" func.declarationScope.this))
			(nop
				(set func.declarationScope "this" new-this)
				(func $arguments)
				(set func.declarationScope "this" save-this)
			)
		)
	)
)


(defun "depend" ^("on") *(load on))
(reload "move-object")
(reload "lists")
(reload "account")
(reload "front-end")
(reload "verb")

(prop "handle-new-client" (lambda "" [client]
	(nop
		(echo client "Welcome to BMud.\nIf you already have an account, login with 'login name password'. If you do not, register with 'register name password'.")
		(set client "command-handler" handle-frontend-command)
	)
))

(prop "handle-client-command" (defun "" [client full-command token switch]
	(client.command-handler client full-command token switch)
))

(prop "allow-switch" (lambda "lallow-switch" [actor switch] (atleast actor.rank 500)))


(prop "handle-lost-client" (defun "" ^("client")
	(if (client.logged_on)
		(move-object client.player null null)
	)
))

(defun "contains" ^("list" "what")
	(atleast (count "item" list *(equal item what)) 1))

(defun "contents" ^("mudobject") (coalesce mudobject.contents ^()))

(reload "matchers")
(reload "object-matcher")
(reload "basic")
(reload "look")
(reload "say")
(reload "get")
(reload "drop")
(reload "go")
(reload "chat")
(reload "open-close")
(reload "wear-remove")
(reload "sit-stand")