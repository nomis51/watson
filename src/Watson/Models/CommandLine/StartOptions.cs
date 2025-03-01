﻿using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("start", HelpText = "Add a frame at the current time")]
public class StartOptions
{
    [Value(0, MetaName = "project", Required = true, HelpText = "Project name")]
    public string Project { get; set; } = null!;

    [Value(1, MetaName = "tags", HelpText = "Optional tags", Required = false)]
    public IEnumerable<string> Tags { get; set; } = [];
}