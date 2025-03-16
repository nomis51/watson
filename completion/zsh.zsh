#compdef watson

local words
words=($(watson complete "${words[-1]}"))

compadd -- $words