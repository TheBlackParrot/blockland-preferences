// newSettingsGui
// --------------------------------------
exec("./resources/NewSettingsGui.gui");

if($Pref::Client::Prefs::NotifyUsers $= "") {
	$Pref::Client::Prefs::NotifyUsers = true;
}

function newSettingsGui::onWake(%this) {
	// setup
	NewSettingsGui_Categories.clear();
	
	%this.changeList = "";
	%this.catListPos = 1;
	%this.clearPrefs();
	
	NewSettingsGui_NotifyUsersBox.setValue($Pref::Client::Prefs::NotifyUsers);
	
	// lets request the category list and some items for the first option.
	commandToServer('requestPrefCategories');
	commandToServer('requestCategoryPrefs', 0);
}

function newSettingsGui::addCategory(%this, %id, %category, %icon) {
	NewSettingsGui_Categories.addRow(%id, "     " @ %category);
	
	%default = "wrench";
	
	if(%icon $= "") {
		%icon = %default; // default;
	}
	
	if(!isFile("add-ons/Support_Prefs_Test/client/prefmanager/resources/icons/" @ %icon @ ".png")) {
		%icon = %default; // use default if icon cannot be found;
	}
	
	// icon
	%c = new GuiBitmapCtrl() {
		position = 2 SPC %this.catListPos;
		extent = "16 16";
		color = "0 0 0 255";
		bitmap = "add-ons/Support_Prefs_Test/client/prefmanager/resources/icons/" @ %icon @ ".png";
	};
	
	NewSettingsGui_Categories.getGroup().add(%c);
	
	%this.catListPos += 16;
}

function newSettingsGui::addSetting(%this, %data) {
	%list = clientPrefsList();
	%list.add(%data);
	
	// did we edit this value?
	%changes = getFieldCount(%this.changeList);
	
	%val = %data.value;
	
	for(%i = 0; %i < %changes; %i++) {
		%field = getField(%this.changeList, %i);
		
		%wc = getWordCount(%field);
		
		%serverId = getWord(%field, 0);
		%newVal = getWords(%field, 1, %wc-1);
		
		if(%serverId == %data.serverId) {
			%val = %newVal;
		}
	}
	
	// create new gui stuff for this item
	%p = NewSettingsGui_Prefs;
	
	// category check
	if(%this.guiCat !$= %data.subcategory) {
		//echo("cat" SPC %data.subcategory);
		
		%this.guiCat = %data.subcategory;
		
		// category listing
		%ysize = 26;
		
		%c = new GuiSwatchCtrl() {
			position = 0 SPC %this.guiPosition;
			extent = getWord(%p.getExtent(), 0) SPC %ysize;
			color = "0 0 0 255";
			horizSizing = "relative";
		};
		
		%title = new GuiMLTextCtrl() {
			position = "4 4";
			extent = getWord(%c.getExtent(), 0) SPC %ysize;
			text = "<color:FFFFFF><font:arial:16>" @ %data.subcategory;
		};
		
		%c.add(%title);
		
		// update the main gui
		%this.guiPosition += %ysize;
		%this.catPosition = %this.guiPosition;
		
		%p.add(%c);
		%p.resize(0, 0, getWord(%p.getExtent(), 0), %this.guiPosition + 64);
	}
	
	//echo("added" SPC %data.name);

	%ysize = 22;

	// gui listing
	%c = new GuiSwatchCtrl() {
		position = 0 SPC %this.guiPosition;
		extent = (getWord(%p.getExtent(), 0)) SPC %ysize;
		color = "0 0 0" SPC (((%this.guiPosition - %this.catPosition) / %ysize) % 2) * 25; // (:
		horizSizing = "relative";
	};
	
	%title = new GuiMLTextCtrl() {
		position = "4 4";
		extent = (getWord(%c.getExtent(), 0) / 2) SPC %ysize;
		text = "<color:444444>" @ %data.name @ ":";
	};
	
	%inputName = "NewSettingsGui_Opt_" @ %data.serverId;
	%input = 0;
	
	%type = %data.type;
	
	// this code is just ripped from rtb lol
	if(firstWord(%type) $= "string")
	{
		%input = new GuiTextEditCtrl(%inputName) {
		   profile = "guiTextEditProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "174 18";
		   minExtent = "8 2";
		   visible = "1";
		   maxLength = getWord(%type,1);
		   historySize = "0";
		   password = "0";
		   tabComplete = "0";
		   sinkAllKeyEvents = "0";
		   command = "newSettingsGui.settingChanged(" @ %data.serverId @ ", " @ %inputName @ ".getValue());";
		};
		
		%input.setValue(%val);
	}
	else if(firstWord(%type) $= "wordlist")
	{
		%input = new GuiTextEditCtrl(%inputName) {
		   profile = "guiTextEditProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "120 18";
		   minExtent = "8 2";
		   visible = "1";
		   enabled = false;
		   maxLength = getWord(%type,1);
		   historySize = "0";
		   password = "0";
		   tabComplete = "0";
		   sinkAllKeyEvents = "0";
		};
		
		// replace _ with space in type
		%delimiter = getWord(%type, 1);
		
		if(%delimiter $= "_") {
			%delimiter = " ";
		}
		
		%input.setValue(strreplace(%val, %delimiter, ";"));
		
		// add change button
		%input2 = new GuiBitmapButtonCtrl() {
		   profile = "BlockButtonProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 70 SPC 2;
		   extent = "50 18";
		   minExtent = "4 4";
		   visible = "1";
		   text = "Edit...";
		   bitmap = "base/client/ui/button1";
		   command = "newSettingsGui.changeWordlist(" @ %data.serverId @ ");";
		};
		
		%c.add(%input2);
	}
	else if(firstWord(%type) $= "userlist")
	{
		%input = new GuiTextEditCtrl(%inputName) {
		   profile = "guiTextEditProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "120 18";
		   minExtent = "8 2";
		   visible = "1";
		   enabled = false;
		   maxLength = getWord(%type,1);
		   historySize = "0";
		   password = "0";
		   tabComplete = "0";
		   sinkAllKeyEvents = "0";
		};
		
		// replace _ with space in type
		%delimiter = getWord(%type, 1);
		
		if(%delimiter $= "_") {
			%delimiter = " ";
		}
		
		%input.setValue(strreplace(%val, %delimiter, ";"));
		
		// add change button
		%input2 = new GuiBitmapButtonCtrl() {
		   profile = "BlockButtonProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 70 SPC 2;
		   extent = "50 18";
		   minExtent = "4 4";
		   visible = "1";
		   text = "Edit...";
		   bitmap = "base/client/ui/button1";
		   command = "newSettingsGui.changeBL_IDlist(" @ %data.serverId @ ");";
		};
		
		%c.add(%input2);
	}
	else if(firstWord(%type) $= "datablocklist")
	{
		%input = new GuiTextEditCtrl(%inputName) {
		   profile = "guiTextEditProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "120 18";
		   minExtent = "8 2";
		   visible = "1";
		   enabled = false;
		   maxLength = getWord(%type,1);
		   historySize = "0";
		   password = "0";
		   tabComplete = "0";
		   sinkAllKeyEvents = "0";
		};
		
		// replace _ with space in type
		%delimiter = getWord(%type, 1);
		
		if(%delimiter $= "_") {
			%delimiter = " ";
		}
		
		%input.setValue(strreplace(%val, %delimiter, ";"));
		
		// add change button
		%input2 = new GuiBitmapButtonCtrl() {
		   profile = "BlockButtonProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 70 SPC 2;
		   extent = "50 18";
		   minExtent = "4 4";
		   visible = "1";
		   text = "Edit...";
		   bitmap = "base/client/ui/button1";
		   command = "newSettingsGui.changedatablocklist(" @ %data.serverId @ ");";
		};
		
		%c.add(%input2);
	}
	else if(firstWord(%type) $= "num")
	{
		%input = new GuiTextEditCtrl(%inputName) {
		   profile = "guiTextEditProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "40 18";
		   minExtent = "8 2";
		   visible = "1";
		   maxLength = "50";
		   historySize = "0";
		   password = "0";
		   tabComplete = "0";
		   sinkAllKeyEvents = "0";
		   command = %inputName @ ".setText(clampFloat(" @ %inputName @ ".getValue(), " @ getWord(%type,1) @ ", " @ getWord(%type,2) @ "));" SPC "newSettingsGui.settingChanged(" @ %data.serverId @ ", " @ %inputName @ ".getValue());";
		};
		
		%input.setValue(%val);
	}
	else if(firstWord(%type) $= "slider")
	{
		%input = new GuiSliderCtrl(%inputName) {
		   profile = "guiTextEditProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "174 18";
		   minExtent = "8 2";
		   visible = "1";
		   maxLength = "50";
		   historySize = "0";
		   password = "0";
		   tabComplete = "0";
		   sinkAllKeyEvents = "0";
		   range = getWord(%type,1) SPC getWord(%type,2);
		   command = "newSettingsGui.settingChanged(" @ %data.serverId @ ", " @ %inputName @ ".getValue());";
		};
		
		%input.setValue(%val);
	}
	else if(firstWord(%type) $= "bool")
	{
		%input = new GuiCheckboxCtrl(%inputName) {
		   profile = "guiCheckboxProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "174 18";
		   minExtent = "4 4";
		   visible = "1";
		   text = getWord(%type, 1);
		   command = "newSettingsGui.settingChanged(" @ %data.serverId @ ", " @ %inputName @ ".getValue());";
		};
		
		%input.setValue(%val);
	}
	else if(firstWord(%type) $= "button")
	{
		%input = new GuiBitmapButtonCtrl(%inputName) {
		   profile = "BlockButtonProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "74 18";
		   minExtent = "4 4";
		   visible = "1";
		   text = getWord(%type, 1);
		   bitmap = "base/client/ui/button1";
		   command = "newSettingsGui.buttonSettingHit(" @ %data.serverId @ ");";
		};
		
		%input.setValue(%val);
	}
	else if(firstWord(%type) $= "playercount")
	{
		%input = new GuiPopupMenuCtrl(%inputName) {
		   profile = "guiPopUpMenuProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "174 18";
		   minExtent = "8 2";
		   visible = "1";
		   text = " ";
		   command = "newSettingsGui.settingChanged(" @ %data.serverId @ ", " @ %inputName @ ".getSelected());";
		};
		
		for(%l=getWord(%type,1);%l<=getWord(%type,2);%l++)
		{
			%s = "s";
			if(%l $= 1)
			   %s = "";
			%input.add(%l @ " Player" @ %s, %l);
		}
		
		%input.setSelected(%val);
	}
	else if(firstWord(%type) $= "dropdown")
	{
		%input = new GuiPopupMenuCtrl(%inputName) {
		   profile = "guiPopUpMenuProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "174 18";
		   minExtent = "8 2";
		   visible = "1";
		   text = " ";
		   command = "newSettingsGui.settingChanged(" @ %data.serverId @ ", " @ %inputName @ ".getSelected());";
		};
		
		%optionCount = (getWordCount(%type) - 1) / 2;
		
		for(%i = 0; %i < %optionCount; %i++) {
			%first = (%i * 2) + 1;
			
			%name = getWord(%type, %first);
			%value = getWord(%type, %first+1);
			
			%name = strreplace(%name, "_", " ");  
			
			%input.add(%name, %value);
		}
		
		%input.setSelected(%val);
	}
	else if(firstWord(%type) $= "datablock")
	{
		%input = new GuiPopupMenuCtrl(%inputName) {
		   profile = "guiPopUpMenuProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "174 18";
		   minExtent = "8 2";
		   visible = "1";
		   text = " ";
		   command = "newSettingsGui.settingChanged(" @ %data.serverId @ ", " @ %inputName @ ".getSelected());";
		};
		
		if(getWord(%type, 2)) {
			// add "NONE"
			 %input.add("NONE", -1);
		}
		
		%dataGroup = ServerConnection;
		if(DataBlockGroup.getCount() > 0)
		   %dataGroup = DataBlockGroup;
		for(%l=0;%l<%dataGroup.getCount();%l++)
		{
		   %object = %dataGroup.getObject(%l);
		   if(%object.getClassName() $= getWord(%type, 1) && %object.uiName !$= "")
			  %input.add(%object.uiName, %object.getId());
		}
		
		%input.setSelected(%val);
	}
	else {
		%input = new GuiTextEditCtrl(%inputName) {
		   profile = "guiTextEditProfile";
		   horizSizing = "right";
		   vertSizing = "bottom";
		   position = getWord(%p.getExtent(), 0) - 194 SPC 2;
		   extent = "174 18";
		   minExtent = "8 2";
		   visible = "1";
		   historySize = "0";
		   password = "0";
		   tabComplete = "0";
		   sinkAllKeyEvents = "0";
		   command = "newSettingsGui.settingChanged(" @ %data.serverId @ ", " @ %inputName @ ".getValue());";
		};
		
		%input.setValue(%val);
	}
	
	setGlobalByName(%inputName, %input); // alternate method for access
	
	%c.add(%input);
	%c.add(%title);
	
	// update the main gui
	%this.guiPosition += %ysize;
	
	%p.resize(0, 0, getWord(%p.getExtent(), 0), %this.guiPosition);
	
	%p.add(%c);
}

function newSettingsGui::clickApply(%this) {
	//commandToServer('prefUpdates', $Pref::Client::Prefs::NotifyUsers);
	
	if(%this.changeList $= "")
		return;
	
	// send the new settings to the server.
	// i.e. everything in changeList
	%changes = getFieldCount(%this.changeList);
	
	for(%i = 0; %i < %changes; %i++) {
		%field = getField(%this.changeList, %i);
		
		%data = findPref(getWord(%field, 0));
		%nv = getWords(%field, 1);
		
		commandToServer('UpdatePref', %data.returnName, %nv, $Pref::Client::Prefs::NotifyUsers);
	}
	
	%this.changeList = "";
	
	// refresh
	%this.selectOption();
}

function newSettingsGui::clickDone(%this) {
	%this.clickApply();
	
	// close the gui
	Canvas.popDialog(NewSettingsGui);
}

function newSettingsGui::clickBack(%this) {
	// do nothing
	
	// close the gui
	Canvas.popDialog(NewSettingsGui);
}

function newSettingsGui::selectOption(%this) {
	%this.clearPrefs();
	commandToServer('requestCategoryPrefs', NewSettingsGui_Categories.getSelectedId());
}

function newSettingsGui::settingChanged(%this, %serverId, %newValue) {
	%changes = getFieldCount(%this.changeList);
	
	%pr = findPref(%serverId);
	
	if(getWord(%pr.type, 0) $= "dropdown" || getWord(%pr.type, 0) $= "datablock" || getWord(%pr.type, 0) $= "playercount") {
		// hotfix inbound
		if(%pr.value == %newValue)
			return;
	}
	
	for(%i = 0; %i < %changes; %i++) {
		// if the value is already in the change list, don't add a new one
		%id = getWord(getField(%this.changeList, %i), 0);
		
		if(%id == %serverId) {
			%this.changeList = setField(%this.changeList, %i, %serverId SPC %newValue);
			//echo(%this.changeList);
			return;
		}
	}
	
	%this.changeList = setField(%this.changeList, %changes, %serverId SPC %newValue);
	//echo(%this.changeList);
}

function newSettingsGui::buttonSettingHit(%this, %serverId) {
	commandToServer('doButtonPref', %serverId);
	%this.selectOption();
}

function newSettingsGui::clearPrefs(%this) {
	%list = clientPrefsList();
	%list.clear();
	
	// clear every pref displayed
	%p = NewSettingsGui_Prefs;
	
	%p.deleteAll();
	%p.resize(0, 0, getWord(%p.getExtent(), 0), 64);
	
	%this.guiPosition = 0;
	%this.catPosition = 0;
	%this.guiCat = "";
}

function newSettingsGui::changeWordList(%this, %serverId) {
	//echo(%serverId);
	%data = findPref(%serverId);
	
	//echo(%data);
	
	// set up the word list Gui
	%val = %data.value;
	
	%changes = getFieldCount(%this.changeList);
	
	for(%i = 0; %i < %changes; %i++) {
		%field = getField(%this.changeList, %i);
		
		%wc = getWordCount(%field);
		
		%serverId = getWord(%field, 0);
		%newVal = getWords(%field, 1, %wc-1);
		
		if(%serverId == %data.serverId) {
			%val = %newVal;
		}
	}
	
	wordListGui.prefData = %data;
	wordListGui.prefCurValue = %val;
	
	// load it
	Canvas.pushDialog(wordListGui);
}

function newSettingsGui::changeBL_IDList(%this, %serverId) {
	//echo(%serverId);
	%data = findPref(%serverId);
	
	//echo(%data);
	
	// set up the word list Gui
	%val = %data.value;
	
	%changes = getFieldCount(%this.changeList);
	
	for(%i = 0; %i < %changes; %i++) {
		%field = getField(%this.changeList, %i);
		
		%wc = getWordCount(%field);
		
		%serverId = getWord(%field, 0);
		%newVal = getWords(%field, 1, %wc-1);
		
		if(%serverId == %data.serverId) {
			%val = %newVal;
		}
	}
	
	bl_idListGui.prefData = %data;
	bl_idListGui.prefCurValue = %val;
	
	// load it
	Canvas.pushDialog(bl_idListGui);
}

function newSettingsGui::changeDatablockList(%this, %serverId) {
	//echo(%serverId);
	%data = findPref(%serverId);
	
	//echo(%data);
	
	// set up the word list Gui
	%val = %data.value;
	
	%changes = getFieldCount(%this.changeList);
	
	for(%i = 0; %i < %changes; %i++) {
		%field = getField(%this.changeList, %i);
		
		%wc = getWordCount(%field);
		
		%serverId = getWord(%field, 0);
		%newVal = getWords(%field, 1, %wc-1);
		
		if(%serverId == %data.serverId) {
			%val = %newVal;
		}
	}
	
	DatablockListGui.prefData = %data;
	DatablockListGui.prefCurValue = %val;
	
	// load it
	Canvas.pushDialog(DatablockListGui);
}

// wordListGui
// --------------------------------------
exec("./resources/wordListGui.gui");

function wordListGui::onWake(%this) {
	// clean up
	wordListGui_List.clear();
	wordListGui_entry.setValue("");
	%this.newNum = 0;
	
	// dissasemble var
	%delimiter = getWord(%this.prefData.type, 1);
	
	if(%delimiter $= "_") {
		%delimiter = " ";
	}
	
	// replace delimiters with tabs
	%prefCur = strreplace(wordListGui.prefCurValue, %delimiter, "" TAB ""); // ??
	
	// go through the var and populate the list
	%count = getFieldCount(%prefCur);
	
	for(%i = 0; %i < %count; %i++) {
		%val = getField(%prefCur, %i);
		
		if(%val !$= "")
			wordListGui_List.addRow(%this.newNum++, %val);
	}
	
	wordListGui_Window.setText("Text List - " @ %this.prefData.name);
	
	%this.rowLimit = getWord(%this.prefData.type, 2);
}

function wordListGui::replaceTextWithSelection(%this) {
	wordListGui_entry.setValue(wordListGui_List.getValue());
}

function wordListGui::addWord(%this) {
	if(wordListGui_entry.getValue() $= "")
		return;
	
	if(bl_idListGui_List.rowCount() >= %this.rowLimit && %this.rowLimit != -1) {
		MessageBoxOk("Damn.", "You've hit the row limit for this pref.");
		return;
	}
	
	wordListGui_List.addRow(%this.newNum++, wordListGui_entry.getValue());
	wordListGui_entry.setValue("");
}

function wordListGui::removeSelected(%this) {
	wordListGui_List.removeRowById(wordListGui_List.getSelectedId());
}

function wordListGui::doneEditing(%this) {
	Canvas.popDialog(wordListGui);
	
	// dissasemble var again
	%delimiter = getWord(%this.prefData.type, 1);
	
	if(%delimiter $= "_") {
		%delimiter = " ";
	}
	
	// build final value from list items
	%val = "";
	
	for(%i = 0; %i < wordListGui_List.rowCount(); %i++) {
		%text = wordListGui_List.getRowText(%i);
		%val = %val @ %text;
		
		if(%i < wordListGui_List.rowCount()-1) {
			%val = %val @ %delimiter;
		}
	}
	
	newSettingsGui.settingChanged(%this.prefData.serverId, %val);
	$NewSettingsGui_Opt_[%this.prefData.serverId].setValue(strreplace(%val, %delimiter, ";"));
}

// bl_idListGui
// --------------------------------------
exec("./resources/bl_idListGui.gui");

function bl_idListGui::onWake(%this) {
	// clean up
	bl_idListGui_List.clear();
	bl_idListGui_PlayerList.clear();
	bl_idListGui_entry.setValue("");
	%this.newNum = 0;
	
	// dissasemble var
	%delimiter = getWord(%this.prefData.type, 1);
	
	if(%delimiter $= "_") {
		%delimiter = " ";
	}
	
	// replace delimiters with tabs
	%prefCur = strreplace(bl_idListGui.prefCurValue, %delimiter, "" TAB ""); // ??
	
	// go through the var and populate the list
	%count = getFieldCount(%prefCur);
	
	for(%i = 0; %i < %count; %i++) {
		%val = getField(%prefCur, %i);
		
		if(%val !$= "")
			bl_idListGui_List.addRow(%this.newNum++, %val);
	}
	
	bl_idListGui_Window.setText("BL_ID List - " @ %this.prefData.name);
	
	%this.rowLimit = getWord(%this.prefData.type, 2);
	
	commandToServer('PopulateBL_IDListPlayers');
}

function bl_idListGui::replaceTextWithSelection(%this) {
	bl_idListGui_entry.setValue(bl_idListGui_List.getValue());
}

function bl_idListGui::addWord(%this) {
	if(bl_idListGui_entry.getValue() $= "")
		return;
	
	if(bl_idListGui_List.rowCount() >= %this.rowLimit && %this.rowLimit != -1) {
		MessageBoxOk("Damn.", "You've hit the row limit for this pref.");
		return;
	}
	
	bl_idListGui_List.addRow(%this.newNum++, bl_idListGui_entry.getValue());
	bl_idListGui_entry.setValue("");
}

function bl_idListGui::addFromRight(%this) {
	if(bl_idListGui_List.rowCount() >= %this.rowLimit && %this.rowLimit != -1) {
		MessageBoxOk("Damn.", "You've hit the row limit for this pref.");
		return;
	}
	
	bl_idListGui_List.addRow(%this.newNum++, bl_idListGui_PlayerList.getSelectedId());
}

function bl_idListGui::removeSelected(%this) {
	bl_idListGui_List.removeRowById(bl_idListGui_List.getSelectedId());
}

function bl_idListGui::doneEditing(%this) {
	Canvas.popDialog(bl_idListGui);
	
	// dissasemble var again
	%delimiter = getWord(%this.prefData.type, 1);
	
	if(%delimiter $= "_") {
		%delimiter = " ";
	}
	
	// build final value from list items
	%val = "";
	
	for(%i = 0; %i < bl_idListGui_List.rowCount(); %i++) {
		%text = bl_idListGui_List.getRowText(%i);
		%val = %val @ %text;
		
		if(%i < bl_idListGui_List.rowCount()-1) {
			%val = %val @ %delimiter;
		}
	}
	
	newSettingsGui.settingChanged(%this.prefData.serverId, %val);
	$NewSettingsGui_Opt_[%this.prefData.serverId].setValue(strreplace(%val, %delimiter, ";"));
}

// datablockListGui
// --------------------------------------
exec("./resources/datablockListGui.gui");

function datablockListGui::onWake(%this) {
	// clean up
	datablockListGui_List.clear();
	datablockListGui_DataList.clear();
	datablockListGui_entry.setValue("");
	%this.newNum = 0;
	
	// dissasemble var
	%delimiter = getWord(%this.prefData.type, 1);
	
	if(%delimiter $= "_") {
		%delimiter = " ";
	}
	
	// replace delimiters with tabs
	%prefCur = strreplace(datablockListGui.prefCurValue, %delimiter, "" TAB ""); // ??
	
	// go through the var and populate the list
	%count = getFieldCount(%prefCur);
	
	for(%i = 0; %i < %count; %i++) {
		%val = getField(%prefCur, %i);
		
		if(%val !$= "")
			datablockListGui_List.addRow(%this.newNum++, %val);
	}
	
	datablockListGui_Window.setText("Datablock List - " @ %this.prefData.name);
	
	%this.rowLimit = getWord(%this.prefData.type, 2);
	
	// populate datablock list
	%dataGroup = ServerConnection;
	if(DataBlockGroup.getCount() > 0)
	   %dataGroup = DataBlockGroup;
	for(%l=0;%l<%dataGroup.getCount();%l++)
	{
	   %object = %dataGroup.getObject(%l);
	   if(%object.getClassName() $= getWord(%this.prefData.type, 3) && %object.uiName !$= "")
		  datablockListGui_DataList.addRow(%object.getId(), %object.uiName);
	}
}

function datablockListGui::replaceTextWithSelection(%this) {
	datablockListGui_entry.setValue(datablockListGui_List.getValue());
}

function datablockListGui::addWord(%this) {
	if(datablockListGui_entry.getValue() $= "")
		return;
	
	if(datablockListGui_List.rowCount() >= %this.rowLimit && %this.rowLimit != -1) {
		MessageBoxOk("Damn.", "You've hit the row limit for this pref.");
		return;
	}
	
	datablockListGui_List.addRow(%this.newNum++, datablockListGui_entry.getValue());
	datablockListGui_entry.setValue("");
}

function datablockListGui::addFromRight(%this) {
	if(datablockListGui_List.rowCount() >= %this.rowLimit && %this.rowLimit != -1) {
		MessageBoxOk("Damn.", "You've hit the row limit for this pref.");
		return;
	}
	
	datablockListGui_List.addRow(%this.newNum++, datablockListGui_DataList.getSelectedId().getName());
}

function datablockListGui::removeSelected(%this) {
	datablockListGui_List.removeRowById(datablockListGui_List.getSelectedId());
}

function datablockListGui::doneEditing(%this) {
	Canvas.popDialog(datablockListGui);
	
	// dissasemble var again
	%delimiter = getWord(%this.prefData.type, 1);
	
	if(%delimiter $= "_") {
		%delimiter = " ";
	}
	
	// build final value from list items
	%val = "";
	
	for(%i = 0; %i < datablockListGui_List.rowCount(); %i++) {
		%text = datablockListGui_List.getRowText(%i);
		%val = %val @ %text;
		
		if(%i < datablockListGui_List.rowCount()-1) {
			%val = %val @ %delimiter;
		}
	}
	
	newSettingsGui.settingChanged(%this.prefData.serverId, %val);
	$NewSettingsGui_Opt_[%this.prefData.serverId].setValue(strreplace(%val, %delimiter, ";"));
}