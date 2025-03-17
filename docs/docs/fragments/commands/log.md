‎

## log

```shell
watson log [options]
```

Display the frames within the specified time range and meeting the specified criteria.
The default behavior of the `log` if you don't specify any options is to show the frames of today.
(`watson log -d 0`)

#### Options

| Option                 | Description                                | Example                     |
|------------------------|--------------------------------------------|-----------------------------|
| `-f, --from`           | The start time of the frame.               | `--from "2025-01-01 14:45"` |
| `-t, --to`             | The end time of the frame.                 | `--to "2025-01-01 14:45"`   |
| `-p, --project`        | Show only frames of those projects.        | `--project cooking`         |
| `-t, --tag`            | Show only the frames having those tags.    | `--tag pizza`               |
| `-a, --all`            | Show all frames.                           | `--all`                     |
| `-d, --day`            | Show the frames of today or specified day. | `--day`, `--day 3`          |
| `-y, --yesterday`      | Show the frames of yesterday.              | `--yesterday`               |
| `-w, --week`           | Show the frames of this week.              | `--week`                    |
| `-m, --month`          | Show the frames of this month.             | `--month`                   |
| `-q, --year`           | Show the frames of this year.              | `--year`                    |
| `-r, --reverse`        | Show the frames in reverse order.          | `--reverse`                 |
| `-i, --ignore-project` | Ignore frames of those projects.           | `--ignore-project cooking`  |
| `-g, --ignore-tag`     | Ignore frames having those tags.           | `--ignore-tag pizza`        |

#### Examples

```shell
watson log -d
watson log -d 3 # show the frames of 3 days ago
watson log --from "2025-01-01 14:45" --to "2025-01-01 14:45"
watson log --from 14:15
watson log -w --project cooking --ignore-tag pizza
```
