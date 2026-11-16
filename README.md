# SortLines

SortLines is a command-line utility written in C# for sorting text files.

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

The default comparer is index `4`, and the default encoding is `utf-8-no-bom`.
