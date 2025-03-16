‎

#### How does a frame start time and end time work?

A frame is actually simply a point in time. It only stores the time at which it was created.
That means that the end time of a frame is technically the start time of "next thing" that comes after it.
This design removes the tedious task of having to perfectly mix and match the start and end time of tasks.

For example, you're working on something, then your boss calls you for something else at 9:13 for 2 minutes,
then you go back on your project, but then a coworker calls you for something 4 minutes later. After all of that,
you realized you forgot to add your tasks in the application. If you'd need to manage end times, that will probably
look like this :

- Add a task "call with the boss" from 9:13 to 9:15
- Add a task "work on project" from 9:15 to 9:20
- Add a task "call with the coworker" from 9:20 to 9:24
- Add a task "work on project" from 9:24 to now

Kind of tedious, isn't it?

Now, what if you only need to remember the start time, it's much easier :

- Add a task "call with the boss" at 9:13
- Add a task "work on project" at 9:15
- Add a task "call with the coworker" at 9:20
- Add a task "work on project" at 9:24

And all frames are gonna adjust automatically, no more "when did it end? how long was the call? etc."!

But, how does it knows when the day ends?
Well, there are settings for that. You can set your work hours in the settings of the application.
By default, it's set to `8:00` to `16:00`.
