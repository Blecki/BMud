(defun "prop" ^("name" "value") ^() *(set this name value))

(depend "move_object")

(prop "short" ^"(this.path)")
(prop "nouns" ^("object"))
(prop "a" ^"a (this:short)") /* 'an object' would be correct, but inheriting objects are more likely to need 'a'. */
(prop "the" ^"the (this:short)")
(prop "description" ^"(this:long)")
(prop "long" ^"(this:short)")

(prop "can_get" true)

(defun "add_object" ^("to" "list" "object") ^() 
	*(nop
		(prop_add to list object)
		(set object "location" (record ^("object" to) ^("list" list)))))