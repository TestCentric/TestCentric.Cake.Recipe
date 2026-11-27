// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

public static partial class Program
{
    /// <summary>
    /// Displays a banner similar to those created by Cake.
    /// We use this to display intermediate steps within a task.
    /// </summary>
    public static void DisplayBanner(string message, char barChar='-', int length=0)
    {
        if (length == 0) length = MaxLineLength(message);
        var bar = new string(barChar, length);

        Console.WriteLine();
        Console.WriteLine(bar);
        Console.WriteLine(message);
        Console.WriteLine(bar);

        int MaxLineLength(string message)
        {
            int length = 0;
            foreach (string line in message.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                length = Math.Max(length, line.Length);
            return length;
        }
    }
}
