(prop "@base" (load "room"))
(prop "long" 
	"A small chamber lined with bookshelves. A narrow window crouches in one wall, above an ancient wooden desk. Ill-fitting stones jut from the walls above the bookshelves. Dark wooden beams criss cross over head.")
	
(add-detail this "window" "The window is tall and narrow with an arched top. It has small panes separated by thin leading. You could look through it.")
(add-keyword-detail this ^("out" "through") "window" "You can see some mountains in the distance, poking up through the fog, but not much of anything else except fog.")

(open-link this "demo-area/balcony" ^("west" "w"))

(defun "random-item" ^("list") ^()
	*(index list (random 0 (length list)))
)

(let ^(^("titles-a"	^(
		"Chronicles of"
		"A Compendium of"
		"Untranslated Analysis of"
		"Adventures in"
		"Nonsense, Nonesuch, and"
		"Impersonal Theories Regarding"
		"How to Cook"
		"A Brief History of"
		"The Complete History of"
	)) ^("titles-b" ^("History"
		"Forgotten Kings" 
		"Antquity" 
		"Bear Livers"
		"Half-Burnt Candles" 
		"High Places" 
		"Sir Richard the Sexy"
		"Nonsense Uttered by Slow Children"
	)) ^("volumes" ^("" " volume I" " volume II" " volume III" " volume IV" " volume LXXIX")
	)
	^("covers" ^(
		"plain"
		"leather"
		"ragged"
		"embossed"
		))
	)
	*(add-verb this "get" (m-keyword "book")
		(lambda "lget-book" ^("matches" "actor") ^("titles-a" "titles-b" "volumes" "covers")
			*(let ^(^("cover" (random-item covers)))
				*((load "get").take actor
					(record 
						^("short" "(cover) book")
						^("adjectives" ^(cover))
						^("title" "(random-item titles-a) (random-item titles-b)(random-item volumes)")
						^("nouns" ^("book"))
						^("@base" (load "object"))
						^("description" *"(this.title)")
					)
					" from the bookshelf"
				)
			)
		)
		"Get book random book generator"
	)
)

(add-object this "contents" (create *(nop
	(prop "short" "dusty rug")
	(prop "nouns" ^("rug"))
	(prop "adjectives" ^("dusty"))
	(prop "description" "It looks like an ordinary rug. Could use some cleaning.")
)))

(add-object this "contents" (create *(nop
	(prop "short" "desk")
	(prop "nouns" ^("desk"))
	(prop "adjectives" ^("ancient" "wooden" "wood"))
	(prop "description" *"This desk is made of rough wooden planks crudely joined.\n(on-list this)\n")
	(prop "on-get" (lambda "" ^("actor") ^() *(echo actor "You couldn't possible carry that.\n")))
	(prop "allow-on" true)
	
	(add-object this "on" (create *(nop
		(prop "short" "small inkwell")
		(prop "nouns" ^("inkwell"))
		(prop "adjectives" ^("small"))
	)))
	
	(add-object this "on" (create *(nop
		(prop "short" "quill")
		(prop "nouns" ^("quill"))
	)))
	
	(add-object this "on" (load "demo-area/admin-ball"))
)))
