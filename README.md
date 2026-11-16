# SortLines

SortLines is a command-line utility written in C# for sorting text files.

## Requirements

- Windows
- .NET Framework 3.5
- Platform target: AnyCPU

## Tested environment

- Windows 7 SP1 (64-bit)

## Features

- Sort text files in ascending or descending order.
- Remove duplicate lines.
- Select the comparer used for sorting.
- Select the text encoding.
- Display the list of available encodings.
- Optionally delete the source file after processing.
- Control destination file overwriting.

## Usage

```text
SortLines <source> <destination> [options]
```

The first two arguments specify the source file and destination file. Additional arguments control the sorting operation.

## Options

| Option | Description |
|---|---|
| `-d` | Delete the source file after writing the destination. |
| `-u` | Remove duplicate lines. |
| `-c:n` | Select the comparer by index. |
| `-e:n` | Select the text encoding by index. |
| `-e:` | Display available encodings. |
| `-f` | Overwrite the destination file. |
| `-r` | Sort in descending order. |
| `-h` | Display help. |

Running the program without arguments also displays help.

## Default settings

- **Comparer:** index `4` (`StringComparer.Ordinal`). Performs a case-sensitive, culture-independent comparison based on the Unicode values of the characters. Uppercase and lowercase letters are treated as different characters.
- **Encoding:** `utf-8-no-bom` (UTF-8 without a byte order mark).

## Author

**ar2731**

[GitHub profile](https://github.com/ar2731)

[Source code](https://github.com/ar2731/SortLines)
