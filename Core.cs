using DeviPackUnpackTool;


bool CheckArgs = args.Length < 2;
switch (CheckArgs)
{
    case true:
        DEcmnMethods.ErrorExit("Error: Enough arguments not specified");
        break;
    case false:
        break;
}

var ToolAction = args[0];
var InFileOrFolder = args[1];

switch (ToolAction)
{
    case "-p":
        bool InFolderDirExists = Directory.Exists(InFileOrFolder);
        switch (InFolderDirExists)
        {
            case true:
                DEpack.PackFolder(InFileOrFolder);
                break;

            case false:
                DEcmnMethods.ErrorExit("Error: Specified folder in the argument does not exist");
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
                DEcmnMethods.ErrorExit("Error: Specified file in the argument does not exist");
                break;
        }
        break;

    default:
        DEcmnMethods.ErrorExit("Error: Specified tool action is invalid");
        break;
}