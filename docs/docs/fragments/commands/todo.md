‎

## todo

```shell
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

```shell
watson todo add "Cook that tasty pizza I saw on YouTube" cooking pizza --due "2026-01-01 12:00" --priority 1
watson todo remove 02af5e
watson todo list
watson todo edit 02af5e "Cook that crazy pizza I saw on YouTube" cooking pizza --due "2026-01-01 12:45" --priority 2
watson todo complete 02af5e
waton todo reset 02af5e
```
