# Watson

A simple terminal based time tracking application highly inspired by td-watson.

## Get started

### Installation

#### Using dotnet

```
dotnet install --global nomis51.Watson
```

#### Standalone

Go to the Release page and download the latest build.

## Usage

You can start tracking an activity or task (which is called a `frame` in the application) with the command

```
watson start cooking pizza
```

Note that each frame need a project name which is basically the 
name of what you're tracking.
You can also add as many tags has you want alongside the project name. 
Tags let you define the task a bit more. 
In the example we have the project `cooking` with the tag `pizza`.

When you're reading for another task, you can `start` another frame. 
You can also `stop` the current frame with the `stop`.

You can see the `status` of the current frame with the `status` command.

You can also list of all the frames within a time period with the `log` command.

For more details about all the available commands and options run `watson help` or visit the [Commands page](https://github.com/nomis51/watson/blob/dev/docs/commands.md).
