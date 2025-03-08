# Commands

## add

```
watson add [project] [tag1] [tag2] ... [options]
```

Add a new frame for the specified project and tag(s) at the specified time<small>*</small>.

*<small>*If no time is specified, the current time is used.*</small>

#### Options

| Option       | Description                  | Example                     |
|--------------|------------------------------|-----------------------------|
| `-f, --from` | The start time of the frame. | `--from "2025-01-01 14:45"` |
| `-t, --to`   | The end time of the frame.   | `--to "2025-01-01 14:45"`   |

#### Example

```
watson add cookin pizza hawaiian --from "2025-01-01 14:45"
```

## cancel

```
watson cancel
```

Cancel the currently running frame.

## create

```
watson create [resource] [name]
```

Create a new *resource* with the specified name. A resource can be a project or a tag.

#### Examples

```
watson create project cooking
watson create tag pizza
```

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

## list

```
watson list [resource]
```

List all items of the specified *resources*. A resource can be a project or a tag.

#### Examples

```
watson list projects
watson list tags
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
watson remove [resource] [id]
```

Remove the specified *resource* with the specified ID. A resource can be a frame, a project or a tag.

#### Examples

```
watson remove frame 02af5e
watson remove project 82af5e
watson remove tag 02af5e
```

## rename

```
watson rename [resource] [id] [new name]
```

Rename the specified *resource* with the specified ID. A resource can a project or a tag.

#### Examples

```
watson rename project 82af5e cooking
watson rename tag 02af5e pizza
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