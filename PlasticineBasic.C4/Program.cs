﻿// See https://aka.ms/new-console-template for more information
using CommandLine;
using PlasticineBasic.Core;

CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed(options =>
    {
        // Here you would typically process the options
        Console.WriteLine($"Source File: {options.SourceFile}");
        Console.WriteLine($"Verbose Mode: {options.Verbose}");

        var sourceCode = File.ReadAllText(options.SourceFile);

        var tokeniser = new Tokeniser(sourceCode, options.Verbose);
        var tokens = tokeniser.Tokenise();
        var tokensString = string.Join(Environment.NewLine, tokens.Select(t => $"{t.Type} - {t.Value} at {t.Line}:{t.Column}"));

        var parser = new PlasticineBasic.Core.Parser(tokens, options.Verbose);
        var program = parser.Parse();
        var interpreter = new Interpreter(options.Verbose);

        interpreter.Execute(program);
    })
    .WithNotParsed(errors =>
    {
        // Handle errors in parsing
        foreach (var error in errors)
        {
            Console.WriteLine($"Error: {error.Tag}");
        }
    });

public class Options
{
    #region Public Properties

    [Option('f', "file", Required = true, HelpText = "Path to the source file to be processed.")]
    public string SourceFile { get; set; }

    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }

    #endregion Public Properties
}