using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SortLines
{
    class Program
    {
        private static readonly char SEP = ':';

        private static readonly int HEX_NUM_DIGITS = 2;

        private static readonly string APP_NAME = "SortLines",
                                       DEFAULT_ENCODING_WEBNAME = "utf-8-no-bom",
                                       BOM_BYTES_SEP = " ",
                                       BOM_BYTES_FMT = $"X{HEX_NUM_DIGITS}",
                                       NONE_STR = "none",
                                       
                                       // Switches
                                       DELETE_SRC_SWITCH = "-d",
                                       UNIQUE_LINES_SWITCH = "-u",
                                       CMP_SWITCH = $"-c{SEP}",
                                       ENC_SWITCH = $"-e{SEP}",
                                       FORCE_OVERWRITE_SWITCH = "-f",
                                       REVERSE_SORT_SWITCH = "-r",
                                       HELP_SWITCH = "-h";

        private static readonly int DEFAULT_ENCODING_IDX = EncodingHelper.EncodingWebNames.IndexOf(DEFAULT_ENCODING_WEBNAME),
                                    DEFAULT_STR_COMPARER_IDX = 4;

        private static readonly StringComparer argsComparer = StringComparer.OrdinalIgnoreCase;
        private static readonly StringComparison argsComparison = StringComparison.OrdinalIgnoreCase;

        private enum ExitCode
        {
            Success = 0, ErrWrongNumArgs, ErrCannotGetFullPaths, ErrUserProvidedDifferentFileNames,
            ErrSourceFileExists, ErrDestPathIsDirectory, ErrDestFileExists, ErrReadFile, 
            ErrWriteDestFile, ErrCannotDeleteSourceFile, ErrInvalidFormat, ErrOutOfRange, 
            ErrExpectedNumericValue, ErrDuplicateOption
        }

        static int Main(string[] args)
        {
            int encodingIdx = DEFAULT_ENCODING_IDX;
            int strComparerIdx = DEFAULT_STR_COMPARER_IDX;

            PrintAppName();

            if (HandleHelpRequest(args))
                return (int)ExitCode.Success;

            if (HandleEncodingsListRequest(args))
                return (int)ExitCode.Success;

            if (!UserProvidedMinimumArgsCount(args))
                return (int)ExitCode.ErrWrongNumArgs;

            string sourceFilePath = string.Empty;
            string destFilePath = string.Empty;

            if (!GetFilesFullPaths(args, ref sourceFilePath, ref destFilePath))
                return (int)ExitCode.ErrCannotGetFullPaths;

            if (!UserProvidedDifferentFileNames(sourceFilePath, destFilePath))
                return (int)ExitCode.ErrUserProvidedDifferentFileNames;

            if (!SourceFileExists(sourceFilePath)) 
                return (int)ExitCode.ErrSourceFileExists;

            if (DestPathIsDirectory(destFilePath)) 
                return (int)ExitCode.ErrDestPathIsDirectory;

            if (DestFileExists(args, destFilePath)) 
                return (int)ExitCode.ErrDestFileExists;

            int parseOptResult = ParseOptions(args, ref encodingIdx, ref strComparerIdx);

            if (parseOptResult != 0)
            {
                return parseOptResult;
            }

            Encoding encoding = EncodingHelper.GetEncoding(EncodingHelper.EncodingWebNames[encodingIdx]);
            string[] allLines = new string[] { };
            string printedBOM = encoding.GetPreamble().ListBytes(BOM_BYTES_SEP, BOM_BYTES_FMT);

            if (!ReadFile(sourceFilePath, encoding, ref allLines)) 
                return (int)ExitCode.ErrReadFile;

            int originalLineCount = allLines.Length;

            if (FileIsEmpty(allLines, originalLineCount))
                return (int)ExitCode.Success;

            if (FileContainsOneLine(originalLineCount))
                return (int)ExitCode.Success;

            Console.WriteLine($"Source: {sourceFilePath}");
            Console.WriteLine($"Destination: {destFilePath}");
            Console.WriteLine($"Requested encoding: {encodingIdx} - {EncodingHelper.EncodingWebNames[encodingIdx]}");
            Console.WriteLine($"Using encoding: {encoding.WebName}");
            Console.WriteLine($"Byte Order Mark: {(printedBOM.Length >= HEX_NUM_DIGITS ? printedBOM : NONE_STR)}");
            Console.WriteLine($"String comparer: {strComparerIdx} - {StringComparisonHelper.StrComparersDescr[strComparerIdx]}");

            if (args.Skip(2).Contains(UNIQUE_LINES_SWITCH, argsComparer))
            {
                allLines = allLines.Distinct(StringComparisonHelper.StrComparers[strComparerIdx]).ToArray();
                Console.WriteLine($"Request: remove duplicates.");
            }

            if (args.Skip(2).Contains(REVERSE_SORT_SWITCH, argsComparer))
            {
                allLines = allLines.OrderByDescending(s => s, StringComparisonHelper.StrComparers[strComparerIdx]).ToArray();
                Console.WriteLine("Request: reverse sorting.");
            }
            else
            {
                allLines = allLines.OrderBy(s => s, StringComparisonHelper.StrComparers[strComparerIdx]).ToArray();
            }

            if (args.Skip(2).Contains(DELETE_SRC_SWITCH, argsComparer))
            {
                Console.WriteLine($"Request: delete source file.");
            }

            if (args.Skip(2).Contains(FORCE_OVERWRITE_SWITCH, argsComparer))
            {
                Console.WriteLine($"Request: overwrite destination file.");
            }

            Console.WriteLine();

            if (!WriteDestFile(destFilePath, encoding, allLines)) 
                return (int)ExitCode.ErrWriteDestFile;

            if (args.Skip(2).Contains(UNIQUE_LINES_SWITCH, argsComparer))
            {
                Console.WriteLine($"Duplicate lines removed: {originalLineCount - allLines.Length}.");
            }

            Console.WriteLine($"{allLines.Length} lines written to {destFilePath}.");

            if (!MaybeDeleteSourceFile(args, sourceFilePath)) 
                return (int)ExitCode.ErrCannotDeleteSourceFile;

            return (int)ExitCode.Success;
        }

        private static bool MaybeDeleteSourceFile(string[] args, string sourceFilePath)
        {
            if (args.Skip(2).Contains(DELETE_SRC_SWITCH, argsComparer))
            {
                if (!File.Exists(sourceFilePath))
                {
                    Console.WriteLine($"ERROR: Cannot find file ({sourceFilePath}).");
                    return false;
                }

                try
                {
                    File.Delete(sourceFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Cannot delete file ({sourceFilePath}). Details: {ex.Message}");
                    return false;
                }

                Console.WriteLine($"Deleted: {sourceFilePath}");
            }

            return true;
        }

        private static bool WriteDestFile(string destFilePath, Encoding encoding, string[] allLines)
        {
            try
            {
                File.WriteAllLines(destFilePath, allLines, encoding);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Cannot write to file ({destFilePath}). Details: {ex.Message}");
                return false;
            }

            return true;
        }

        private static bool FileContainsOneLine(int originalLineCount)
        {
            if (originalLineCount == 1)
            {
                Console.WriteLine("INFO: Source file contains only one line. Nothing to sort.");
                return true;
            }

            return false;
        }

        private static bool FileIsEmpty(string[] allLines, int originalLineCount)
        {
            if (originalLineCount == 0)
            {
                Console.WriteLine("INFO: Source file is empty. Nothing to sort.");
                return true;
            }

            if (allLines.All(string.IsNullOrEmpty))
            {
                Console.WriteLine("INFO: Source file contains only empty lines. Nothing to sort.");
                return true;
            }

            return false;
        }

        private static bool ReadFile(string sourceFilePath, Encoding encoding, ref string[] allLines)
        {
            try
            {
                allLines = File.ReadAllLines(sourceFilePath, encoding);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Cannot read file ({sourceFilePath}). Details: {ex.Message}");
                return false;
            }

            return true;
        }

        private static bool DestFileExists(string[] args, string destFilePath)
        {
            if (File.Exists(destFilePath) &&
                !args.Skip(2).Contains(FORCE_OVERWRITE_SWITCH, argsComparer))
            {
                Console.WriteLine("ERROR: Destination file already exists. Use -f to overwrite.");
                return true;
            }

            return false;
        }

        private static bool DestPathIsDirectory(string destFilePath)
        {
            if (Directory.Exists(destFilePath))
            {
                Console.WriteLine("ERROR: Destination path points to a directory.");
                return true;
            }

            return false;
        }

        private static bool SourceFileExists(string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath))
            {
                Console.WriteLine($"ERROR: File ({sourceFilePath}) not found.");
                return false;
            }

            return true;
        }

        private static bool UserProvidedDifferentFileNames(string sourceFilePath, string destFilePath)
        {
            if (string.Equals(sourceFilePath, destFilePath, argsComparison))
            {
                Console.WriteLine("ERROR: Source and destination are the same file.");
                return false;
            }

            return true;
        }

        private static bool GetFilesFullPaths(string[] args, ref string sourceFilePath, ref string destFilePath)
        {
            try
            {
                sourceFilePath = Path.GetFullPath(args[0]);
                destFilePath = Path.GetFullPath(args[1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Cannot get full file path(s). Details: {ex.Message}");
                return false;
            }

            return true;
        }

        private static bool UserProvidedMinimumArgsCount(string[] args)
        {
            if (args.Length >= 2)
            {
                return true;
            }

            Console.WriteLine("ERROR: Wrong number of arguments.");
            return false;
        }

        private static bool HandleEncodingsListRequest(string[] args)
        {
            if (args.Length == 1 && string.Equals(args[0], ENC_SWITCH, argsComparison))
            {
                PrintEncodings();
                return true;
            }

            return false;
        }

        private static bool HandleHelpRequest(string[] args)
        {
            if (args.Length == 0 || (args.Length == 1 && string.Equals(args[0], HELP_SWITCH, argsComparison)))
            {
                PrintExtendedUsage();
                return true;
            }

            return false;
        }

        private static void PrintEncodings()
        {
            Console.WriteLine("Encodings:");

            for (int i = 0; i < EncodingHelper.EncodingWebNames.Count; i++)
            {
                Console.WriteLine($"\t{i}: {EncodingHelper.EncodingWebNames[i]}");
            }
        }

        private static void PrintExtendedUsage()
        {
            PrintBasicUsage();
            Console.WriteLine();

            PrintOptions();
            Console.WriteLine();
            
            PrintStringComparers();
            Console.WriteLine();
            
            PrintSupportedEncBasicHelp();
            Console.WriteLine();
            
            PrintExamples();
        }

        private static void PrintExamples()
        {
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine($"\t{APP_NAME} urls.txt sorted-urls.txt");
            Console.WriteLine($"\t{APP_NAME} urls.txt sorted-urls.txt {CMP_SWITCH}5");
            Console.WriteLine($"\t{APP_NAME} input.txt output.txt {DELETE_SRC_SWITCH}");
            Console.WriteLine($"\t{APP_NAME} input.txt output.txt {UNIQUE_LINES_SWITCH}");
            Console.WriteLine($"\t{APP_NAME} input.txt output.txt {DELETE_SRC_SWITCH} {UNIQUE_LINES_SWITCH} {ENC_SWITCH}43");
            Console.WriteLine($"\t{APP_NAME} {ENC_SWITCH}");
            Console.WriteLine($"\t{APP_NAME} input.txt output.txt {FORCE_OVERWRITE_SWITCH}");
            Console.WriteLine($"\t{APP_NAME} input.txt output.txt {REVERSE_SORT_SWITCH}");
        }

        private static void PrintSupportedEncBasicHelp()
        {
            Console.WriteLine($"Encodings: pass ");
            Console.WriteLine($"\t{ENC_SWITCH}\tas the sole argument to display a list of encodings.");
            Console.WriteLine($"\t\tPlease note that not all listed encodings are");
            Console.WriteLine($"\t\tnecessarily available on your system.");
            Console.WriteLine($"\t\tDefault/fallback encoding: {DEFAULT_ENCODING_IDX} ({DEFAULT_ENCODING_WEBNAME}).");
        }

        private static void PrintBasicUsage()
        {
            Console.WriteLine("USAGE: ");
            Console.WriteLine("\tPass the path to the file whose lines have to be sorted ");
            Console.WriteLine("\t(FIRST argument) and the path to the destination file ");
            Console.WriteLine("\t(SECOND argument).");
        }

        private static void PrintOptions()
        {
            Console.WriteLine($"OPTIONALLY, pass (THIRD argument onwards):");
            Console.WriteLine($"\t{DELETE_SRC_SWITCH}\tto delete the source file when done;");
            Console.WriteLine($"\t{UNIQUE_LINES_SWITCH}\tto remove duplicates;");
            Console.WriteLine($"\t{CMP_SWITCH}n\tto choose the string comparer.");
            Console.WriteLine($"\t\tIf not specified or invalid, defaults to ");
            Console.WriteLine($"\t\t{DEFAULT_STR_COMPARER_IDX} ({StringComparisonHelper.StrComparersDescr[DEFAULT_STR_COMPARER_IDX]}).");
            Console.WriteLine($"\t{ENC_SWITCH}n\tto choose the source/destination encoding.");
            Console.WriteLine($"\t\tIf not specified or invalid, defaults to {DEFAULT_ENCODING_IDX} ({EncodingHelper.EncodingWebNames[DEFAULT_ENCODING_IDX]}).");
            Console.WriteLine($"\t{FORCE_OVERWRITE_SWITCH}\tto overwrite an existing destination file.");
            Console.WriteLine($"\t{REVERSE_SORT_SWITCH}\tto sort lines in descending order.");
        }

        private static void PrintStringComparers()
        {
            Console.WriteLine("Available string comparers:");

            for (int i = 0; i < StringComparisonHelper.StrComparersDescr.Count; i++)
            {
                Console.WriteLine($"\t{i}: {StringComparisonHelper.StrComparersDescr[i]}");
            }
        }

        private static void PrintAppName()
        {
            Console.Write($"{APP_NAME}\r\n\r\n");
        }

        private static int ParseOptions(string[] args, ref int encodingIdx, ref int strComparerIdx)
        {
            bool cmpSwitchFound = false;
            bool encSwitchFound = false;

            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].StartsWith(CMP_SWITCH, argsComparison))
                {
                    if (cmpSwitchFound)
                    {
                        Console.WriteLine("ERROR: Duplicate comparer option.");
                        return (int)ExitCode.ErrDuplicateOption;
                    }

                    cmpSwitchFound = true;

                    string[] cmpArr = args[i].Split(SEP);

                    if (cmpArr.Length != 2)
                    {
                        Console.WriteLine($"ERROR: Invalid format in {args[i]}.");
                        return (int)ExitCode.ErrInvalidFormat;
                    }

                    if (int.TryParse(cmpArr[1], out strComparerIdx))
                    {
                        if (strComparerIdx < 0 || strComparerIdx >= StringComparisonHelper.StrComparers.Count)
                        {
                            Console.WriteLine($"ERROR: out-of-range number in {args[i]}.");
                            return (int)ExitCode.ErrOutOfRange;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: expected numeric value in {args[i]}.");
                        return (int)ExitCode.ErrExpectedNumericValue;
                    }
                }
                else if (args[i].StartsWith(ENC_SWITCH, argsComparison))
                {
                    if (encSwitchFound)
                    {
                        Console.WriteLine("ERROR: Duplicate encoding option.");
                        return (int)ExitCode.ErrDuplicateOption;
                    }

                    encSwitchFound = true;

                    string[] encArr = args[i].Split(SEP);

                    if (encArr.Length != 2)
                    {
                        Console.WriteLine($"ERROR: Invalid format in {args[i]}.");
                        return (int)ExitCode.ErrInvalidFormat;
                    }

                    if (int.TryParse(encArr[1], out encodingIdx))
                    {
                        if (encodingIdx < 0 || encodingIdx >= EncodingHelper.EncodingWebNames.Count)
                        {
                            Console.WriteLine($"ERROR: out-of-range number in {args[i]}.");
                            return (int)ExitCode.ErrOutOfRange;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: expected numeric value in {args[i]}.");
                        return (int)ExitCode.ErrExpectedNumericValue;
                    }
                }
            }

            return (int)ExitCode.Success;
        }
    }
}
