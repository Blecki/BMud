(depend "lists")

(defun "move_object" ^("what" "to" "to_list") ^() /*Move an object. Maintains location property of object.*/
	*(nop
		(if (notequal what.location null)
			*(prop_remove what.location.object what.location.list what)
		)
		(if (notequal to null)
			*(nop
				(prop_add to to_list what)
				(set what "location" (record ^("object" to) ^("list" to_list)))
			)
			*(nop
				(set what "location" null)
			)
		)
	)
)
		