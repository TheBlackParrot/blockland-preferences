// prepend every function so we're not confusing anyone
// these aren't default functions

// i was considering just creating a dummy scriptobject to store these functions in so they appear on a dump of the object
// but you could just, y'know, look here

function BLP_alNum(%str) {
	if(%str $= "") {
		return;
	}

	// if there's a better way to strip a string of every non-alphanumeric character, do replace this
	%chars = "!@#$%^&*();'\"[]{},./<>?|\\-=_+` ";
	for(%i=0;%i<strLen(%chars);%i++) {
		%str = strReplace(%str, getSubStr(%chars, %i, 1), "");
	}

	return %str;
}