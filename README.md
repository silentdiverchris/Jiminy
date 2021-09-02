# Jiminy
=ctx-prj:Jiminy-docu=

## Overview
A custom to-do system that watches MarkDown files in a set of nominated directories, scans through new and changed ones to gather custom tags from an expandable GTD-esque set, combines them and generates one or more highly configurable static HTML and/or JSON outputs from templates.

## Why
Over the years I've used numerous systems to organise the things I needed to do, such as EverNote, OneNote, BoostNote and a few others, mainly trying to shoehorn in a personalised version of the GTD system into a host that seemed to prefer that I didn't.

While they are all capable, pretty and/or useful in their own ways, I found each to be variously annoying, bloated, awkward, tying me into their way of working, or otherwise not ideal.

I want something that is super-simple to add items to, where I can just dump in a note, reminder or other to-do item that I need to be nudged about, then find them all magically in one place in a nice structure that helps me prioritise and schedule getting on with them.

I also want something that can be backed up easily, and where the backups are easily readable and greppable, and selectively copy-pastable; not an obscure .unreadablebyhumans format.

Since I use MarkDown for making notes anyway, and a lot of my to-dos in other systems just referenced those .md files (or worse, duplicated chunks of them), I figured why not just add the to-do items (I'll just call them 'items' hereafter) directly to the MarkDown in such a way that they can be easily recognised and dug out by software.

So my solution is to go back to good old text files in directories for my to-do system, just like my old method from 20 years ago of having a 'todo.txt' in each folder, but now with MarkDown as the glue that holds everything together and a system that gathers all the items in one place, reminds me about things and makes it easy to work out what's important while keeping a complete set of everything I want to get done during my next two or three lifetimes.

This means I can keep using my favourite Markdown editor (which is the splendid [GhostWriter](https://wereturtle.github.io/ghostwriter/), but any will do) to make notes and draft documents but when I need to remind myself of something, I just add something like;

```
 =b:n-p:2-prj:ABC-enh-pho= Call Fred about making it do XYZ in a better way
```

..on a line on it's own and continue typing without breaking my stride to open up EverNote, click a button to create a new note, find it now asks me what type of note, oh, that's new, no I don't want a task thanks, hit new note again, type in the title, tab and add the text, then add 5 different tags to make it a priority 2 'next' enhancement in the ABC project that involves a phone call, remember yet again that there isn't a save button to press, then switch back to the document I'm writing and try to remember what I was planning to write next.

This way I have no context switching, I just keep typing and Jiminy tells me what I need to remember later.

## Tags overview
Tags are the way you tell Jiminy everything about an item. There will be a full explanation of each standard tag and how to add custom ones somewhere below this section but as a brief introduction to how it works, take the example from above, namely;

``` 
 =b:n-p:2-enh-pho= Call Fred about making it do XYZ in a better way
```

Tags start and end with a selected string, by default both are '=' as they seem unused by Markdown but they could be any characters or strings; say '>=>' and '<!=' or 'tagstart' and 'tagend' if you prefer.

Tags are separated by hyphens and parameters are delimited by colons but again, any strings can be defined instead. I chose these because they easily typed without pressing shift and visually separate the tags nicely.

 This tag set is interpreted as below;

|Tag|Short for|Meaning|
|-|-|-|
|b:n|bucket:next|This item goes into the 'Next' GTD bucket; aka 'GTD list', but I prefer bucket, you can cystomise the settns to make them called lists if you like.|
|p:2|priority:2|Priority 2 item, this could have been p:med, pri:medium etc. Priorities can be indicated by number or name, and you can redefine the priority list as you like.|
|enh|enhancement|Custom tag to indicate an enhancement, you can have as many custom tags as you like.|
|pho|phone|Custom tag to indicate this item needs a phone call to get done, you can have it make lists of specific tags so for example could have a list of all the phone calls you need to make, or all items that require you to be in a specific place to do, essentially the GTD context concept.|

This tag set results in a display in the output HTML along the lines of;
![Call Fred](./Screenshots/CallFredExample.png)

Tags must begin as character 1 on the line, the system only looks for them starting there; at present anyway. The examples above have a sneaky space in front of them to stop them messing up my Jiminy outputs with reminders to call Fred. That preceding space makes them be ignored.

## Contexts
To avoid having to add the project to each item, it's best to set that as a context, so adding;

```
 =p:ABC-setctx= 
```

..earlier in the file tells it to set the context that all subsequent items are for project ABC, so for the rest of the document I don't need to tell it which project items are for, but I could override the context for a specific item or clear the context with 'clear' or 'xctx'.

You can set any tag as a context, so;

```
 =ctx-prj:ABC-b:wait-enh-pri:low-rem:3/nov-due:10/dec=
```

..would set every subsequent item for project ABC, bucket 'Waiting', priority 'low', mark it as an enhancement and set a reminder for the 3rd of November and a due date of 10th December.

You can use the full tag name, eg 'bucket:\[bucket name]' or any synonym that you set up, I have a synonym of 'b' for bucket. Similarly, the 'Project' tag has a synonym of 'prj' but you can easily have 'p' as the synonym for project instead.

This document has one at the beginning to set the context for items I want to remind me about things to do with it. No preceding space as that's the context I want anyway.

```
=ctx-prj:Jiminy-docu=
```

So all items I add in here are put in my 'Jiminy' project, and marked as relating to documentation.

## Source files
By default it only looks at '*.md' files but you can tell it to look in any files you like, according to the settings fragment below;

```
"MonitoredDirectories": [
    {
      "Recursive": true,
      "IncludeFileSpecification": "*.md",
      "IsActive": true,
      "Path": "C:\\Personal"
    },
    {
      "Recursive": true,
      "IncludeFileSpecification": "*.md",
      "IsActive": false,
      "Path": "C:\\Development"
    }
  ],
  ...etc...
```

## Output files
You can have any number of HTML and/or JSON output files, filtered on lists of project names or tags. The HTML files use a .HTML template file which you can completely customise with whatever styling you like.

For example you might want one overall HTML for all items and that's it, or have a second output for a specific project, plus another just for bugs and enhancement requests and another for all reminders and items with due dates. 

I'll add more filtering criteria but with existing functionality its easy to just add a new custom tag 'PutInOutputForFred', tag a bunch of items with it and define a new output that only includes items with that tag.

Each output can use either the standard HTML template or a specific template just for that output.

The JSON file outputs aren't configurable other than filtering which items are written, you just get a standard (indented so human-readable) .JSON with all the item and tag information.

I wanted a system where the output is entirely portable and could be viewed or copied anywhere, emailed to somebody, chucked onto a phone etc. so the HTML is completely self-contained in a single file, not dependent on any external files or internet connection. 

The CSS is completely in-line and the icons are SVGs which are read in on startup, included in the file as HTML statements and customised for their base size and colour at output generation time.

At the time of writing, it doesn't even use any JavaScript.

```
"HtmlSettings": {
    "ShowDiagnostics": false,
    "VerboseDiagnostics": false,
    "HtmlTemplateFileName": "C:\\Personal\\Jiminy\\HtmlTemplate.html",
    "Outputs": [
      {
        "IsEnabled": true,
        "Title": "All Items",
        "HtmlPath": "C:\\Personal\\Jiminy\\Output.html",
        "OverrideHtmlTemplateFileName": null,
        "ItemSelection": null
      },
      {
        "IsEnabled": false,
        "Title": "Jiminy Items",
        "HtmlPath": "C:\\Personal\\Jiminy\\Jiminy.html",
        "JsonPath": "C:\\Personal\\Jiminy\\Jiminy.json",
        "OverrideHtmlTemplateFileName": null,
        "ItemSelection": {
          "MustMatchAll": false,
          "IncludeTagNames": [],
          "IncludeProjectNames": [
            "Jiminy"
          ]
        }
      },
```

## Customising the output
The template HTML file mainly consists of two \<style> statements, the second is purely for the tabs so can be altered but with care. 

The first one has all the styling that applies to the item content, and can be tweaked to your heart's content. 

The HTML within the \<body> element  is generated entirely by code and is inserted into the template where it finds '[ContentPlaceholder]'.

The generated HTML uses classes everywhere to allow extra customisation. You can add more HTML around the placeholder to slot the output into a larger document, or whatever.

```
...head stuff...
</head>
<body>

    <!--The placeholder below will be replaced by the content, feel free to add any custom content around it-->
    [ContentPlaceholder]

</body>
</html>
```

If the item data needs to be played with more extensively you can always use the JSON output instead.

## Icons & Colours
Each tag, and some properties within tags can have an icon associated with them. The icons are all configurable or you can dispense with them entirely.

The icons must be SVG files, and are stored in a local directory indicated by the MediaDirectoryPath setting and named in the settings. The reason being that they are read in by the system and added to the output in html format, so the SVG files are not needed to view the output, just when it is generated.

The system also sets the fill colour of the icons according to the properties of the item, so for example in the standard settings, the icon for priority is automatically filled with colour 'orange' as that is what the settings tell it to do, but filled with 'darkgrey' for medium and low priority, see below for partial settings JSON.

The colours are the standard [CSS colour names](https://www.w3schools.com/cssref/css_colors.asp), so any valid CSS colour name is supported, or you can supply a hex value.

```
"TagSettings": {
    ...blah...
    "Defintions": {
      "Items": [
        {
          "GenerateView": true,
          "Synonyms": [
            "p"
          ],
          "Code": "priority",
          "IsCustomTag": false,
          "IsStandardTag": true,
          "Name": "Priority",
          "IconFileName": "priority-medium.svg",
          "Colour": null,
          "DisplayOrder": 3,
          "Description": "The priority of this item"
        },
		...etc...

```
...and...
```
"PrioritySettings": {
    "Defintions": {
      "Items": [
        {
          "Number": 1,
          "Name": "High",
          "IconFileName": "priority-high.svg",
          "Colour": "orange",
          "DisplayOrder": 0,
          "Description": ""
        },
        {
          "Number": 2,
          "Name": "Medium",
          "IconFileName": "priority-medium.svg",
          "Colour": "darkgrey",
          "DisplayOrder": 0,
          "Description": ""
        },
        {
          "Number": 3,
          "Name": "Low",
          "IconFileName": "priority-low.svg",
          "Colour": "darkgrey",
          "DisplayOrder": 0,
          "Description": ""
        }
      ]
    }
  },
```

## Errors & Diagnostics
If you give it a tag or parameter that it doesn't recognise, it will report it right at he top of the HTML output where you can't miss it, so;

```
 =badtag:wrong= Something wrong here
```
..will result in..

![Bad tag](./Screenshots/BadTagError.png)

If you can't work out what you're doing wrong with a tag, turn on diagnostics;
```
"HtmlSettings": {
    "ShowDiagnostics": false,
    "VerboseDiagnostics": false,
    "HtmlTemplateFileName": "C:\\Personal\\Jiminy\\HtmlTemplate.html",
    ..etc..
```
...and you'll get a full report in the output of what it did when interpreting each tag set;

![Diagnostics sample](./Screenshots/DiagnosticsSample.png)

Note that even though the tag on 'Something wrong here' wasn't valid, it still creates an item and warns that it's not tagged properly.

## Console application
Currently it runs from a Windows console, you can see activity and progress messages displayed there. 

At some point I may write a service shell for it, so it runs as a Windows Service.

## Updating source tags from the HTML display
There is no updating of the source tags from the HTML. You must go to the source MarkDown file, change it there and refresh the browser to see the regenerated HTML file. 

Obviously having an HTML page writing to local files is theoretically impossible and philosophically undesirable, and even I were to wangle a hack to get around this, having it write back to the source files is fraught with problems when the files may well be open in a MarkDown editor and just get overwritten etc.

The upshot being there is no "This item is completed", "Move this to the waiting bucket" or "Set a reminder for tomorrow" functionality in the HTML output.

At some point I might put a WPF front end on it, then might revisit that decision, but probably not, there would still be the problem of the changes being overwritten.

Perhaps a JavaScript button could pop up a dialogue where changes were made and the the changed tag text put the clipboard so it could be pasted into the Markdown. That would save some typing, but at the cost of clicking checkboxes, selecting from dropdowns etc. which can often be slower / more disruptive to the 'flow'.

## Configuration file
Everything is defined in a fairly large appsettings.JSON file in the directory the program runs from.

Initially there is no such file, the first time Jiminy runs it will create a default, template version of it and then fail spectacularly because it has no idea what your directories are called. You can then customise the settings as you wish.

If you ever need to regenerate the default template appsettings.JSON, delete or rename the existing one and restart Jiminy.

## Synchronising between machines
One of the benefits of EverNote, OneNote etc is the way they synchronise between PC, Laptop, phone etc. 

The thing is though, I work almost exclusively from a single desktop machine, and occasionally use a laptop when I am dragged kicking and screaming from my home office so that's not really a big issue for me.

Since this is all based on good old text files, it would be easy to use a file synching tool to do the job, or store the files in a DropBox folder or whatever.

## Sample appsettings.json

There's a lot of stuff in here, most of which can happily be left alone but if delved into, allows you to alter a lot of things about how it interprets tags it finds in source files and what output it produces.

The main one to start with is the 'MonitoredDirectories' settings;

A good deal of additional documentation is required here...

=b:eve= Fill out tag customisation section.

```JSON

{
  "LatencySeconds": 10,
  "MediaDirectoryPath": "C:\\Personal\\Jiminy\\Media",
  "TagSettings": {
    "Prefix": "=",
    "Suffix": "=",
    "Seperator": "-",
    "Delimiter": ":",
    "Defintions": {
      "Items": [
        {
          "Type": 9,
          "GenerateView": false,
          "Synonyms": [
            "closed",
            "x"
          ],
          "Code": "completed",
          "IsCustomTag": false,
          "IsStandardTag": true,
          "Name": "Completed",
          "IconFileName": "completed.svg",
          "Colour": null,
          "DisplayOrder": 1,
          "Description": "This item is completed"
        },
        {
          "Type": 10,
          "GenerateView": false,
          "Synonyms": [
            "url"
          ],
          "Code": "link",
          "IsCustomTag": false,
          "IsStandardTag": true,
          "Name": "Link",
          "IconFileName": "link.svg",
          "Colour": "blue",
          "DisplayOrder": 2,
          "Description": "A link to a URL"
        },
        {
          "Type": 5,
          "GenerateView": true,
          "Synonyms": [
            "b"
          ],
          "Code": "bucket",
          "IsCustomTag": false,
          "IsStandardTag": true,
          "Name": "Bucket",
          "IconFileName": "bucket.svg",
          "Colour": null,
          "DisplayOrder": 3,
          "Description": "This item is in a bucket (in, next, waiting, maybe)"
        },
        {
          "Type": 2,
          "GenerateView": true,
          "Synonyms": [
            "p"
          ],
          "Code": "priority",
          "IsCustomTag": false,
          "IsStandardTag": true,
          "Name": "Priority",
          "IconFileName": "priority-medium.svg",
          "Colour": null,
          "DisplayOrder": 3,
          "Description": "The priority of this item"
        },
        {
          "Type": 6,
          "GenerateView": true,
          "Synonyms": [
            "prj"
          ],
          "Code": "project",
          "IsCustomTag": false,
          "IsStandardTag": true,
          "Name": "Project",
          "IconFileName": "project.svg",
          "Colour": "green",
          "DisplayOrder": 3,
          "Description": "This item relates to a project"
        },
        {
          "Type": 4,
          "GenerateView": true,
          "Synonyms": [],
          "Code": "due",
          "IsCustomTag": false,
          "IsStandardTag": true,
          "Name": "Due",
          "IconFileName": "due.svg",
          "Colour": null,
          "DisplayOrder": 4,
          "Description": "There is a due date for this item"
        },
        {
          "Type": 3,
          "GenerateView": true,
          "Synonyms": [
            "r"
          ],
          "Code": "reminder",
          "IsCustomTag": false,
          "IsStandardTag": true,
          "Name": "Reminder",
          "IconFileName": "reminder.svg",
          "Colour": null,
          "DisplayOrder": 5,
          "Description": "There is a reminder for this item"
        },
        {
          "Type": 7,
          "GenerateView": false,
          "Synonyms": [],
          "Code": "repeating",
          "IsCustomTag": false,
          "IsStandardTag": true,
          "Name": "Repeating",
          "IconFileName": "repeating.svg",
          "Colour": null,
          "DisplayOrder": 6,
          "Description": "This item repeats"
        },
        {
          "Type": 8,
          "GenerateView": false,
          "Synonyms": [
            "context",
            "ctx",
            "setctx"
          ],
          "Code": "setcontext",
          "IsCustomTag": false,
          "IsStandardTag": true,
          "Name": "SetContext",
          "IconFileName": null,
          "Colour": null,
          "DisplayOrder": 7,
          "Description": "An abstract property that sets the context of subsequent tags"
        },
        {
          "Type": 11,
          "GenerateView": false,
          "Synonyms": [
            "clear",
            "xctx"
          ],
          "Code": "clearcontext",
          "IsCustomTag": false,
          "IsStandardTag": true,
          "Name": "ClearContext",
          "IconFileName": null,
          "Colour": null,
          "DisplayOrder": 7,
          "Description": "An abstract property that sets the context of subsequent tags"
        },
        {
          "Type": 1,
          "GenerateView": true,
          "Synonyms": [],
          "Code": "bug",
          "IsCustomTag": true,
          "IsStandardTag": false,
          "Name": "Bug",
          "IconFileName": "bug.svg",
          "Colour": null,
          "DisplayOrder": 8,
          "Description": "Bug"
        },
        {
          "Type": 1,
          "GenerateView": true,
          "Synonyms": [],
          "Code": "enhancement",
          "IsCustomTag": true,
          "IsStandardTag": false,
          "Name": "Enhancement",
          "IconFileName": "enhancement.svg",
          "Colour": null,
          "DisplayOrder": 8,
          "Description": "Enhancement"
        },
        {
          "Type": 1,
          "GenerateView": true,
          "Synonyms": [],
          "Code": "conversation",
          "IsCustomTag": true,
          "IsStandardTag": false,
          "Name": "Conversation",
          "IconFileName": "conversation.svg",
          "Colour": null,
          "DisplayOrder": 9,
          "Description": "Talk to somebody"
        },
        {
          "Type": 1,
          "GenerateView": true,
          "Synonyms": [],
          "Code": "phonecall",
          "IsCustomTag": true,
          "IsStandardTag": false,
          "Name": "Phone call",
          "IconFileName": "phone.svg",
          "Colour": null,
          "DisplayOrder": 10,
          "Description": "Phone call required"
        },
        {
          "Type": 1,
          "GenerateView": false,
          "Synonyms": [],
          "Code": "question",
          "IsCustomTag": true,
          "IsStandardTag": false,
          "Name": "Question",
          "IconFileName": "question.svg",
          "Colour": null,
          "DisplayOrder": 10,
          "Description": "Question"
        },
        {
          "Type": 1,
          "GenerateView": false,
          "Synonyms": [],
          "Code": "videocall",
          "IsCustomTag": true,
          "IsStandardTag": false,
          "Name": "Video call",
          "IconFileName": "video-call.svg",
          "Colour": null,
          "DisplayOrder": 10,
          "Description": "Video call"
        }
      ]
    }
  },
  "LogSettings": {
    "VerboseConsole": true,
    "VerboseEventLog": false,
    "LogDirectoryPath": "C:\\Personal\\Jiminy\\Logs",
    "SqlConnectionString": null
  },
  "IgnoreFileSpecifications": [
    "readme.*",
    "README.*",
    "LICENCE.*"
  ],
  "MonitoredDirectories": [
    {
      "Recursive": true,
      "IncludeFileSpecification": "*.md",
      "Exists": true,
      "IsActive": true,
      "Path": "C:\\Personal"
    },
    {
      "Recursive": true,
      "IncludeFileSpecification": "*.md",
      "Exists": true,
      "IsActive": false,
      "Path": "C:\\Dev"
    }
  ],
  "BucketSettings": {
    "Defintions": {
      "Items": [
        {
          "Synonyms": [],
          "Name": "Incoming",
          "IconFileName": "inbox.svg",
          "Colour": "red",
          "DisplayOrder": 1,
          "Description": "The place where new items go when they have no home"
        },
        {
          "Synonyms": [],
          "Name": "Next",
          "IconFileName": "next.svg",
          "Colour": "purple",
          "DisplayOrder": 2,
          "Description": "Items to do next"
        },
        {
          "Synonyms": [],
          "Name": "Soon",
          "IconFileName": "soon.svg",
          "Colour": "blue",
          "DisplayOrder": 3,
          "Description": "Items to do soon"
        },
        {
          "Synonyms": [],
          "Name": "Waiting",
          "IconFileName": "waiting.svg",
          "Colour": "darkgrey",
          "DisplayOrder": 4,
          "Description": "Items that are waiting on other items or something else"
        },
        {
          "Synonyms": [],
          "Name": "Maybe",
          "IconFileName": "maybe.svg",
          "Colour": "green",
          "DisplayOrder": 5,
          "Description": "Items that may or may not end up happening"
        },
        {
          "Synonyms": [],
          "Name": "Eventually",
          "IconFileName": "eventually.svg",
          "Colour": "darkgrey",
          "DisplayOrder": 6,
          "Description": "Items to do eventually"
        }
      ]
    }
  },
  "PrioritySettings": {
    "Defintions": {
      "Items": [
        {
          "Number": 1,
          "Name": "High",
          "IconFileName": "priority-high.svg",
          "Colour": "orange",
          "DisplayOrder": 0,
          "Description": ""
        },
        {
          "Number": 2,
          "Name": "Medium",
          "IconFileName": "priority-medium.svg",
          "Colour": "darkgrey",
          "DisplayOrder": 0,
          "Description": ""
        },
        {
          "Number": 3,
          "Name": "Low",
          "IconFileName": "priority-low.svg",
          "Colour": "darkgrey",
          "DisplayOrder": 0,
          "Description": ""
        }
      ]
    }
  },
  "RepeatSettings": {
    "Defintions": {
      "Items": [
        {
          "NumberOfDays": 1,
          "NumberOfWeeks": 0,
          "NumberOfMonths": 0,
          "NumberOfYears": 0,
          "Name": "Daily",
          "IconFileName": "repeating.svg",
          "Colour": "red",
          "DisplayOrder": 1,
          "Description": "This item repeats daily"
        },
        {
          "NumberOfDays": 0,
          "NumberOfWeeks": 1,
          "NumberOfMonths": 0,
          "NumberOfYears": 0,
          "Name": "Weekly",
          "IconFileName": "repeating.svg",
          "Colour": "green",
          "DisplayOrder": 2,
          "Description": "This item repeats weekly"
        },
        {
          "NumberOfDays": 0,
          "NumberOfWeeks": 0,
          "NumberOfMonths": 1,
          "NumberOfYears": 0,
          "Name": "Monthly",
          "IconFileName": "repeating.svg",
          "Colour": "green",
          "DisplayOrder": 3,
          "Description": "This item repeats monthly"
        },
        {
          "NumberOfDays": 0,
          "NumberOfWeeks": 0,
          "NumberOfMonths": 0,
          "NumberOfYears": 1,
          "Name": "Yearly",
          "IconFileName": "repeating.svg",
          "Colour": "green",
          "DisplayOrder": 4,
          "Description": "This item repeats yearly"
        }
      ]
    }
  },
  "HtmlSettings": {
    "ShowDiagnostics": false,
    "VerboseDiagnostics": false,
    "HtmlTemplateFileName": "C:\\Personal\\Jiminy\\HtmlTemplate.HTML",
    "Outputs": [
      {
        "IsEnabled": true,
        "Title": "All Items",
        "HtmlPath": "C:\\Personal\\Jiminy\\Output.HTML",
        "JsonPath": null,
        "OverrideHtmlTemplateFileName": null,
        "ItemSelection": null
      },
      {
        "IsEnabled": true,
        "Title": "SingLink Items",
        "HtmlPath": "C:\\Personal\\Jiminy\\SingLink.HTML",
        "JsonPath": null,
        "OverrideHtmlTemplateFileName": null,
        "ItemSelection": {
          "MustMatchAll": false,
          "IncludeTagNames": [],
          "IncludeProjectNames": [
            "SingLink"
          ]
        }
      },
      {
        "IsEnabled": true,
        "Title": "Respondent Items",
        "HtmlPath": "C:\\Personal\\Jiminy\\Respondent.HTML",
        "JsonPath": null,
        "OverrideHtmlTemplateFileName": null,
        "ItemSelection": {
          "MustMatchAll": false,
          "IncludeTagNames": [],
          "IncludeProjectNames": [
            "Respondent"
          ]
        }
      },
      {
        "IsEnabled": true,
        "Title": "Bugs and Enhancements",
        "HtmlPath": "C:\\Personal\\Jiminy\\BugsEnhancements.HTML",
        "JsonPath": null,
        "OverrideHtmlTemplateFileName": "C:\\Personal\\Jiminy\\BugsEnhancementsTemplate.HTML",
        "ItemSelection": {
          "MustMatchAll": false,
          "IncludeTagNames": [
            "Bug",
            "Enhancement"
          ],
          "IncludeProjectNames": []
        }
      }
    ]
  }
}

```









## Acknowledgements
The delicious CSS-only tabs were gleefully lifted from https://codeconvey.com/simple-css-tabs-without-javascript/ - most impressive tabs without a hint of javascript or external dependency.

The SVG files that it uses are taken from the Bootstrap icon collection, at https://icons.getbootstrap.com/?#icons.

=b:eve= Move this section up near the start when the structure has settled down.