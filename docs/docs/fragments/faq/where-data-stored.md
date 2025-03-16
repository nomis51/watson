‎

#### Where are the application data stored?

All the data of the application is stored in a folder called `.watson` in your home directory.

- On Windows, it's `C:\Users\<username>\.watson`
- On Linux and macOS, it's `~/.watson`
- On macOS, it's `/Users/<username>/.watson`

The directory contains several elements :

- The `logs` directory contains the logs of the application
- The `settings.json` file contains the settings of the application (which you can also edit through the application)
- The database file `data.db` which contains the frames, projects, tags, etc.
