
(defun "first-noun" ["object object"] 
	("(if (notequal (length object.adjectives) 0) 
		(strcat $(map "adjective" object.adjectives "(adjective) ")) 
		""
	) (first object.nouns)")
)

(lfun "explicit-object-name" ["object object"] (first-noun object))

(defun "icon-formatter-list-objects" ^("list list" "prepend-isare" "list-on" "from")
	(if (equal (length list) 1)
		(if (prepend-isare) 
			"is <:(explicit-object-name (first list))>((first list):list-short)<:>{:look (first-noun (first list))(if from " from (first-noun from)" "")}(icon-formatter-list-objects-on (first list) list-on)"
			"<:(explicit-object-name (first list))>((first list):list-short)<:>{:look (first-noun (first list))(if from " from (first-noun from)" "")}(icon-formatter-list-objects-on (first list) list-on)"
		)
		"(if prepend-isare "are " "")(strcat 
			$(mapi "i" list
				(if (equal i (subtract (length list) 1)) 
					"and <:(explicit-object-name (index list i))>((index list i):list-short)<:>{:look (first-noun (index list i))(if from " from (first-noun from)" "")}(icon-formatter-list-objects-on (index list i) list-on)" 
					"<:(explicit-object-name (index list i))>((index list i):list-short)<:>{:look (first-noun (index list i))(if from " from (first-noun from)" "")}(icon-formatter-list-objects-on (index list i) list-on), "
				)
			)
		)"
	)
)

(defun "icon-formatter-list-objects-on" ^("object of" "list-on")
	(if (and list-on (notequal (length of.on) 0))
		" [On which (icon-formatter-list-objects of.on true null of)]"
		""
	)
))

(defun "icon-formatter-list-links" ^("list links")
	(strcat 
		$(map "link" links 
			"<:(link.name)>(link.name)<:>{:go (link.name)} (if link.door "[through <:(explicit-object-name link.door)>(link.door:list-short)<:>] " "")"
		)
	)
)

(prop "is-formatter" true)
(prop "@base" (load "basic-formatter"))
(prop "list-objects" 
	(lambda "" ^("list list" "prepend-isare" "list-on") (icon-formatter-list-objects list prepend-isare list-on null)))
(prop "list-objects-preposition" icon-formatter-list-objects)
(prop "list-links" icon-formatter-list-links)