﻿(depend "lists")

(defun "move-object" ^("what" "to" "to_list") /*Move an object. Maintains location property of object.*/
	*(nop
		(if (notequal what.location null)
			*(prop-remove what.location.object what.location.list what)
		)
		(if (notequal to null)
			*(nop
				(prop-add to to_list what)
				(set what "location" (record ^("object" to) ^("list" to_list)))
			)
			*(nop
				(set what "location" null)
			)
		)
	)
)

(defun "is-held-by" [object who] (and (equal object.location.object who) (equal object.location.list "held")) )

		