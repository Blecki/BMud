(prop "base" (load "room"))
(prop "long" "Default start room.")

(add_object this "contents" (create *(nop
	(prop "short" "small dolphin statue")
	(prop "nouns" ^("dolphin" "statue" "test"))
	(prop "on_get" (defun "" ^("actor") ^()
		*(echo actor "The small statue scurries just out of reach.\n")
	))
)))

(add_object this "contents" (create *(nop
	(prop "short" "table")
	(prop "nouns" ^("table" "test"))
	(prop "allow_on" true)
	(prop "allow_under" true)
	(prop "on_get" (defun "" ^("actor") ^() *(echo actor "You couldn't possibly carry that.\n")))
	
	(add_object this "on" (create *(nop
		(prop "short" "candle")
		(prop "nouns" ^("candle"))
	)))
	
		(add_object this "on" (create *(nop
		(prop "short" "orange")
		(prop "nouns" ^("orange"))
		(prop "a" ^"an (this:short)")
	)))
	
		(add_object this "on" (create *(nop
		(prop "short" "book")
		(prop "nouns" ^("book"))
	)))


	
	(add_object this "under" (create *(nop
		(prop "short" "pen")
		(prop "nouns" ^("pen"))
	)))
)))

(add_object this "contents" (create *(nop
	(prop "short" "dresser")
	(prop "nouns" ^("dresser"))
	(prop "allow_on" true)
	(prop "allow_in" true)
	
	(add_object this "on" (create *(nop
		(prop "short" "candle")
		(prop "nouns" ^("candle"))
	)))
	
	(add_object this "in" (create *(nop
		(prop "short" "book")
		(prop "nouns" ^("book"))
	)))
)))



/*(depend "go")
(open_link this "west" "room")*/