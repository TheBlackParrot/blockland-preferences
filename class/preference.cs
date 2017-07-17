//Preference class file

//================================================================
// Creation
//================================================================

function registerServerSetting(%cat, %title, %type, %variable, %addon, %default, %params, %callback, %legacy, %isSecret, %isHostOnly) {
  new ScriptObject(Preference) {
    className     = "ServerSettingPref";

    addon         = -1;
    serverSetting = true;
    category      = %cat;
    title         = %title;

    type          = %type;
    params        = %params;

    variable      = %variable;

    defaultValue  = %default;

    hostOnly      = %isHostOnly;
    secret        = %isSecret;

    noSave        = true;
  };
}

function Preference::onAdd(%this) {
  if(%this.initialized)
    return;

  %this.initialized = true;

  //validation

  %valid = %this._validateParameters();
  if(!%valid) {
    return;
  }

  %valid = %this._validateType();
  if(!%valid) {
    return;
  }

  %valid = %this._validateDefault();
  if(!%valid) {
    return;
  }

  if(!%this.serverSetting) {
    // group
    %groupName = BLP_alNum(%this.addon) @ "Prefs";
  	if(!isObject(%groupName)) {
  		%group = new ScriptGroup(PreferenceAddon) {
  			name = %groupName;
  			title = BLP_alNum(%this.addon);
  			file = %this.addon;
  			legacy = false;
  			icon = $Pref::BlPrefs::iconDefault;
  		};
  	} else {
  		%group = (%groupName).getID();
  	}

    if(%this.loadNow) {
      %this.forceLoad();
    }

    %variable = %this.variable;
    %newVariable = true;
  	for(%i = 0; %i < $BLPrefs::PrefCount + 1; %i++) {
  		if($BLPrefs::Pref[%i] $= %variable) {
  			%newVariable = false;
  		}
  	}

  	if(%newVariable) {
  		$BLPrefs::Pref[$BLPrefs::PrefCount++] = %variable;
  	}

    if(!%newVariable) {
      warn("Variable \"" @ %variable @  "\" is already associated with another preference!");
      %this.duplicate = true;
    }
  } else {
    //server setting
    %groupName = "ServerSettingPrefs";
  	if(!isObject(%groupName)) {
  		%group = new ScriptGroup(PreferenceAddon) {
  			name = %groupName;
  			title = "Server Settings";
  			file = -1;
  			legacy = false;
  			icon = "blLogo";
  		};
  	} else {
  		%group = (%groupName).getID();
  	}
  }


  %this.id = PreferenceGroup.getCount();
  %this.setName("");

  %group.add(%this);

  PreferenceGroup.add(%this);
}

function Preference::findByVariable(%varname) {
  if(strpos(%varname, "$") == 0)
    %varname = getSubStr(%varName, 1, strlen(%varname)-1);

  for(%i = 0; %i < PreferenceGroup.getCount(); %i++) {
    %pref = PreferenceGroup.getObject(%i);
    if(%pref.variable $= %varname) {
      return %pref;
    }
  }
}

//================================================================
// Individual loading
//================================================================

function Preference::forceLoad(%this) {
  BlocklandPrefs::checkMigrate(); //old save format

  %variable = %this.variable;
  %fo = new FileObject();
	%fo.openForRead($BLPrefs::File);
  %ct = 0;
	while(!%fo.isEOF()){
		%line = %fo.readLine();

    if(%ct == 0) {
      if(getWord(%line, 0) !$= "blprefs") {
        error("Invalid preference file at \"" @ $BLPrefs::File @ "\"");
        break;
      }
      continue;
    }

		if(getField(%line, 0) $= %variable) {
      %found = true;
			%val = collapseEscape(%line);
			break;
		}
	}
	%fo.close();
	%fo.delete();

	if(!%found) {
		//default
    echo("Loading default for " @ %this @ " (" @ %this.defaultValue @ ")");
    %this.value = %this.defaultValue;
		setGlobalByName(getWord(%variable, 0), %this.value);

    %this.onDefault(%this.value);
	} else {
    %this.value = %val;
		setGlobalByName(getWord(%variable, 0), %this.value);

    %this.onLoad(%val);
  }

  %this._loaded = true;
}

//================================================================
// Validation
//================================================================

function validateFunctionName(%name) {
  %valid = "abcdefghijklmnopqrstuvwxyz1234567890_:";
  for(%i = 0; %i < strlen(%name); %i++) {
    %c = strlwr(getSubStr(%name, %i, 1));
    if(strpos(%valid, %c) < 0) {
      return false;
    }
  }

  return true;
}

function Preference::_validateParameters(%this) {
  if(%this.addon $= "") {
    error(%this.getName() @ " pref: No Add-On Specified!");
    return false;
  }// else if(%this.addon !$= -1) {
  //  %path = "Add-Ons/" @ %this.addon @ "/";
  //  if(!isFile(%path @ "server.cs")) {
  //    error(%this.className @ " pref: Invalid Add-On Path! (No file " @ %path @  "server.cs)");
  //    return false;
  //  }
  //}

  if(trim(%this.category) $= "")
    %this.category = "General";

  if(%this.title $= "") {
    error(%this.getName() @ " pref: No title specified!");
    return false;
  }

  //param - validation later
  //variable - ?
  //defaultValue - validation later

  if(!validateFunctionName(%this.updateCallback)) {
    error(%this.getName() @ " pref: Invalid update callback!");
    return false;
  }

  if(!validateFunctionName(%this.loadCallback)) {
    error(%this.getName() @ " pref: Invalid load callback!");
    return false;
  }

  if(!validateFunctionName(%this.defaultCallback)) {
    error(%this.getName() @ " pref: Invalid default callback!");
    return false;
  }

  if(strpos(%this.variable, "$") == 0) {
    %this.variable = getSubStr(%this.variable, 1, strlen(%this.variable)-1);
  }

  //implicitly set to a boolean
  %this.hostOnly       = (%this.hostOnly != false);
  %this.secret         = (%this.secret != false);
  %this.loadNow        = (%this.loadNow != false);
  %this.noSave         = (%this.noSave != false);
  %this.requireRestart = (%this.requireRestart != false);
  return true;
}

function Preference::_validateType(%this) {
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
		return false;
	}
  %this.type = %type;
  return true;
}

function Preference::_validateDefault(%this) {
  %type = %this.type;
  %params = %this.params;
  switch$(%type) {
    case "num":
      %this.minValue = getWord(%params, 0);
      %this.maxValue = getWord(%params, 1);
      %this.decimalPoints = getWord(%params, 2);

      if(%this.defaultValue < %this.minValue) {
        %this.defaultValue = %this.minValue;
      } else if(%this.defaultValue > %this.maxValue) {
        %this.defaultValue = %this.maxValue;
      }

    case "playercount":
      %this.minValue = getWord(%params, 0);
      %this.maxValue = getWord(%params, 1);

      if(%this.defaultValue < %this.minValue) {
        %this.defaultValue = %this.minValue;
      } else if(%this.defaultValue > %this.maxValue) {
        %this.defaultValue = %this.maxValue;
      }

      %this.defaultValue = mFloor(%this.defaultValue);

    case "string":
      %this.maxLength = getWord(%params, 0);
      %this.stripML = getWord(%params, 1);

      if(strlen(%this.defaultValue) > %this.maxLength) {
        %this.defaultValue = getSubStr(%this.defaultValue, 0, %this.maxLength);
      }

    case "slider":
      %this.minValue = getWord(%params, 0);
      %this.maxValue = getWord(%params, 1);
      %this.snapTo = getWord(%params, 2);
      %this.stepValue = getWord(%params, 3);

      if(%this.defaultValue < %this.minValue) {
        %this.defaultValue = %this.minValue;
      } else if(%this.defaultValue > %this.maxValue) {
        %this.defaultValue = %this.maxValue;
      }

    case "bool":
      if(%this.defaultValue > 1)
        %this.defaultValue = 1;

      if(%this.defaultValue < 0)
        %this.defaultValue = 0;

    case "dropdown":
      // using the ol rtb list format
      %count = getWordCount(%params) / 2;

      for(%i = 0; %i < %count; %i++) {
        %first = (%i * 2);

        %this.rowName[%count] = strreplace(getWord(%params, %first), "_", " ");
        %this.rowValue[%count] = getWord(%params, %first+1);

        %this.valueName[%this.rowValue[%count]] = %this.rowName[%count];
      }

      %this.listCount = %count;

    case "datablock":
      %this.dataType = getWord(%params, 0);
      %this.canBeNone = getWord(%params, 1);

      if(!isObject(%this.defaultValue)) {
        %this.defaultValue = -1;
        %this.canBeNone = true;
      } else {
        if((%this.defaultValue).getClassName() != %this.dataType) {
          %this.defaultValue = -1;
          %this.canBeNone = true; // actually make it the first possible datablock in future rather than forcing "NONE" option and setting it
        }
      }

      // populate the pref object with all possible data values
      // IMPORTANT: when setting the actual global, MAKE SURE YOU USE THE DATABLOCK NAME. In all other situations, use its ID.
      %count = 0;

      if(%this.canBeNone) {
        // add "NONE" option
        %this.rowName[%count] = "NONE";
        %this.rowValue[%count] = -1;

        %this.valueName[%this.rowValue[%count]] = %this.rowName[%count];

        %count++;
      }

      for(%i = 0; %i < DataBlockGroup.getCount(); %i++) {
        %data = DataBlockGroup.getObject(%i);

        if(%data.getClassName() != %this.dataType)
        continue;

        %this.rowName[%count] = %data.uiName !$= "" ? %data.uiName : %data.getName();
        %this.rowValue[%count] = %data.getId();

        %this.valueName[%this.rowValue[%count]] = %this.rowName[%count];

        %count++;
      }

    case "wordlist":
      %this.delimiter = strreplace(getWord(%params, 0), "_", " ");
      %this.maxWords = getWord(%params, 1);

      %prefsAsFields = strreplace(%this.defaultValue, %this.delimiter, "" TAB ""); // hacky but it works

      // amend?
      if(getFieldCount(%prefsAsFields) > %this.maxWords && %this.maxWords != -1) {
        %prefsAsFields = getFields(%this.defaultValue, %this.maxWords);
      }

      %this.defaultValue = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);

    case "userlist":
      %this.delimiter = strreplace(getWord(%params, 0), "_", " ");
      %this.maxWords = getWord(%params, 1);

      %prefsAsFields = strreplace(%this.defaultValue, %this.delimiter, "" TAB ""); // hacky but it works

      // amend?
      if(getFieldCount(%prefsAsFields) > %this.maxWords && %this.maxWords != -1) {
        %prefsAsFields = getFields(%this.defaultValue, %this.maxWords);
      }

      // make sure EVERY field is a valid number.
      for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
        %prefsAsFields = setField(%prefsAsFields, %i, mFloor(getField(%prefsAsFields, %i)));
      }

      %this.defaultValue = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);

    case "datablocklist":
      %this.dataType = getWord(%params, 0);
      %this.delimiter = strreplace(getWord(%params, 1), "_", " ");
      %this.maxWords = getWord(%params, 2);

      %prefsAsFields = strreplace(%this.defaultValue, %this.delimiter, "" TAB ""); // hacky but it works

      // amend?
      if(getFieldCount(%prefsAsFields) > %this.maxWords && %this.maxWords != -1) {
        %prefsAsFields = getFields(%this.defaultValue, %this.maxWords);
      }

      // make sure EVERY field is a valid datablock.
      for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
        %data = getField(%prefsAsFields, %i);

        %validated = false;

        if(isObject(%data)) {
          if((%data).getClassName() == %this.dataType) {
            %validated = true;
          }
        }

        if(!%validated) {
          %prefsAsFields = setField(%prefsAsFields, %i, -1);
        }
      }

      %this.defaultValue = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);
  }
  return true;
}

//================================================================
// Value
//================================================================

function Preference::getValue(%this) {
	return getGlobalByName(%this.variable); //eval("return " @ %this.variable @ ";");
}

function Preference::updateValue(%this, %value, %updater) {
	// we need some way to validate the values on this end of things
	//%updater - client that updated value.

	if(isObject(%updater)) {
		%updaterClean = %updater.getId();
	} else {
		%updaterClean = 0;
	}

  %value = %this.validateValue(%value);

	// when storing datablocks, use their NAME.
	if(%this.type $= "datablock")
		setGlobalByName(%this.variable, (%value).getName());
	else {
		setGlobalByName(%this.variable, %value);
	}

  %this.value = %value;

	%this.onUpdate(%value, %updater);
  return true;
}

function Preference::validateValue(%this, %value) {
	//echo("validating" SPC %value SPC "(" @ %this @ ")");

	// this is where the SO's come in handy
	switch$(%this.type) {
		case "num":
			if(%this.decimalPoints !$= "") {
				%value = mFloatLength(%value, %this.decimalPoints);
			}

			if(%value < %this.minValue) {
				%value = %this.minValue;
			}
			else if(%value > %this.maxValue){
				%value = %this.maxValue;
			}

		case "string":
			if(strlen(%value) > %this.maxLength) {
				%value = getSubStr(%value, 0, %this.maxLength);
			}
			if(%this.stripML) {
				%value = stripMLControlChars(%value); // sure we couldn't have a MLString type? Yes.
			}

		case "slider":
			if(%value < %this.minValue) {
				%value = %this.minValue;
			}
			else if(%value > %this.maxValue){
				%value = %this.maxValue;
			}

			%value -= (%value % %this.snapTo);

		case "bool":
			if(%value > 1)
				%value = 1;

			if(%value < 0)
				%value = 0;

		case "dropdown":
			if(%this.valueName[%value] $= "") {
				%value = %this.rowValue[0]; // hacky, but it should work
			}

		case "datablock":
			if(%this.valueName[%value] $= "") {
				%value = %this.rowValue[0];
			}

		case "wordlist":
			%prefsAsFields = strreplace(%value, %this.delimiter, "" TAB ""); // hacky but it works

			// amend?
			if(getFieldCount(%prefsAsFields) > %this.maxWords && %this.maxWords != -1) {
				%prefsAsFields = getFields(%value, %this.maxWords);
			}

			%value = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);

		case "userlist":
			%prefsAsFields = strreplace(%value, %this.delimiter, "" TAB "");

			// amend?
			if(getFieldCount(%prefsAsFields) > %this.maxWords && %this.maxWords != -1) {
				%prefsAsFields = getFields(%value, %this.maxWords);
			}

			// make sure EVERY field is a valid number.
			for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
				%prefsAsFields = setField(%prefsAsFields, %i, mFloor(getField(%prefsAsFields, %i)));
			}

			%value = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);

		case "datablocklist":
			%prefsAsFields = strreplace(%value, %this.delimiter, "" TAB "" && %this.maxWords != -1);

			// amend?
			if(getFieldCount(%prefsAsFields) > %this.maxWords) {
				%prefsAsFields = getFields(%value, %this.maxWords);
			}

			// make sure EVERY field is a valid datablock.
			for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
				%data = getField(%prefsAsFields, %i);

				%validated = false;

				if(isObject(%data)) {
					if((%data).getClassName() == %this.dataType) {
						%validated = true;
					}
				}

				if(!%validated) {
					%prefsAsFields = setField(%prefsAsFields, %i, -1);
				}
			}

			%value = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);
	}
	return %value;
}

//================================================================
// Callbacks
//================================================================

function Preference::onUpdate(%this, %val, %client) {
  echo("Preference::onUpdate");
  if(%this.updateCallback !$= "") {
    eval(%this.updateCallback @ "(%this, %val, %client);");
  }

  if(%this.className !$= "" && isFunction(%this.className, "onUpdate"))
    eval(%this.className @ "::onUpdate(%this, %val, %client);");
}

function Preference::onLoad(%this, %val) {
  if(%this.loadCallback !$= "") {
    eval(%this.loadCallback @ "(%this, %val);");
  }

  if(%this.className !$= "" && isFunction(%this.className, "onLoad"))
    eval(%this.className @ "::onLoad(%this, %val);");
}

function Preference::onDefault(%this, %val) {
  if(%this.defaultCallback !$= "") {
    eval(%this.defaultCallback @ "(%this, %val);");
  }

  if(%this.className !$= "" && isFunction(%this.className, "onDefault"))
    eval(%this.className @ "::onDefault(%this, %val);");
}
