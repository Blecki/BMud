(prop "base" (load "room"))
(prop "long" "Default start room.")

(add_object this "contents" (decor *(nop
	(prop "short" "small dolphin statue")
	(prop "nouns" ^("dolphin" "statue")))))
	
(add_object this "contents" (decor *(nop
	(prop "short" "table")
	(prop "nouns" ^("table"))
	
	(add_object this "on" (decor *(nop
		(prop "short" "candle")
		(prop "nouns" ^("candle"))
	)))
	
	(add_object this "in" (decor *(nop
		(prop "short" "pen")
		(prop "nouns" ^("pen"))
	)))
)))



/*(depend "go")
(open_link this "west" "room")*/