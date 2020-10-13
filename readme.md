# Motivation

Why invent a new serialization system? My problems with JSON and XML are:

* Data tend to be bulky when viewed in text editors
* No pointer support
* Duplication of information
* Not self-transformative
* Expression trees are enweildly
* No support for predefined constants (ex. Pi)
* Only support one root element or dataset per file

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
            "country": "Greece"
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

# Multiple datasets in the same document

By convention, the root object being serialized/deserialized is referred to as `root`, ex.:

```C#
root: "Hello World";
```

Sometimes, you want to represent multiple datasets in the same file. For example, let's assume you want to be able to get Aristotles and Plato seperately. Then you could code it like this:

```C#
plato: Person<Name, Born, Location>("Plato", -423, athens);
aristotle: Person<Name, Born, Location>("Aristotle", -384, stagira);

athens => Location<City, Country>("Athens", "Greece");
stagira => Location<City, Country>("Stagira", "Greece");
```

It is now possible to get the datasets seperately:

```C#
var plato = serializer.Deserialize<Person>(INPUT_DESERIALIZE, "plato");
var aristotle = serializer.Deserialize<Person>(INPUT_DESERIALIZE, "aristotle");
```

# Custom term names and inheritance

Sometimes you need to deserialize classes with inheritance. In this case you must create explicit mappings to specify which term maps to which subclass. You could, for example, have these class definitions:

```C#
public interface IShape { }
public class Circle : IShape { public double Radius { get; set; } }
public class Square : IShape { public double Width { get; set; } }
```

The `NameAndTypeMappings` class is used to set up mappings to subtypes:

```C#
var nameMappings = new NameAndTypeMappings();
nameMappings.MapTermNameToType<Circle>("circle");
nameMappings.MapTermNameToType<Square>("square");
```

These mappings can then be used with deserialization:

```C#
var serializer = new StringSerializer(nameAndTypeMappings: nameMappings);
IShape circle = serializer.Deserialize<IShape>("root: circle<Radius>(10);");
IShape square = serializer.Deserialize<IShape>("root: square<Width>(10);");
```

# Named constants

Sometimes it is convenient to use named constants in instead of values. For example, let's say that you want to define PI (3.14...). This can be done with the `NameAndTypeMappings` class, ex.:

```C#
var nameMappings = new NameAndTypeMappings();
var serializer = new StringSerializer(nameAndTypeMappings: nameMappings);
nameMappings.MapIdentifierNameToConstant("Pi", Math.PI);

var output = serializer.Serialize(Math.PI);
Console.WriteLine(output);
```

Output:

```C#
root: Pi;
```

# Term serialization and deserialization using constructors and deconstructors

Sometimes you need to create objects by invoking their constructors. In this scenario, you need to not specify a class member mapping list. For example, you may want to represent a .NET `DateTime` object like this:

```C#
root: datetime(2020,10,13);
```

Constructors will automatically be invoked when no class member mappings are given. This code can be used for the deserialization:

```C#
string INPUT_SERIALIZE = "root: datetime(2020,10,13);";
var output = serializer.Deserialize<DateTime>(INPUT_SERIALIZE);
```

Serialization is the opposite of this. During serialization we need something that is like a constructor for classes, but that gives you the constructor parameters as outputs. A neat new feature that fits this description very well is _deconstructors_, which were introduced in C# 7. Deconstructors can be written in classes or in extension methods. This allows us to create a deconstructor for the .NET build-in `DateTime` type:

```C#
public static class DateTimeExtensions
{
    public static void Deconstruct(this DateTime dateTime, out int year, out int month, out int day)
    {
        year = dateTime.Year;
        month = dateTime.Month;
        day = dateTime.Day;
    }
}
```

.NET deconstructors must have at least 2 _out_ arguments. In _Trl.Serialization_ decconstructors with 1 or 0 _out_ parameters are also supported. Serialization code for making use of the `DateTimeExtensions` class looks like this:

```C#
var nameMappings = new NameAndTypeMappings();
var serializer = new StringSerializer(nameAndTypeMappings: nameMappings);
nameMappings.MapExtensionMethodDestructorsFromType(typeof(DateTimeExtensions));
nameMappings.MapTermNameToType<DateTime>("datetime");
DateTime INPUT_DESERIALIZE = new DateTime(2020, 7, 8);
var outputSerialized = serializer.Serialize(INPUT_DESERIALIZE);
```

# Building expression trees

With all of the above features in place, it is possible to create expression trees. A full example of how this might work is given in the sample console app in this repository.

The first step would be to define a base class or interface for expressions. In the sample app, this is done in the `BinaryOperator` class. After this, the child classes used to represent expression tree operators need to be registered, ex.:

```C#
var nameMappings = new NameAndTypeMappings();
var serializer = new StringSerializer(nameAndTypeMappings: nameMappings);
nameMappings.MapTermNameToType<Add>("add");
nameMappings.MapTermNameToType<Sub>("sub");
nameMappings.MapTermNameToType<Mul>("mul");
nameMappings.MapTermNameToType<Div>("div");
```

In this code sample, the `Add`, `Sub`, `Mul`, and `Div` subclasses inherits from `BinaryOperator`. Each of this example, the subclasses also implement an interface called `IExpression` defining the `Calculate` method.

All of this could be used for deserialization and executing the expression tree:

```C#
string INPUT_DESERIALIZE = "root: div(mul(4,sub(add(3,2),1)),5);";
IExpression expr = serializer.Deserialize<IExpression>(INPUT_DESERIALIZE);
Console.WriteLine($"Result = {expr.Calculate()}");
```

# Installation via Nuget

See [https://www.nuget.org/packages/Trl.Serialization/](https://www.nuget.org/packages/Trl.Serialization/) for nuget package.

# Unit Test Code Coverage

<!--**Note October 2020 - C# .NET Core Unit test coverage tools currently do not support switch expressions. This seems to be a general problem with a new C# language feature.**-->

Unit tests can be run using the `.\test.ps1` script. This will generate a code coverage report in the `.\UnitTestCoverageReport` folder using [Coverlet](https://github.com/tonerdo/coverlethttps://github.com/tonerdo/coverlet) and [ReportGenerator](https://github.com/danielpalme/ReportGenerator).

![Code Coverage](code_coverage.PNG)

# Licence

Trl.Serialization is released under the MIT open source licence. See LICENCE.txt in this repository for the full text.
