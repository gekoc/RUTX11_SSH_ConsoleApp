using RUTX11_SSH_ConsoleApp;

var userLog = new UserLog();
var console = new ConsoleIO();
var router = new Router(userLog, console);
new App(userLog, console, router).Run();