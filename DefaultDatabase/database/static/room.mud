(prop "@base" (load "object"))
(prop "long" "Set the 'long' property to change this.")

(defun "actioned-short-list" ^("list") ^()
	(if (equal (length list) 1)
		("is ((index list 0):a){:look (first (index list 0).nouns)}.")
		("(isare list) (strcat
			$(mapi "i" list
				(if (equal i (subtract (length list) 1))
					("and ((index list i):a){:look (first (index list i).nouns)}.")
					("((index list i):a){:look (first (index list i).nouns)}, ")
				)
			)
		)")
	)
)

(defun "actioned-short-list-from-on" ^("list" "object") ^()
	(if (equal (length list) 1)
		("is ((index list 0):a){:look (first (index list 0).nouns) on (first object.nouns)}.")
		("(isare list) (strcat
			$(mapi "i" list
				(if (equal i (subtract (length list) 1))
					("and ((index list i):a){:look (first (index list i).nouns) on (first object.nouns)}.")
					("((index list i):a){:look (first (index list i).nouns) on (first object.nouns)}, ")
				)
			)
		)")
	)
)

(defun "actioned-short-list-with-on" ^("list") ^()
	(if (equal (length list) 1)
		("is ((first list):a){:look (first (first list).nouns)}(on-list (first list)).")
		("(isare list) (strcat
			$(mapi "i" list
				(if (equal i (subtract (length list) 1))
					("and ((index list i):a){:look (first (index list i).nouns)}(actioned-on-list (index list i)).")
					("((index list i):a){:look (first (index list i).nouns)}(actioned-on-list (index list i)), ")
				)
			)
		)")
	)
)

(defun "actioned-on-list" ^("object") ^()
	*(if (equal (length object.on) 0)
		*("")
		*(" [On which (actioned-short-list-from-on object.on object)]")
	)
)

(prop "description" 
*"(this:short)\n(this:long)\n(let ^(^("contents" (where "object" this.contents (notequal actor object))))
	(if	(equal (length contents) 0)
		("There doesn't appear to be anything here.")
		("Also here (actioned-short-list-with-on contents)")
	)
)\n(if (equal (length (coalesce this.links ^())) 0)
		*("There are no obvious exits.")
		*("Obvious exits: (strcat $(map "link" (this.links) ("(link){:go (link)} "))).")
)"
)

(defun "add-detail" ^("to" "name" "text") ^()
	*(add-verb to "look"
		(m-complete (m-sequence ^((m-optional (m-keyword "at")) (m-keyword name))))
		(lambda "ldetail" ^("matches" "actor") ^("text" "name")
			*(echo actor "[Looking at the (name)]\n(text)\n")
		)
		"Detail"
	)
)

(defun "add-adjective-detail" ^("to" "name" "adjectives" "text") ^()
	*(add-verb to "look"
		(m-complete (m-sequence ^((m-optional (m-keyword "at")) (m-?-adjectives adjectives) (m-keyword name))))
		(lambda "ldetail" ^("matches" "actor") ^("text" "name")
			*(echo actor "[Looking at the (name)]\n(text)\n")
		)
		"Detail"
	)
)

(defun "add-keyword-detail" ^("to" "keywords" "name" "text") ^()
	*(add-verb to "look"
		(m-complete (m-sequence ^((m-anyof keywords "keyword") (m-keyword name))))
		(lambda "ldetail" ^("matches" "actor") ^("text" "name")
			*(echo actor "[Looking ((first matches).keyword) the (name)]\n(text)\n")
		)
		"Keyword detail"
	)
)