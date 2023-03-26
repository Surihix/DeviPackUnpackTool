namespace DeviPackUnpackTool
{
    internal class DEcmnMethods
    {
        public static void ErrorExit(string ErrorMsg)
        {
            Console.WriteLine(ErrorMsg);
            Console.ReadLine();
            Environment.Exit(0);
        }

        public static void CheckAndDelFile(string FileName)
        {
            bool FileCheck = File.Exists(FileName);
            switch (FileCheck)
            {
                case true:
                    File.Delete(FileName);
                    break;

                case false:
                    break;
            }
        }
    }
}