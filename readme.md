# Motivation

Why invent a new serialization system? My problems with JSON and XML are:

* They are bulky
* They do not support pointers
* They cause duplication of information
* They are not self-transformative
* Expressing computation is enweildly

_Trl.Serialization_ aims to adress these issues and create a compact human readable general-purpose data representation system based on the definition of terms. These _terms_ should be familiar to any programmer because they are basically strings, numbers, and function symbols.

For example, let's say that you want to represent this data (JSON version):

```JSON
[
    {
        "name": "Socrates",
        "born": -470,
        "location":
        {
            "city": "Athens",
            "country": "Attica, Greece"
        }
    },
    {
        "name": "Plato",
        "born": -423,
        "location":
        {
            "city": "Athens",
            "country": "Greece"
        }
    },
    {
        "name": "Aristotle",
        "born": -384,
        "location":
        {
            "city": "Stagira",
            "country": "Greece"
        }
    }
]
```

Representing this information in the Term Rewriting Langauge (TRL) will give:

```C#
root: (p1, p2, p3);

p1 => person<name, born, location>("Socrates", -470, athens);
p2 => person<name, born, location>("Plato", -423, athens);
p3 => person<name, born, location>("Aristotle", -384, stagira);

athens => location<city, country>("Athens", "Greece");
stagira => location<city, country>("Stagira", "Greece");
```

TRL is the language used by _Trl.Serialization_ and is defined in [Trl.TermDataRepresentation](https://github.com/WCoetser/Trl.TermDataRepresentation).

# Simple example: Deserialization

Input:
```C#
root: (p1, p2, p3);

p1 => Person<Name, Born, Location>("Socrates", -470, athens);
p2 => Person<Name, Born, Location>("Plato", -423, athens);
p3 => Person<Name, Born, Location>("Aristotle", -384, stagira);

athens => Location<City, Country>("Athens", "Greece");
stagira => Location<City, Country>("Stagira", "Greece");
```

Sample program showing deserialization:

```C#
StringSerializer serializer = new StringSerializer();
var philosophers = serializer.Deserialize<List<Person>>(INPUT_DESERIALIZE);
foreach (var p in philosophers)
{
    Console.WriteLine($"Name = {p.Name}, Born = {p.Born}, Location = {p.Location.City}, Country = {p.Location.Country}");
}
```

Output:

```
Name = Socrates, Born = -470, Location = Athens, Country = Greece
Name = Plato, Born = -423, Location = Athens, Country = Greece
Name = Aristotle, Born = -384, Location = Stagira, Country = Greece
```

# Simple example: Serialization

Input:

```C#
static Location ATHENS = new Location
{
    City = "Athens",
    Country = "Greece"
};

static Location STAGIRA = new Location
{
    City = "Stagira",
    Country = "Greece"
};

static Person[] INPUT_SERIALIZE = new Person[]
{
    new Person { Name = "Socrates", Born = -470, Location = ATHENS },
    new Person { Name = "Plato", Born = -423, Location = ATHENS },
    new Person { Name = "Aristotle", Born = -384, Location = STAGIRA }
};
```

Sample program showing serialization:

```C#
StringSerializer serializer = new StringSerializer();
var philosophers = serializer.Serialize(INPUT_SERIALIZE, prettyPrint: true);
Console.WriteLine(philosophers);
```

Output:

```C#
root: (Person<Born,Location,Name>(-470,L0,"Socrates"),Person<Born,Location,Name>(-423,L0,"Plato"),Person<Born,Location,Name>(-384,Location<City,Country>("Stagira","Greece"),"Aristotle"));
L0 => Location<City,Country>("Athens","Greece");
```

# Installation via Nuget

See [https://www.nuget.org/packages/Trl.Serialization/](https://www.nuget.org/packages/Trl.Serialization/) for nuget package.

# Unit Test Code Coverage

Unit tests can be run using the `.\test.ps1` script. This will generate a code coverage report in the `.\UnitTestCoverageReport` folder using [Coverlet](https://github.com/tonerdo/coverlethttps://github.com/tonerdo/coverlet) and [ReportGenerator](https://github.com/danielpalme/ReportGenerator).

![Code Coverage](code_coverage.PNG)

# Licence

Trl.Serialization is released under the MIT open source licence. See LICENCE.txt in this repository for the full text.
