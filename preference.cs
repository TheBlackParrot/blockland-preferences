//Preference class file

function validateFunctionName(%name) {
  %valid = "abcdefghijklmnopqrstuvwxyz1234567890_:";
  for(%i = 0; %i < strlen(%name); %i++) {
    %c = getSubStr(%name, %i, 1);
    if(strpos(%valid, %c) < 0) {
      return false;
    }
  }

  return true;
}

// PreferenceGroup -> Preference

function Preference::onAdd(%this) {
  if(%this.initialized)
    return;

  %this.initialized = true;

  //validation

  if(%this.addon $= "") {
    error(%this.getName() @ " pref: No Add-On Specified!");
    return;
  } else {
    %path = "Add-Ons/" @ %this.addon @ "/";
    if(!isFile(%path @ "server.cs")) {
      error(%this.getName() @ " pref: Invalid Add-On Path! (No file " @ %path @  "server.cs)");
      return;
    }
  }

  if(%this.category $= "")
    %this.category = "General";

  if(%this.title $= "") {
    error(%this.getName() @ " pref: No title specified!");
    return;
  }

  //param - validation later
  //variable - ?
  //defaultValue - validation later

  if(!validateFunctionName(%this.updateCallback)) {
    error(%this.getName() @ " pref: Invalid update callback!");
    return;
  }

  if(!validateFunctionName(%this.loadCallback)) {
    error(%this.getName() @ " pref: Invalid load callback!");
    return;
  }

  if(!validateFunctionName(%this.defaultCallback)) {
    error(%this.getName() @ " pref: Invalid default callback!");
    return;
  }

  //implicitly set to a boolean
  %this.hostOnly       = (%this.hostOnly != false);
  %this.secret         = (%this.secret != false);
  %this.loadNow        = (%this.loadNow != false);
  %this.noSave         = (%this.noSave != false);
  %this.requireRestart = (%this.requireRestart != false);

  // shorthand types
  %type = %this.type;
	switch$(%type) {
		case "boolean" or "tf":
			%type = "bool";

		case "number" or "real" or "intiger" or "int":
			%type = "num";

		case "numplayers":
			%type = "playercount";

		case "str" or "mlstring":
			%type = "string";

		case "slide" or "range" or "float":
			%type = "slider";

		case "choice" or "choices" or "list":
			%type = "dropdown";

		case "delimited":
			%type = "wordlist";

		case "users" or "bl_idlist" or "adminlist":
			%type = "userlist";

		case "colour":
			%type = "color";

		case "data":
			%type = "datablock";

		case "datalist" or "delimiteddata":
			%type = "datablocklist";

		case "vec":
			%type = "vector";

		case "call" or "function" or "push" or "callbackButton":
			%type = "button";
	}

  %valid = ":playercount:wordlist:datablocklist:userlist:datablock:slider:num:bool:button:dropdown:string:rgb:colorset";

	if(stripos(%valid, ":" @ %type) == -1) {
    error(%this.getName() @ " pref: Invalid preference type \"" @ %type @ "\"!");
		return;
	}

  // group
  // assumings %this.addon is valid!

  %groupName = BLP_alNum(%this.addon) @ "Prefs";
	if(!isObject(%groupName)) {
		%group = new ScriptGroup(%groupName) {
			class = "PreferenceGroup";
			title = BLP_alNum(%this.addon);
			file = %this.addon;
			legacy = %this.legacy;
			icon = $Pref::BlPrefs::iconDefault;
		};
	} else {
		%group = (%groupName).getID();
	}

  %group.add(%this);
}

function Preference::onUpdate(%this, %val) {
  if(%this.updateCallback !$= "") {
    eval(%this.updateCallback @ "(%this, %val);");
  }
}

function Preference::onLoad(%this, %val) {
  if(%this.loadCallback !$= "") {
    eval(%this.loadCallback @ "(%this, %val);");
  }
}

function Preference::onDefault(%this, %val) {
  if(%this.defaultCallback !$= "") {
    eval(%this.defaultCallback @ "(%this, %val);");
  }
}
