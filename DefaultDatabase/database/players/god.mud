(prop "password" "password")
(prop "short" "God")
(prop "nouns" ^("god"))
(prop "base" (load "player"))

(add_object this "held" (create *(nop
	(prop "short" "backpack")
	(prop "nouns" ^("backpack" "pack"))
	(prop "allow_in" true)
	
	(add_object this "in" (create *(nop
		(prop "short" "red apple")
		(prop "nouns" ^("apple"))
	)))
	
	
)))
