(defun "prop" ^("name" "value") (set @scope.@parent.this name value)) 
	/* It's closing over this - therefore the prop is always added to 'object' */
	/* Perhaps '@scope.@parent.this' ? */

(prop "short" *"(this.@path)")
(prop "nouns" ^("object"))
(prop "a" *"a (this:short)") /* 'an object' would be correct, but inheriting objects are more likely to need 'a'. */
(prop "the" *"the (this:short)")
(prop "description" "You see nothing special.")
(prop "list-short" *"(this:a)")

(prop "can-get" true)
(prop "can-open" false)
(prop "can-wear" false)
(prop "can-sit" false)

(defun "add-object" ^("object to" "string list" "object object") 
	(nop
		(prop-add to list object)
		(set object "location" (record ^("object" to) ^("list" list)))
	)
)

(defun "make-object" ^("code code")
	(let ^(^("this" (record ^("@base" (load "object")))))
		(lastarg
			(eval :code)
			this
		)
	)
)

(defun "make-instance" ^("object of") (record ^("@base" of)))
