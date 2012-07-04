(defun "depend" ^("on") ^() *(load on))
(depend "object")
(depend "move_object")


(prop "on_unknown_verb" (defun "" ^("command" "actor") ^() 
	*(echo actor "Huh?")))
(prop "on_player_joined" (defun "" ^("actor") ^()
	*(nop
		(move_object actor null "" (load "room") "contents")
		(command actor "look"))))

(defun "contains" ^("list" "what") ^()
	*(atleast (count "item" list *(equal item what)) 1))

(defun "short_list" ^("object_list") ^() 
	*(strcat $(map "object" object_list *("(object.short), "))))

(defun "contents" ^("mudobject") ^() *(coalesce mudobject.contents ^()))

(depend "matchers")
(depend "look")
(depend "say")
(depend "get")

(discard_verb "functions")
(verb "functions" (none)
	(defun "" ^("match" "actor") ^()
		*(for "function" functions
			*(echo actor "(function.name) - (function.shortHelp)\n"))))
			
(discard_verb "examine")
(verb "examine" (or (or (me "object") (here "object")) (object location_contents "object"))
	(defun "" ^("match" "actor") ^()
		*(echo actor
			(strcat
				$(map "prop_name" (members match.object)
					*("(prop_name): (match.object.(prop_name))\n"))))))

