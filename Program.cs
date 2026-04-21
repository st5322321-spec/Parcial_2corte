using Parqueo.Core;
using Parqueo.UI;

var map     = new ParkingMap();
var manager = new ParkingManager(map);
var stats   = new Statistics(map, manager);
var menu    = new MenuHandler(manager, stats);

menu.Run();
