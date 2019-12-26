using CommandLine;

namespace UsfmConverterConsole
{
    class CommandOptions
    {
        [Option('i', "input", Required = true, HelpText = "Path to directory that contains usfm files.")]
        public string ProjectInput { get; set; }

        [Option('o', "output", Required = true, HelpText = "Path to the output directory.")]
        public string ProjectOutput { get; set; }

        [Option('j', "justified", Required = false, HelpText = "Justified text")]
        public bool IsJustified { get; set; }

        [Option('s', "double-space", Required = false, HelpText = "Double space line spacing")]
        public bool IsDoubleSpaced { get; set; }

        [Option('t', "two-columns", Required = false, HelpText = "Two columns text")]
        public bool HasTwoColumns { get; set; }

        [Option('b', "blank-column", Required = false, HelpText = "Two-column text where one column is blank. Ignored if two-columns mode is set.")]
        public bool HasBlankColumn { get; set; }

        [Option('r', "rtl", Required = false, HelpText = "Right-to-left text direction")]
        public bool IsR2LDirection { get; set; }

        [Option('c', "combine-chapter", Required = false, HelpText = "Combine chapters")]
        public bool WillCombineChapter { get; set; }

        [Option('v', "separate-verse", Required = false, HelpText = "Separate verses")]
        public bool WillSeparateVerse { get; set; }

        [Option('f', "font", Required = false, HelpText = "Set the font size (small, med or large).")]
        public string Font { get; set; }
    }
}
