(defun "depend" ^("on") ^() *(load on))
(depend "object")
(depend "move_object")


(prop "on_unknown_verb" (defun "" ^("command" "actor") ^() 
	*(echo actor "Huh?")))
(prop "on_player_joined" (defun "" ^("actor") ^()
	*(nop
		(move_object actor (load "room") "contents")
		(command actor "look"))))

(defun "contains" ^("list" "what") ^()
	*(atleast (count "item" list *(equal item what)) 1))

(defun "short_list" ^("object_list") ^() 
	*(strcat $(map "object" object_list *("(object.short), "))))

(defun "contents" ^("mudobject") ^() *(coalesce mudobject.contents ^()))

(depend "matchers")
(depend "object_matcher")
(depend "look")
(depend "say")
(depend "get")
(depend "drop")
(depend "go")

(discard_verb "functions")
(verb "functions" (none)
	(defun "" ^("match" "actor") ^()
		*(for "function" functions
			*(echo actor "(function.name) - (function.shortHelp)\n"))))
			
(discard_verb "examine")
(verb "examine" (any_object "object")
	(defun "" ^("match" "actor") ^()
		*(echo actor
			(strcat
				$(map "prop_name" (members match.object)
					*("(prop_name): (asstring match.object.(prop_name) 2)\n"))))))

