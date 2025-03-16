#### What is a frame?

It's a task or an activity. Basically what you're working on. Usually it's a simple name, like
`cooking` or 'project-abc'. You can also add as many `tags` as you want to describe the `frame`.

For example, if you're cooking a pizza, you could say `watson start cooking pizza`.
The project is `cooking` the tag is `pizza`.

Tags can help you find frames related to specific tasks within a project.

#### Is the application using 12h or 24h time?

The application uses 24h time. It's much easier to work with and remove the ambiguity of 12h time.

#### Do I need to create a special project for lunchtime?

No, you can specify your lunch time in the settings of the application.
By default, it's set to `12:00` to `13:00`.

#### Where are the application data stored?

All the data of the application is stored in a folder called `.watson` in your home directory.

- On Windows, it's `C:\Users\<username>\.watson`
- On Linux and macOS, it's `~/.watson`
- On macOS, it's `/Users/<username>/.watson`

The directory contains several elements :

- The `logs` directory contains the logs of the application
- The `settings.json` file contains the settings of the application (which you can also edit through the application)
- The database file `data.db` which contains the frames, projects, tags, etc.

#### How does time input works?

There are several parsing rules available to speed up the time input process, so you don't have to type the date time.
Of course, you can always type the full date time manually if you want to, but here are some shortcuts to save up some
time.

- 1-2 digits means `hour`. For example
    - `3` means `03:00`
    - `13` means `13:00`
    - `88` means `08:08`
- 3-4 digits means `hour:minute`. For example
    - `1234` means `12:34`
    - `845` means `08:45`
    - `138` means `13:08`
- 2 sets of digits means `day hour`. For example
    - `13 14` means `13th` of the current month at `14:00`
    - `3 1245` means `3rd` of the current month at `12:45`
- hyphen separated digits means `year-month-day`. For example
    - `3-13` means `March 13th` of the current year
    - `10-23 852` means `October 23rd` of the current year at `08:52`
    - `2023-4-13 14` means `April 13th` of `2023` at `14:00`

#### How does a frame start time and end time work?

A frame is actually simply a point in time. It only stores the time at which it was created.
That means that the end time of a frame is technically the start time of "next thing" that comes after it.
This design removes the tedious task of having to perfectly mix and match the start and end time of tasks.

For example, you're working on something, then your boss calls you for something else at 9:13 for 2 minutes,
then you go back on your project, but then a coworker calls you for something 4 minutes later. After all of that,
you realized you forgot to add your tasks in the application. If you'd need to manage end times, that will probably
look like this :

- Add a task "call with the boss" from 9:13 to 9:15
- Add a task "work on project" from 9:15 to 9:20
- Add a task "call with the coworker" from 9:20 to 9:24
- Add a task "work on project" from 9:24 to now

Kind of tedious, isn't it?

Now, what if you only need to remember the start time, it's much easier :

- Add a task "call with the boss" at 9:13
- Add a task "work on project" at 9:15
- Add a task "call with the coworker" at 9:20
- Add a task "work on project" at 9:24

And all frames are gonna adjust automatically, no more "when did it end? how long was the call? etc."!

But, how does it knows when the day ends?
Well, there are settings for that. You can set your work hours in the settings of the application.
By default, it's set to `8:00` to `16:00`.

#### How can I add completion?

The application supports completion for the following terminals :

**powershell**

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force; $script=irm "https://raw.githubusercontent.com/nomis51/watson/master/completion/powershell.ps1"; iex $script; $script | Out-File -Append $PROFILE
```

**bash**

```bash
bash -c 'curl -sL "https://raw.githubusercontent.com/nomis51/watson/master/completion/bash.sh" | tee -a ~/.bashrc | bash'
```

**zsh**

```zsh
zsh -c 'curl -sL "https://raw.githubusercontent.com/nomis51/watson/master/completion/zsh.sh" | tee -a ~/.zshrc | zsh'
```




