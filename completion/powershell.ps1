Register-ArgumentCompleter -CommandName watson -ScriptBlock {
    param($commandName, $wordToComplete, $cursorPosition)
    watson complete $wordToComplete | ForEach-Object { $_ }
}