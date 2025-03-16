_watson_completion() {
    local cur prev words
    cur="${COMP_WORDS[COMP_CWORD]}"
    words=$(watson complete "$cur")
    
    COMPREPLY=($(compgen -W "$words" -- "$cur"))
}
complete -F _watson_completion watson
