﻿(depend "grammar")

(defun "list-remove" ^("what" "list") ^() *(where "item" (coalesce list ^()) *(notequal item what)))
(defun "list-add" ^("what" "list") ^() *(cat (coalesce list ^()) ^(what)))

(defun "prop_remove" ^("object" "property" "item") ^() 
	*(set object property 
		(list-remove item (object.(property)))
	)
)

(defun "prop_add" ^("object" "property" "item") ^() *(set object property (list-add item (coalesce (object.(property)) ^()))))


(defun "short_list" ^("list") ^()
	*(if 
		(equal (length list) 1)
		*("is ((index list 0):a).")
		*("(isare list) (strcat
			$(mapi "i" list
				*(if 
					(equal i (subtract (length list) 1))
					*("and ((index list i):a).")
					*("((index list i):a), ")
				)
			)
		)")
	)
)

(defun "short_list_with_on" ^("list") ^()
	*(if 
		(equal (length list) 1)
		*("is ((first list):a)(on_list (first list)).")
		*("(isare list) (strcat
			$(mapi "i" list
				*(if 
					(equal i (subtract (length list) 1))
					*("and ((index list i):a)(on-list (index list i)).")
					*("((index list i):a)(on-list (index list i)), ")
				)
			)
		)")
	)
)

(defun "on-list" ^("object") ^()
	*(if (equal (length object.on) 0)
		*("")
		*(" [On which (short_list object.on)]")
	)
)
