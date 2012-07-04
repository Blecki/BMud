(prop "long" "Changed Long description")
(add_object this "contents" (load "object"))
(add_object this "contents" (decor *(nop
	(prop "short" "small dolphin statue")
	(prop "nouns" ^("dolphin" "statue")))))

(depend "go")
(open_link this "west" "room")