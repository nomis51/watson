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
