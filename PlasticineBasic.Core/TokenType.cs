namespace PlasticineBasic.Core
{
    public enum TokenType
    {
        // === Special ===

        EndOfLine,       // Line break or end of statement separator

        EndOfFile,       // End of the entire input

        // === Literals ===
        NumberLiteral,   // e.g., 123, 3.14

        StringLiteral,   // e.g., "Hello world"
        Identifier,      // e.g., A, X1, MYVAR

        // === Keywords ===
        Let,             // LET A = 10

        Random,
        Print,           // PRINT "Hello"
        PrintLine,       // RAND A
        SetForegroundColor, // FGCOLOR 4
        SetBackgroundColor, // BGCOLOR 2
        Input,           // INPUT A
        If,              // IF A = 1 THEN ...
        Then,            // THEN keyword
        Else,            // Optional: ELSE support
        Goto,            // GOTO 100
        Gosub,           // GOSUB 200
        Return,          // RETURN from GOSUB
        For,             // FOR I = 1 TO 10
        To,              // TO in FOR loops
        Step,            // STEP in FOR loops
        Next,            // NEXT I
        Dim,             // DIM A(10)
        Rem,             // REM comments
        End,             // END of program
        Stop,            // STOP execution
        Def,             // DEF FN
        Fn,              // FN(x)
        Sub,             // SUB procedure

        // === Operators ===
        Plus,            // +

        Minus,           // -
        Multiply,        // *
        Divide,          // /
        Modulus,         // MOD or %
        Power,           // ^

        Assign,          // = (for LET)
        Equal,           // = (for comparisons)
        NotEqual,        // <>
        LessThan,        // <
        GreaterThan,     // >
        LessThanOrEqual, // <=
        GreaterThanOrEqual, // >=

        // === Punctuation ===
        LeftParen,       // (

        RightParen,      // )
        Comma,           // ,
        Colon,           // : (statement separator)

        // === Misc ===
        LineNumber,      // Optional: distinguish numeric labels from normal numbers

        Comment,         // Entire REM line or anything after `'`
    }
}