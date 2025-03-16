‎

## edit

```shell
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

```shell
watson edit watching-tv avengers --id 02af5e --from "2025-01-01 14:45"
```
