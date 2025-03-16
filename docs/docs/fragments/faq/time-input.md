‎

#### How does time input works?

There are several parsing rules available to speed up the time input process, so you don't have to type the date time.
Of course, you can always type the full date time manually if you want to, but here are some shortcuts to save up some
time.

- 1-2 digits means `hour`. For example
    - `3` means `03:00`
    - `13` means `13:00`
    - `88` means `08:08`
- 3-4 digits means `hour:minute`. For example
    - `1234` means `12:34`
    - `845` means `08:45`
    - `138` means `13:08`
- 2 sets of digits means `day hour`. For example
    - `13 14` means `13th` of the current month at `14:00`
    - `3 1245` means `3rd` of the current month at `12:45`
- hyphen separated digits means `year-month-day`. For example
    - `3-13` means `March 13th` of the current year
    - `10-23 852` means `October 23rd` of the current year at `08:52`
    - `2023-4-13 14` means `April 13th` of `2023` at `14:00`
