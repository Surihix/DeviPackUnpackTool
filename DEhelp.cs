namespace DeviPackUnpackTool
{
    internal class DEhelp
    {
        public static void ShowCommands()
        {
            Console.WriteLine("Valid functions:");
            Console.WriteLine("-p = Pack a folder with files into a devi archive file");
            Console.WriteLine("-u = Unpack a devi archive file");
            Console.WriteLine("-? = Show valid app functions");
            Console.WriteLine("");
            Console.WriteLine("When using -p function, you will have to specify a compression level argument");
            Console.WriteLine("");
            Console.WriteLine("Valid compression levels:");
            Console.WriteLine("-c0 = No compression");
            Console.WriteLine("-c1 = Fastest compression");
            Console.WriteLine("-c2 = Optimal compression");
            Console.WriteLine("-c3 = Smallest size");
            Console.WriteLine("");
            Console.WriteLine("Usage Examples:");
            Console.WriteLine("To pack a folder: DeviPackUnpackTool -p " + @"""Folder To pack""" + " -c3");
            Console.WriteLine("To Unpack a file: DeviPackUnpackTool -u " + @"""archiveFile.devi""");
            Environment.Exit(0);
        }
    }
}