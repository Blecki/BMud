(defun "remove" ^("what" "list") ^() *(where "item" (coalesce list ^()) *(not (equal item what))))
(defun "add" ^("what" "list") ^() *(cat (coalesce list ^()) ^(what)))
(defun "prop_remove" ^("object" "property" "item") ^() *(set object property (remove item (object.(property)))))
(defun "prop_add" ^("object" "property" "item") ^() *(set object property (add item (coalesce (object.(property)) ^()))))

(defun "move_object" ^("what" "from" "from_list" "to" "to_list") ^() 
	*(nop
		(if (not (equal from null))
			*(prop_remove from from_list what))
		(prop_add to to_list what)))
		