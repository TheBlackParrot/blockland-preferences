//riding on Greek2Me's preloading script
//since we're essentially a necessity, we'll ensure that we load every time

exec("config/server/ADD_ON_LIST.cs");
$AddOn__Support_Preferences = 1; //I don't know what the actual name is, this is just what I (jincux) have been using
export("$AddOn__*", "config/server/ADD_ON_LIST.cs");

$BLPrefs::PreLoad = true;
if($Server::Dedicated) {
   //let the add-on know that it's being run NOW, not normally
  exec("./server.cs"); //we should load up first. That way, we capture ALL the rtb legacy prefs, not just the ones after us
  $BLPrefs::didLoad = true;

  echo("\c4[Support_Preferences] Version " @ $BLPrefs::Version @ " loaded!");
}
