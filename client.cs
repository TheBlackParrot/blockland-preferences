// --------------------------------------------------------------------------------------------
// Client-Side code.
// --------------------------------------------------------------------------------------------

$BLPrefs::ClVersion = "0.0-dev";

if(!$BLPrefs::Debug)
  return; //Support_Preferences is a support package, the client is for testing only

// SUPPORT
exec("./support/cl.cs");

// CLIENT STUFF
exec("./client/prefmanager/main.cs");
