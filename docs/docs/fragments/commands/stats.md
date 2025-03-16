‎

## stats (work in progress)

```shell
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

```shell
watson stats projects -d
```

