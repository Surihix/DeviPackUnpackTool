using DeviPackUnpackTool;


try
{
    // Check the default argument length for help
    if (args.Length < 1)
    {
        Console.WriteLine("Warning: Enough arguments not specified");
        Console.WriteLine("");
        DEhelp.ShowCommands();
    }


    // Display Help page according to the argument
    if (args[0].Contains("-?") || args[0].Contains("-h"))
    {
        DEhelp.ShowCommands();
    }
    

    // Check the default argument length for tool actions
    if (args.Length < 2)
    {
        Console.WriteLine("Warning: Enough arguments not specified");
        Console.WriteLine("");
        DEhelp.ShowCommands();
    }


    var toolAction = args[0];
    var inFileOrFolder = args[1];


    // Check if compression level is specified if tool 
    // is set to use the pack function
    var cmpLvl = "";
    if (toolAction.Contains("-p"))
    {
        if (args.Length < 3)
        {
            DEcmn.ErrorExit("Error: Compression level is not specified");
        }

        cmpLvl = args[2];
        string[] validCmpLvls = { "-c0", "-c1", "-c2", "-c3" };

        if (!validCmpLvls.Contains(cmpLvl))
        {
            Console.WriteLine("Error: Valid Compression level is not specified");
            Console.WriteLine("");
            DEhelp.ShowCommands();
        }
    }


    // Check if specific file path is specified if tool 
    // is set to use the unpack single file function
    var specificFilePath = "";
    if (toolAction.Contains("-uf"))
    {
        if (args.Length < 3)
        {
            DEcmn.ErrorExit("Error: Specific file path is not specified");
        }

        specificFilePath = args[2];
    }


    // According to the specified tool action,
    // do the respective action
    bool inFolderDirExists = false;
    bool inFileExists = false;

    switch (toolAction)
    {
        case "-p":
            inFolderDirExists = Directory.Exists(inFileOrFolder);
            switch (inFolderDirExists)
            {
                case true:
                    DEpack.PackFolder(inFileOrFolder, cmpLvl);
                    break;

                case false:
                    DEcmn.ErrorExit("Error: Specified folder in the argument does not exist");
                    break;
            }
            break;

        case "-u":
            inFileExists = File.Exists(inFileOrFolder);
            switch (inFileExists)
            {
                case true:
                    DEunpack.UnpackFiles(inFileOrFolder);
                    break;

                case false:
                    DEcmn.ErrorExit("Error: Specified file in the argument does not exist");
                    break;
            }
            break;

        case "-uf":
            inFileExists = File.Exists(inFileOrFolder);
            switch (inFileExists)
            {
                case true:
                    DEunpack.UnpackSingleFile(inFileOrFolder, specificFilePath);
                    break;

                case false:
                    DEcmn.ErrorExit("Error: Specified file in the argument does not exist");
                    break;
            }
            break;

        case "-up":
            inFileExists = File.Exists(inFileOrFolder);
            switch (inFileExists)
            {
                case true:
                    DEunpack.UnpackFilePaths(inFileOrFolder);
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
}
catch (Exception ex)
{
    DEcmn.ErrorExit("Error: " + ex);
}