namespace DeviPackUnpackTool
{
    internal class DEcmn
    {
        public static void ErrorExit(string ErrorMsg)
        {
            Console.WriteLine(ErrorMsg);
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}