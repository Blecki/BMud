/* Object sources for object matcher */

(defun "contents_source" ^("relative" "list") ^() /*Search relative some object for the items*/
	*(defun "" ^("match") ^("relative" "list") 
		*(coalesce match.(relative).(list) ^())
	)
)

(defun "location_source" ^("relative" "list") ^() /*Search relative some object for the items*/
	*(defun "" ^("match") ^("relative" "list") 
		*(coalesce match.(relative).location.object.(list) ^())
	)
)

(defun "cat_source" ^("A" "B") ^() /* Cat two sources together*/
	*(defun "" ^("match") ^("A" "B") 
		*(cat (A match) (B match))
	)
)

(defun "object" ^("source" "into") ^() /* Match an object in the list returned by 'source' */
	*(defun "" ^("matches") ^("source" "into")
		*(cat 																						/* Combine list of lists into single list */
			$(map "match" matches																	/* Map existing matches to new matches */
				*(map "object" 																		/* Map objects to matches */
					(where "object" (source match) 													/* Result is a list of objects who'se noun property contains the next word in the command */
						*(contains (coalesce object.nouns ^()) match.token.word))
					*(clone match ^("token" match.token.next) ^(into object)))))))

					