# Getting started:

### 1. Install from NuGet:

You can get the library via [NuGet](http://www.nuget.org) if you have the extension installed for Visual Studio or via the PowerShell package manager. NuGet package [SimpleObjectCompareDotNet](https://www.nuget.org/packages/SimpleObjectCompareDotNet).

<table>
<tr><td>
            <code>PM&gt; Install-Package SimpleObjectCompareDotNet</code>
</td></tr></table>

### 2. Use Comparer:

```cs
 using SimpleObjectCompareDotNet;

var instance1 = new Simple { Test = "1" };
var instance2 = new Simple { Test = "2" };

var result = ObjectComparer.ComparePublicMembers(instance1, instance2);
 
```

### 3. Use ObjectMembersCollector:

```cs
 using SimpleObjectCompareDotNet;

 var instance = new Simple { Test = "1" };

var result = ObjectMembersCollector.Collect(instance); 
```

### 4. [Browse the API Reference](../api/index.md)

### 5. Explore the tests on GitHub:

 - [Tests](https://github.com/stiankroknes/SimpleObjectCompareDotNet/tree/main/tests) 
