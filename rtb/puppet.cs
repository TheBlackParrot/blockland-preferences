
//$RTB::MSSC::Pref[$RTB::MSSC::Prefs] = %name TAB %pref TAB %cat TAB %vartype TAB %mod TAB %requiresRestart TAB %hostOnly TAB %callback;

//================================================================
// Overwritten Methods
//================================================================

//- RTBSC_sendPrefList (Sends a pref list to a specific client)
function RTBSC_sendPrefList(%client) {
  if(!%client.isSuperAdmin || %client.hasPrefList)
     return;

  %client.hasPrefList = 1;

  for(%i = 0; %i < PreferenceGroup.getCount(); %i++) {
    %pref = PreferenceGroup.getObject(%i);

    %prefs = %prefs SPC %i;
    %tranId = %i%16;
    %data[%transId] = %pref.title TAB %pref.addon TAB %pref.type TAB %pref.requireRestart; //name TAB category TAB type TAB restart

    if(%index%16 $= 15 || %i $= PreferenceGroup.getCount()-1) { //batch ready or last
      if(%i $= $PreferenceGroup.getCount()-1) //last pref
         %prefs = %prefs SPC "D";

      %prefs = getSubStr(%prefs, 1, strlen(%prefs)-1);

      commandtoclient(%client,'RTB_addServerPrefs', %prefs, %data0, %data1, %data2, %data3, %data4, %data5, %data6, %data7, %data8, %data9, %data10, %data11, %data12, %data13, %data14, %data15);
      %prefs = "";
      for(%j = 0; %j < 16; %j++) {
         %data[%j] = "";
      }
    }
  }

  RTBSC_sendPrefValues(%client);
}

//- RTBSC_sendPrefValues (Sends pref values to a client)
function RTBSC_sendPrefValues(%client) {
  if(!%client.isSuperAdmin || %client.hasPrefList)
     return;

  %client.hasPrefList = 1;

  for(%i = 0; %i < PreferenceGroup.getCount(); %i++) {
    %pref = PreferenceGroup.getObject(%i);

    %prefs = %prefs SPC %i;
    %tranId = %i%16;
    %data[%transId] = %pref.getValue();

    if(%index%16 $= 15 || %i $= PreferenceGroup.getCount()-1) { //batch ready or last
      if(%i $= $PreferenceGroup.getCount()-1) //last pref
         %prefs = %prefs SPC "D";

      %prefs = getSubStr(%prefs, 1, strlen(%prefs)-1);

      commandtoclient(%client,'RTB_setServerPrefs', %prefs, %data0, %data1, %data2, %data3, %data4, %data5, %data6, %data7, %data8, %data9, %data10, %data11, %data12, %data13, %data14, %data15);
      %prefs = "";
      for(%j = 0; %j < 16; %j++) {
         %data[%j] = "";
      }
    }
  }
}

//================================================================
// Admin
//================================================================


//- serverCmdRTB_getAutoAdminList (Sends the auto admin list to the client)
function serverCmdRTB_getAutoAdminList(%client) {
  //direct copy

  if(%client.isSuperAdmin || !%client.hasRTB) {
    %adminList = $Pref::Server::AutoAdminList;
    %superAdminList = $Pref::Server::AutoSuperAdminList;
    commandtoclient(%client,'RTB_getAutoAdminList',%adminList,%superAdminList);
  }
}

//- serverCmdRTB_addAutoStatus (Allows a client to add a player to the auto list)
function serverCmdRTB_addAutoStatus(%client,%bl_id,%status) {
  //near direct copy

  if(%client.isSuperAdmin) {
    if(%bl_id $= "" || !isInt(%bl_id) || %bl_id < 0) {
      commandtoclient(%client,'MessageBoxOK',"Whoops","You have entered an invalid BL_ID.");
      return;
    }

    if(%status !$= "Admin" && %status !$= "Super Admin") {
      commandtoclient(%client,'MessageBoxOK',"Whoops","You have entered an invalid Status.");
      return;
    }

    $Pref::Server::AutoAdminList = removeItemFromList($Pref::Server::AutoAdminList,%bl_id);
    $Pref::Server::AutoSuperAdminList = removeItemFromList($Pref::Server::AutoSuperAdminList,%bl_id);
    if(%status $= "Admin") {
      $Pref::Server::AutoAdminList = addItemToList($Pref::Server::AutoAdminList,%bl_id);
    } else if(%status $= "Super Admin") {
      $Pref::Server::AutoSuperAdminList = addItemToList($Pref::Server::AutoSuperAdminList,%bl_id);
    }
    export("$Pref::Server::*","config/server/prefs.cs");

    serverCmdRTB_getAutoAdminList(%client);

    for(%i=0;%i<ClientGroup.getCount();%i++) {
      %cl = ClientGroup.getObject(%i);
      if(%cl.bl_id $= %bl_id) {
        if(%status $= "Super Admin") {
          if(%cl.isSuperAdmin)
             return;

          %cl.isAdmin = 1;
          %cl.isSuperAdmin = 1;
          %cl.sendPlayerListUpdate();
          commandtoclient(%cl,'setAdminLevel',2);
          messageAll('MsgAdminForce','\c2%1 has become Super Admin (Auto)',%cl.name);

          RTBSC_SendPrefList(%client);
        } else if(%status $= "Admin") {
          if(%cl.isAdmin)
             return;

          %cl.isAdmin = 1;
          %cl.isSuperAdmin = 0;
          %cl.sendPlayerListUpdate();
          commandtoclient(%cl,'setAdminLevel',1);
          messageAll('MsgAdminForce','\c2%1 has become Admin (Auto)',%cl.name);
        }
      }
    }
  }
}

//- serverCmdRTB_removeAutoStatus (Removes a player from the auto lists)
function serverCmdRTB_removeAutoStatus(%client,%bl_id) {
  //direct copy
	if(%client.isSuperAdmin) {
		$Pref::Server::AutoAdminList = removeItemFromList($Pref::Server::AutoAdminList,%bl_id);
		$Pref::Server::AutoSuperAdminList = removeItemFromList($Pref::Server::AutoSuperAdminList,%bl_id);

    export("$Pref::Server::*","config/server/prefs.cs");

		serverCmdRTB_getAutoAdminList(%client);
	}
}

//- serverCmdRTB_clearAutoAdminList (Empties the auto admin lists)
function serverCmdRTB_clearAutoAdminList(%client) {
  //direct copy
  if(%client.isSuperAdmin) {
    $Pref::Server::AutoAdminList = "";
    $Pref::Server::AutoSuperAdminList = "";
    export("$Pref::Server::*","config/server/prefs.cs");

    serverCmdRTB_getAutoAdminList(%client);
  }
}

//- serverCmdRTB_deAdminPlayer (De-admins a player)
function serverCmdRTB_deAdminPlayer(%client,%victim) {
  //direct copy
  if(!%client.isSuperAdmin)
    return;

  if(findLocalClient() $= %victim || %victim.bl_id $= getNumKeyID()) {
    messageClient(%client,'','\c2You cannot de-admin the host.');
    return;
  } else if(%victim.isSuperAdmin && %client.bl_id !$= getNumKeyID()) {
    messageClient(%client,'','\c2Only the host can de-admin a Super Admin.');
    return;
  } else if(%victim.isAdmin) {
    %victim.isAdmin = 0;
    %victim.isSuperAdmin = 0;
    %victim.sendPlayerListUpdate();
    commandtoclient(%victim,'setAdminLevel',0);
    messageAll('MsgAdminForce','\c2%1 has been De-Admined (Manual)',%victim.name);
  }
}

//- serverCmdRTB_adminPlayer (Makes a player an admin)
function serverCmdRTB_adminPlayer(%client,%victim) {
   if(!%client.isSuperAdmin)
      return;

   if((findLocalClient() $= %victim || %victim.bl_id $= getNumKeyID()) && %victim.isSuperAdmin) {
      messageClient(%client,'','\c2You cannot de-admin the host.');
      return;
   } else if(%victim.isSuperAdmin && %client.bl_id !$= getNumKeyID()) {
      messageClient(%client,'','\c2Only the host can de-admin a Super Admin.');
      return;
   } else if(!%victim.isAdmin || (%victim.isAdmin && %victim.isSuperAdmin)) {
      %victim.isAdmin = 1;
      %victim.isSuperAdmin = 0;
      %victim.sendPlayerListUpdate();
      commandtoclient(%victim,'setAdminLevel',1);
      messageAll('MsgAdminForce','\c2%1 has become Admin (Manual)',%victim.name);
   }
}

//- serverCmdRTB_superAdminPlayer (Makes a player a super admin)
function serverCmdRTB_superAdminPlayer(%client,%victim) {
   if(!%client.isSuperAdmin)
      return;

   if(!%victim.isSuperAdmin) {
      %victim.isAdmin = 1;
      %victim.isSuperAdmin = 1;
      %victim.sendPlayerListUpdate();
      commandtoclient(%victim,'setAdminLevel',2);
      messageAll('MsgAdminForce','\c2%1 has become Super Admin (Manual)',%victim.name);

      RTBSC_SendPrefList(%victim);
   }
}

//================================================================
// Server Options
//================================================================

//- serverCmdRTB_setServerOptions (Sets changed server options)
function serverCmdRTB_setServerOptions(%client,%notify,%options,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8,%v9,%v10,%v11,%v12,%v13,%v14,%v15,%v16)  {
  warn("serverCmdRTB_setServerOptions - implementation missing");
}

//- serverCmdRTB_getServerOptions (Sends server options to the client)
function serverCmdRTB_getServerOptions(%client) {
  warn("serverCmdRTB_getServerOptions - implementation missing");
}

//================================================================
// Prefs
//================================================================

//- serverCmdRTB_defaultServerPrefs (Reverts all prefs back to their defined default values)
function serverCmdRTB_defaultServerPrefs(%client) {
  messageClient(%client, "Not Supported", "Resetting preferences to their default values is currently not supported.");
}

//- serverCmdRTB_setServerPrefs (Updates the prefs on the server with those sent from the client)
function serverCmdRTB_setServerPrefs(%client,%prefs,%var0,%var1,%var2,%var3,%var4,%var5,%var6,%var7,%var8,%var9,%var10,%var11,%var12,%var13,%var14,%var,%var15) {
  for(%i = 0; %i < getWordCount(%prefs); %i++) {
    %varId = getWord(%prefs, %i);
    if(%varId $= "D") {
      break;
    }

    serverCmdUpdatePref(%client, %varId, %var[%i]);
  }
}

//================================================================
// Bans
//================================================================

function serverCmdRTB_clearBans(%client) {
  //direct copy
  if(!%client.isSuperAdmin)
     return;

  %cleared = 0;
  %numBans = BanManagerSO.numBans;
  for(%i=0;%i<%numBans;%i++)
  {
     BanManagerSO.removeBan(0);
     %cleared++;
  }
  BanManagerSO.saveBans();

  BanManagerSO.sendBanList(%client);

  echo("BAN LIST CLEARED by "@%client.name@" BL_ID:"@%client.bl_id@" IP:"@%client.getRawIP());
  echo("  +- bans cleared = "@%cleared);
}


//================================================================
// Package
//================================================================

//to keep the rest of the code clean, this will be self contained
// and will hook in to support_prefs via package

package SupportRTBPrefs {
  function Preference::onUpdate(%this, %val, %client) {
    for(%i = 0; %i < ClientGroup.getCount(); %i++) {
      %cl = ClientGroup.getObject(%i);
      if(%cl.isSuperAdmin && %cl.hasPrefList) {
        RTBSC_SendPrefValues(%cl);
      }
    }
  }

  function GameConnection::autoAdminCheck(%this) {
    %auto = Parent::autoAdminCheck(%this);
    if(%this.hasRTB) {
      commandtoclient(%this,'sendRTBVersion', "4.02"); //lets pretend
      RTBSC_SendPrefList(%this);
    }
    return %auto;
  }
};
activatePackage(SupportRTBPrefs);
