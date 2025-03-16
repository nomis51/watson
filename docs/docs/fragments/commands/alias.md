‎

## alias

```shell
watson alias [action] [arguments]
```

Set or creation an alias for a command.

#### Examples

```shell
watson alias create mystatus status
watson alias mystatus status // shorthand for the above command
watson mystatus
watson alias remove mystatus

watson alias myalias start project1 tag1
watson myalias tag2 --from 8:45
# the above command is equivalent to:
watson start project1 tag1 tag2 --from 8:45
```
