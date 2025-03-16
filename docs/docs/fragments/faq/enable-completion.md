‎

#### How can I enable completion?

The application supports completion for the following terminals :

**powershell**

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force; $script = irm "https://raw.githubusercontent.com/nomis51/watson/master/completion/powershell.ps1"; iex $script; $script | Out-File -Append $PROFILE
```

**bash**

```bash
bash -c 'curl -sL "https://raw.githubusercontent.com/nomis51/watson/master/completion/bash.sh" | tee -a ~/.bashrc | bash'
```

**zsh**

```zsh
zsh -c 'curl -sL "https://raw.githubusercontent.com/nomis51/watson/master/completion/zsh.sh" | tee -a ~/.zshrc | zsh'
```
