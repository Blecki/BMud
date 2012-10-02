(prop "@base" (load "room"))
(prop "short" "Order Creche")
(prop "long" 
	"This is a small alcove where three wide halls intersect. Various ancient benches clutter the center of the chamber. Each hall has a sign hanging above it, made of various parts worn steel and new bits of painted cardboard. To the north, 'Minin', the east, 'BuNKs', and south, 'UrFace'.")
	
(add-detail this "benches" "These hard metal benches are dented and worn.")
(open-link this "new-haven/hallway-1" ^("east" "e"))

(add-object this "contents" (make-object (nop
	(prop "short" "bench")
	(prop "nouns" ^("bench"))
	(prop "can-sit" true)


	)))