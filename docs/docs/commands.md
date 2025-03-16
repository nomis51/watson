# Commands

!include "fragments/commands/add.md"

## cancel

```
watson cancel
```

Cancel the currently running frame.

## edit

```
watson edit [project] [tag1] [tag2] ... [options]
```

Edit an existing frame<small>*</small>.

*<small>*If frame ID is specified, the currently running frame, if any, is used.</small>*

#### Options

| Option       | Description                  | Example                     |
|--------------|------------------------------|-----------------------------|
| `-f, --from` | The start time of the frame. | `--from "2025-01-01 14:45"` |
| `-i, --id`   | The ID of the frame to edit. | `--id 02af5e`               |

#### Example

```
watson edit watching-tv avengers --id 02af5e --from "2025-01-01 14:45"
```

## log

```
watson log [options]
```

Display the frames within the specified time range and meeting the specified criteria.

#### Options

| Option                 | Description                             | Example                     |
|------------------------|-----------------------------------------|-----------------------------|
| `-f, --from`           | The start time of the frame.            | `--from "2025-01-01 14:45"` |
| `-t, --to`             | The end time of the frame.              | `--to "2025-01-01 14:45"`   |
| `-p, --project`        | Show only frames of those projects.     | `--project cooking`         |
| `-t, --tag`            | Show only the frames having those tags. | `--tag pizza`               |
| `-a, --all`            | Show all frames.                        | `--all`                     |
| `-d, --day`            | Show the frames of today.               | `--day`                     |
| `-w, --week`           | Show the frames of this week.           | `--week`                    |
| `-m, --month`          | Show the frames of this month.          | `--month`                   |
| `-y, --year`           | Show the frames of this year.           | `--year`                    |
| `-r, --reverse`        | Show the frames in reverse order.       | `--reverse`                 |
| `-i, --ignore-project` | Ignore frames of those projects.        | `--ignore-project cooking`  |
| `-g, --ignore-tag`     | Ignore frames having those tags.        | `--ignore-tag pizza`        |

#### Examples

```
watson log -d
watson log --from "2025-01-01 14:45" --to "2025-01-01 14:45"
watson log --from 14:15
watson log -w --project cooking --ignore-tag pizza
```

## remove

```
watson remove [id]
```

Remove the frame with the specified ID.

#### Examples

```
watson remove 02af5e
```

## restart

```
watson restart [id (optional)]
```

Restart the specified frame with the same project and tags. If no ID is specified, the latest stopped frame is
restarted, if any.

### Examples

```
watson restart
watson restart 02af5e
```

## start

```
watson start [project] [tag1] [tag2] ...
```

Start a new frame for the specified project and tag(s).

#### Examples

```
watson start watching-tv
watson start cooking pizza hawaiian
```

## status

```
watson status
```

Display the status of the currently running frame.

#### Examples

```
watson status
```

## stop

```
watson stop [options]
```

Stop the currently running frame.

#### Options

| Option     | Description                           | Example                   |
|------------|---------------------------------------|---------------------------|
| `-a, --at` | Stop the frame at the specified time. | `--at "2025-01-01 14:45"` |

#### Examples

```
watson stop
watson stop --at "2025-01-01 14:45"
```

## config

```
watson config [action] [key] [value]
```

Get or set a settings value.

#### Examples

```
watson config get workTime.endTime
watson config set workTime.endTime 9:15
```

## workhours

```
watson workhours [start|end|reset] [time]
```

Get or set custom work hours for the current day.
If `reset` is passed, the work hours are reset to the default values.

#### Examples

```
watson workhours start 9:15
watson workhours end 17:15
watson workhours reset
```

## stats (Work in progress)

```
watson stats [type] [options]
```

Display various statistics.

Types possible :

- projects

#### Options

| Option                 | Description                             | Example                     |
|------------------------|-----------------------------------------|-----------------------------|
| `-f, --from`           | The start time of the frame.            | `--from "2025-01-01 14:45"` |
| `-t, --to`             | The end time of the frame.              | `--to "2025-01-01 14:45"`   |
| `-p, --project`        | Show only frames of those projects.     | `--project cooking`         |
| `-t, --tag`            | Show only the frames having those tags. | `--tag pizza`               |
| `-a, --all`            | Show all frames.                        | `--all`                     |
| `-d, --day`            | Show the frames of today.               | `--day`                     |
| `-w, --week`           | Show the frames of this week.           | `--week`                    |
| `-m, --month`          | Show the frames of this month.          | `--month`                   |
| `-y, --year`           | Show the frames of this year.           | `--year`                    |
| `-r, --reverse`        | Show the frames in reverse order.       | `--reverse`                 |
| `-i, --ignore-project` | Ignore frames of those projects.        | `--ignore-project cooking`  |
| `-g, --ignore-tag`     | Ignore frames having those tags.        | `--ignore-tag pizza`        |

#### Examples

```
watson stats projects -d
```

## todo

```
watson todo [action] [arguments] [options]
```

Perform actions on todos, such as

- Create a todo
- Remove a todo
- List todos
- Edit a todo
- Complete / uncomplete a todo

#### Actions

| Action                          | Description       |
|---------------------------------|-------------------|
| add                             | Create a todo     |
| remove                          | Remove a todo     |
| list                            | List todos        |
| edit                            | Edit a todo       |
| complete, done                  | Complete a todo   |
| uncomplete, undone, undo, reset | Uncomplete a todo |

#### Options

| Option           | Description               | Example              |
|------------------|---------------------------|----------------------|
| `-d, --due`      | The due date of the todo. | `--due "2025-01-01"` |
| `-p, --priority` | The priority of the todo. | `--priority 1`       |

#### Examples

```
watson todo add "Cook that tasty pizza I saw on YouTube" cooking pizza --due "2026-01-01 12:00" --priority 1
watson todo remove 02af5e
watson todo list
watson todo edit 02af5e "Cook that crazy pizza I saw on YouTube" cooking pizza --due "2026-01-01 12:45" --priority 2
watson todo complete 02af5e
waton todo reset 02af5e
```

## project

```
watson project [action] [arguments] [options]
```

Perform actions on projects, such as

- Create a project
- Remove a project
- List projects
- Rename a project

#### Actions

| Action | Description      |
|--------|------------------|
| add    | Create a project |
| remove | Remove a project |
| list   | List projects    |
| rename | Rename a project |

#### Examples

```
watson project add cooking
watson project remove 82af5e
watson project list
watson project rename 82af5e eating
```

## tag

```
watson tag [action] [arguments] [options]
```

Perform actions on tags, such as

- Create a tag
- Remove a tag
- List tags
- Rename a tag

#### Actions

| Action | Description  |
|--------|--------------|
| add    | Create a tag |
| remove | Remove a tag |
| list   | List tags    |
| rename | Rename a tag |

#### Examples

```
watson tag add pizza
watson tag remove 02af5e
watson tag list
watson tag rename 02af5e burger
```
