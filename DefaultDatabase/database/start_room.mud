(prop "base" (load "room"))
(prop "long" "Default start room.")

(add_object this "contents" (create *(nop
	(prop "short" "small dolphin statue")
	(prop "nouns" ^("dolphin" "statue")))))
	
(add_object this "contents" (create *(nop
	(prop "short" "table")
	(prop "nouns" ^("table"))
	(prop "allow_on" true)
	(prop "allow_under" true)
	
	(add_object this "on" (create *(nop
		(prop "short" "candle")
		(prop "nouns" ^("candle"))
	)))
	
	(add_object this "under" (create *(nop
		(prop "short" "pen")
		(prop "nouns" ^("pen"))
	)))
)))



/*(depend "go")
(open_link this "west" "room")*/