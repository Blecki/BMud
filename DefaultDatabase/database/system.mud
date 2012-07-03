(defun "depend" ^("on") ^() *(load on))

(defun "prop" ^("name" "value") ^() *(set this name value))
(prop "on_unknown_verb" (defun "" ^("command" "actor") ^() 
	*(echo actor "Huh?")))

(defun "contains" ^("list" "what") ^()
	*(atleast (count "item" list *(equal item what)) 1))

(defun "short_list" ^("object_list") ^() 
	*(strcat $(map "object" object_list *("(object.short), "))))

(defun "contents" ^("mudobject") ^() *(coalesce mudobject.contents ^()))

(defun "remove" ^("what" "list") ^() *(where "item" (coalesce list ^()) *(not (equal item what))))
(defun "add" ^("what" "list") ^() *(cat (coalesce list ^()) ^(what)))
(defun "prop_remove" ^("object" "property" "item") ^() *(set object property (remove item (object.(property)))))
(defun "prop_add" ^("object" "property" "item") ^() *(set object property (add item (coalesce (object.(property)) ^()))))

(defun "move_object" ^("what" "to") ^() 
	*(nop
		(if (not (equal what.location null)) 
			*(prop_remove what.location "contents" what))
		(prop_add to "contents" what)))

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

