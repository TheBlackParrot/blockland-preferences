// prepend every function so we're not confusing anyone
// these aren't default functions

// i was considering just creating a dummy scriptobject to store these functions in so they appear on a dump of the object
// but you could just, y'know, look here

// Chris's todo: move my shit in here :c

function BLP_alNum(%str) {
	if(%str $= "") {
		return "";
	}

	// if there's a better way to strip a string of every non-alphanumeric character, do replace this
	%chars = "!@#$%^&*();'\"[]{},./<>?|\\-=_+` ";
	for(%i=0;%i<strLen(%chars);%i++) {
		%str = strReplace(%str, getSubStr(%chars, %i, 1), "");
	}

	return %str;
}

function GameConnection::BLP_isAllowedUse(%this) {
	if(%this.getBLID() == getNumKeyID()) {
		return true;
	}

	switch($Pref::BLPrefs::AllowedRank) {
		case 3:
			if(%this.getBLID() == 999999 || %this.getBLID() == getNumKeyID() || %this.isHost) {
				return true;
			}

		case 2:
			if(%this.isSuperAdmin) {
				return true;
			}

		case 1:
			if(%this.isAdmin) {
				return true;
			}

		default:
			warn("[Support_Preferences] Allowed rank not set, setting to Super Admin");
			$Pref::BLPrefs::AllowedRank = 2;
			return %this.BLP_isAllowedUse;
	}
	return false;
}

function getFirstWord(%str) {
	return getWord(%str, 0);
}
