(defun "prop" ^("name" "value") ^() *(set this name value))

(depend "move_object")

(prop "short" "object")
(prop "nouns" ^("object"))
(prop "a" ^"a (this:short)")

(defun "add_object" ^("to" "list" "object") ^() 
	*(nop
		(prop_add to list object)
		(set object "location" (record ^("object" to) ^("list" list)))))