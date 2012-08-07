(defun "prop" ^("name" "value") (set this name value))

(prop "short" *"(this.@path)")
(prop "nouns" ^("object"))
(prop "a" *"a (this:short)") /* 'an object' would be correct, but inheriting objects are more likely to need 'a'. */
(prop "the" *"the (this:short)")
(prop "description" "You see nothing special.")

(prop "can-get" true)

(defun "add-object" ^("object to" "string list" "object object") 
	(nop
		(prop-add to list object)
		(set object "location" (record ^("object" to) ^("list" list)))
	)
)

(defun "make-object" ^("code code")
	(let ^(^("object" (record ^("@base" (load "object")))))
		(lastarg
			(eval object :code)
			object
		)
	)
)
