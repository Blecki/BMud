(prop "@base" (load "room"))
(prop "short" "")
(prop "long" 
	"")
	
(open-link this "demo-area/balcony" ^("west" "w") (lambda "" [actor] (echo actor "On-follow worked!\n")))
