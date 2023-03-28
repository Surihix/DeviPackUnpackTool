using DeviPackUnpackTool;


bool CheckIfArgIsHelp = args[0].Contains("-?") || args[0].Contains("-h");
switch (CheckIfArgIsHelp)
{
    case true:
        DEhelp.ShowCommands();
        break;

    case false:
        break;
}

bool CheckArgsLength = args.Length < 2;
switch (CheckArgsLength)
{
    case true:
        Console.WriteLine("Warning: Enough arguments not specified");
        Console.WriteLine("");
        DEhelp.ShowCommands();
        break;
    case false:
        break;
}

var ToolAction = args[0];
var InFileOrFolder = args[1];

var CompLvl = "";
bool CheckIfSetToPack = ToolAction.Contains("-p");
switch (CheckIfSetToPack)
{
    case true:
        bool CheckArgCount = args.Length > 2;

        switch (CheckArgCount)
        {
            case true:
                break;

            case false:
                DEcmn.ErrorExit("Error: Compression level is not specified");
                break;
        }

        CompLvl = args[2];

        string[] ValidCompLvls = { "-c0", "-c1", "-c2", "-c3" };

        bool CheckIfCompLvlIsValid = ValidCompLvls.Contains(CompLvl);
        switch (CheckIfCompLvlIsValid)
        {
            case true:
                break;

            case false:
                Console.WriteLine("Error: Valid Compression level is not specified");
                Console.WriteLine("");
                DEhelp.ShowCommands();
                break;
        }
        break;

    case false:
        break;
}

switch (ToolAction)
{
    case "-p":
        bool InFolderDirExists = Directory.Exists(InFileOrFolder);
        switch (InFolderDirExists)
        {
            case true:
                DEpack.PackFolder(InFileOrFolder, CompLvl);
                break;

            case false:
                DEcmn.ErrorExit("Error: Specified folder in the argument does not exist");
                break;
        }
        break;

    case "-u":
        bool InFileExists = File.Exists(InFileOrFolder);
        switch (InFileExists)
        {
            case true:
                DEunpack.UnpackFile(InFileOrFolder);
                break;

            case false:
                DEcmn.ErrorExit("Error: Specified file in the argument does not exist");
                break;
        }
        break;

    default:
        DEcmn.ErrorExit("Error: Specified tool action is invalid");
        break;
}