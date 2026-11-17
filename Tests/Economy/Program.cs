using System;

public static class Program
{
    public static int Main()
    {
        try { Console.WriteLine("PASS: " + EconomyChecks.Run() + " economy and mission checks"); return 0; }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
