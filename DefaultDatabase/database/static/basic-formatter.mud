
(defun "basic-formatter-list-objects" ^("list list" "prepend-isare" "list-on") 
	(if (equal (length list) 1)
		(if (prepend-isare) 
			"is ((first list):a)(basic-formatter-list-objects-on (first list) list-on)"
			"((first list):a)(basic-formatter-list-objects-on (first list) list-on)"
		)
		(if (prepend-isare) "are (strcat 
			$(mapi "i" list
				(if (equal i (subtract (length list) 1)) 
					"and ((index list i):a)(basic-formatter-list-objects-on (index list i) list-on)" 
					"((index list i):a)(basic-formatter-list-objects-on (index list i) list-on), "
				)
			)
		)")
	)
)

(defun "basic-formatter-list-objects-on" ^("object of" "list-on")
	(if (and list-on (notequal (length of.on) 0))
		" [On which (basic-formatter-list-objects of.on true null)]"
		""
	)
))

(defun "basic-formatter-list-links" ^("list links")
	(strcat 
		$(map "link" links 
			"(link) "
		)
	)
)

(prop "is-formatter" true)
(prop "list-objects" basic-formatter-list-objects)
(prop "list-objects-preposition" 
	(lambda "" ^("list list" "prepend-isare" "list-on" "from") (basic-formatter-list-objects list prepend-isare list-on)))
(prop "list-links" basic-formatter-list-links)