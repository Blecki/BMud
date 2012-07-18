(prop "@base" (load "room"))
(prop "long" 
	"A small chamber lined with bookshelves. A narrow window crouches in one wall, above an ancient wooden desk. Ill-fitting stones jut from the walls above the bookshelves. Dark wooden beams criss cross over head.")
	
(add-detail this "window" "The window is tall and narrow with an arched top. It has small panes separated by thin leading. You could look through it.")
(add-keyword-detail this ^("out" "through") "window" "You can see some mountains in the distance, poking up through the fog, but not much of anything else except fog.")

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
	(prop "description" ^"This desk is made of rough wooden planks crudely joined.\n(on-list this)\n")
	(prop "on-get" (lambda "" ^("actor") ^() *(echo actor "You couldn't possible carry that.\n")))
	(prop "allow-on" true)
	
	(add-object this "on" (create *(nop
		(prop "short" "small inkwell")
		(prop "nouns" ^("inkwell"))
		(prop "adjectives" ^("small"))
	)))
)))
