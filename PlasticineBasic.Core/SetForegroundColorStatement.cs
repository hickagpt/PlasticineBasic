using System.Drawing;

namespace PlasticineBasic.Core;

public class SetForegroundColorStatement : StatementNode
{
    #region Public Properties

    public Color Color { get; set; }

    #endregion Public Properties

    #region Public Methods

    public ConsoleColor AsConsoleColor()
    {
        if (Color == Color.Red) return ConsoleColor.Red;
        if (Color == Color.Green) return ConsoleColor.Green;
        if (Color == Color.Blue) return ConsoleColor.Blue;
        if (Color == Color.Yellow) return ConsoleColor.Yellow;
        if (Color == Color.Cyan) return ConsoleColor.Cyan;
        if (Color == Color.Magenta) return ConsoleColor.Magenta;
        if (Color == Color.White) return ConsoleColor.White;
        if (Color == Color.Black) return ConsoleColor.Black;
        if (Color == Color.Gray) return ConsoleColor.Gray;
        if (Color == Color.DarkRed) return ConsoleColor.DarkRed;
        if (Color == Color.DarkGreen) return ConsoleColor.DarkGreen;
        if (Color == Color.DarkBlue) return ConsoleColor.DarkBlue;
        if (Color == Color.DarkCyan) return ConsoleColor.DarkCyan;
        if (Color == Color.DarkMagenta) return ConsoleColor.DarkMagenta;
        if (Color == Color.DarkGray) return ConsoleColor.DarkGray;
        if (Color == Color.LightGray) return ConsoleColor.Gray; // LightGray is not a console color, so we map it to Gray
        return ConsoleColor.White; // Default to White if no match
    }

    #endregion Public Methods
}