/* Object sources for object matcher */

(defun "contents_source" ^("relative" "list") ^() /*Search relative some object for the items*/
	*(defun "" ^("match") ^("relative" "list") 
		*(coalesce match.(relative).(list) ^())
	)
)

(defun "contents_source_rel" ^("relative" "list") ^()
	*(defun "" ^("match") ^("relative" "list")
		*(coalesce match.(relative).(match.(list)) ^())
	)
)

(defun "location_source" ^("relative" "list") ^() /*Search relative some object for the items*/
	*(defun "" ^("match") ^("relative" "list") 
		*(coalesce match.(relative).location.object.(list) ^())
	)
)

(defun "filter_source" ^("source" "filter") ^()
	*(defun "" ^("match") ^("source" "filter")
		*(where "object" (source match) *(filter object match))
	)
)

(defun "allow_preposition_filter" ^("object" "match") ^()
	*(object:("allow_(match.preposition)"))
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

					