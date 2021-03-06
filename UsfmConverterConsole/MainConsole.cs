﻿using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using USFMToolsSharp;
using USFMToolsSharp.Models.Markers;
using USFMToolsSharp.Renderers.HTML;

namespace UsfmConverterConsole
{
    class MainConsole
    {
        private string folderName = "";
        private string projectOutput = "";

        private bool isTextJustified = false;
        private bool isSingleSpaced = true;
        private bool hasOneColumn = true;
        private bool hasBlankColumn = false;
        private bool isL2RDirection = true;
        private bool willSeparateChap = true;
        private bool willSeparateVerse = false;

        private HTMLConfig configHTML;

        private Dictionary<double, string> LineSpacingClasses;
        private string[] ColumnClasses;
        private string[] TextDirectionClasses;
        private string[] TextAlignmentClasses;
        private string fontClass;

        public MainConsole(string[] args)
        {
            this.ParseArguments(args);

            LineSpacingClasses = new Dictionary<double, string>
            {
                [1] = "single-space",
                [1.5] = "one-half-space",
                [2] = "double-space",
                [2.5] = "two-half-space",
                [3] = "triple-space"
            };
            ColumnClasses = new string[] { "", "two-column" };
            TextDirectionClasses = new string[] { "", "rtl-direct" };
            TextAlignmentClasses = new string[] { "", "right-align", "center-align", "justified" };
        }

        public void Run()
        {
            // Does not parse through section headers yet
            var parser = new USFMParser(new List<string> { "s5" });

            configHTML = BuildConfig();
            //Configure Settings -- Spacing ? 1, Column# ? 1, TextDirection ? L2R 
            var renderer = new HtmlRenderer(configHTML);

            // Added ULB License and Page Number
            renderer.FrontMatterHTML = GetLicenseInfo();
            renderer.InsertedFooter = GetFooterInfo();

            var usfm = new USFMDocument();

            if (folderName.Equals(string.Empty)) return;

            var dirinfo = new DirectoryInfo(folderName);

            if (!dirinfo.Exists)
            {
                Console.WriteLine("The input path doesn't exist. Make sure you spelled it correctly");
                return;
            }

            if(!Directory.Exists(projectOutput))
            {
                Console.WriteLine("The output path doesn't exist. Make sure you spelled it correctly");
                return;
            }

            var allFiles = dirinfo.GetFiles("*.usfm", SearchOption.AllDirectories)
                .Union(dirinfo.GetFiles("*.txt", SearchOption.AllDirectories)).ToArray();
            Array.Sort(allFiles, delegate (FileInfo a, FileInfo b)
            {
                return a.FullName.CompareTo(b.FullName);
            });
            var progress = allFiles.Length;
            var progressStep = 0;

            foreach (FileInfo fileInfo in allFiles)
            {
                var text = File.ReadAllText(fileInfo.FullName);
                usfm.Insert(parser.ParseFromString(text));
                progressStep++;
                Console.Write("\r[{0}%] ", (int)(progressStep / (float)progress * 100));
            }

            var html = renderer.Render(usfm);
            var htmlFilename = Path.Combine(projectOutput, "out.html");

            File.WriteAllText(htmlFilename, html);

            var dirname = Path.GetDirectoryName(htmlFilename);
            var cssFilename = Path.Combine(dirname, "style.css");
            if (!File.Exists(cssFilename))
            {
                File.Copy("style.css", cssFilename);
            }

            Console.WriteLine("Your project was successfully converted!");
        }

        private void ParseArguments(string[] args)
        {
            Parser.Default.ParseArguments<CommandOptions>(args)
                   .WithParsed<CommandOptions>(o =>
                   {
                       folderName = o.ProjectInput;

                       if (o.ProjectOutput != null && !o.ProjectOutput.Equals(String.Empty))
                       {
                           projectOutput = o.ProjectOutput;
                       }

                       isTextJustified = o.IsJustified;
                       isSingleSpaced = !o.IsDoubleSpaced;
                       hasOneColumn = !o.HasTwoColumns;
                       hasBlankColumn = o.HasBlankColumn && hasOneColumn;
                       isL2RDirection = !o.IsR2LDirection;
                       willSeparateChap = !o.WillCombineChapter;
                       willSeparateVerse = o.WillSeparateVerse;

                       if (o.Font != null)
                       {
                           if (o.Font.Equals("small"))
                           {
                               fontClass = "small-text";
                           }
                           else if (o.Font.Equals("med"))
                           {
                               fontClass = "med-text";
                           }
                           else if (o.Font.Equals("large"))
                           {
                               fontClass = "large-text";
                           }
                       }
                   });
        }

        private string GetLicenseInfo()
        {
            // Identifies License within Directory 
            string ULB_License_Doc = "insert_ULB_License.html";
            FileInfo f = new FileInfo(ULB_License_Doc);
            string licenseHTML = "";

            if (File.Exists(ULB_License_Doc))
            {
                licenseHTML = File.ReadAllText(ULB_License_Doc);
            }
            return licenseHTML;
        }
        private string GetFooterInfo()
        {
            // Format --  June 13, 2019 11:42
            string dateFormat = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
            string footerHTML = $@"
            <div class=FooterSection>
            <table id='hrdftrtbl' border='0' cellspacing='0' cellpadding='0'>
            <div class=FooterSection>
            <table id='hrdftrtbl' border='0' cellspacing='0' cellpadding='0'>
            <tr><td>
            <div style='mso-element:footer' id=f1>
            <p class=MsoFooter></p>
            {dateFormat}
            <span style=mso-tab-count:1></span>
            <span style='mso-field-code: PAGE '></span><span style='mso-no-proof:yes'></span></span>
            <span style=mso-tab-count:1></span>
            <img alt='Creative Commons License' style='border-width:0' src='https://i.creativecommons.org/l/by-sa/4.0/88x31.png' />
            </p>
            </div>
            </td></tr>
            </table>
            </div> ";
            return footerHTML;
        }

        private HTMLConfig BuildConfig()
        {
            HTMLConfig config = new HTMLConfig();
            if (!isSingleSpaced)
            {
                config.divClasses.Add(LineSpacingClasses[2.0]);
            }
            if (!hasOneColumn)
            {
                config.divClasses.Add(ColumnClasses[1]);
            }
            if (!isL2RDirection)
            {
                config.divClasses.Add(TextDirectionClasses[1]);
            }
            if (isTextJustified)
            {
                config.divClasses.Add(TextAlignmentClasses[3]);
            }
            config.divClasses.Add(fontClass);

            // Will be added to HTML config class 
            config.separateVerses = willSeparateVerse;

            config.separateChapters = willSeparateChap;

            config.blankColumn = hasBlankColumn;

            return config;
        }
    }
}
