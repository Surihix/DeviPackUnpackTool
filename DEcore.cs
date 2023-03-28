using DeviPackUnpackTool;


try
{
    // Check the default argument length for help
    bool CheckHelpArgLength = args.Length < 1;
    switch (CheckHelpArgLength)
    {
        case true:
            Console.WriteLine("Warning: Enough arguments not specified");
            Console.WriteLine("");
            DEhelp.ShowCommands();
            break;
        case false:
            break;
    }

    // Display Help page according to the argument
    bool CheckIfArgIsHelp = args[0].Contains("-?") || args[0].Contains("-h");
    switch (CheckIfArgIsHelp)
    {
        case true:
            DEhelp.ShowCommands();
            break;

        case false:
            break;
    }


    // Check the default argument length
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


    // Check if compression level is specified if tool 
    // is set to use the pack function
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


    // Check if Specific file is specified if tool 
    // is set to use the unpack single file function
    var SpecificFilePath = "";
    bool CheckIfSetToUnpkOneFile = ToolAction.Contains("-uf");
    switch (CheckIfSetToUnpkOneFile)
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

            SpecificFilePath = args[2];
            break;

        case false:
            break;
    }


    // According to the specified tool action,
    // do the respective action
    bool InFolderDirExists = false;
    bool InFileExists = false;

    switch (ToolAction)
    {
        case "-p":
            InFolderDirExists = Directory.Exists(InFileOrFolder);
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
            InFileExists = File.Exists(InFileOrFolder);
            switch (InFileExists)
            {
                case true:
                    DEunpack.UnpackFiles(InFileOrFolder);
                    break;

                case false:
                    DEcmn.ErrorExit("Error: Specified file in the argument does not exist");
                    break;
            }
            break;

        case "-uf":
            InFileExists = File.Exists(InFileOrFolder);
            switch (InFileExists)
            {
                case true:
                    DEunpack.UnpackSingleFile(InFileOrFolder, SpecificFilePath);
                    break;

                case false:
                    DEcmn.ErrorExit("Error: Specified file in the argument does not exist");
                    break;
            }
            break;

        case "-up":
            InFileExists = File.Exists(InFileOrFolder);
            switch (InFileExists)
            {
                case true:
                    DEunpack.UnpackFilePaths(InFileOrFolder);
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