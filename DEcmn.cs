namespace DeviPackUnpackTool;

internal class DEcmn
{
    public static void ErrorExit(string errorMsg)
    {
        Console.WriteLine(errorMsg);
        Console.ReadLine();
        Environment.Exit(0);
    }

    public static void CheckAndDelFile(string fileName)
    {
        bool fileCheck = File.Exists(fileName);
        switch (fileCheck)
        {
            case true:
                File.Delete(fileName);
                break;

            case false:
                break;
        }
    }
}