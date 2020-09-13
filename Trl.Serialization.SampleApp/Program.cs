﻿using System;
using System.Collections.Generic;

namespace Trl.Serialization.SampleApp
{
    class Program
    {
        const string INPUT_DESERIALIZE = @"
root: (p1, p2, p3);

p1 => Person<Name, Born, Location>(""Socrates"", -470, athens);
p2 => Person<Name, Born, Location>(""Plato"", -423, athens);
p3 => Person<Name, Born, Location>(""Aristotle"", -384, stagira);

athens => Location<City, Country>(""Athens"", ""Greece"");
stagira => Location<City, Country>(""Stagira"", ""Greece"");
";

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

        public static void Deserialize()
        {
            Console.WriteLine("Deserialize ...");
            StringSerializer serializer = new StringSerializer();
            var philosophers = serializer.Deserialize<List<Person>>(INPUT_DESERIALIZE);
            foreach (var p in philosophers)
            {
                Console.WriteLine($"Name = {p.Name}, Born = {p.Born}, Location = {p.Location.City}, Country = {p.Location.Country}");
            }
            Console.WriteLine();
        }
        
        public static void Serialize()
        {
            Console.WriteLine("Serialize ...");
            StringSerializer serializer = new StringSerializer();
            var philosophers = serializer.Serialize(INPUT_SERIALIZE, prettyPrint: true);
            Console.WriteLine(philosophers);
            Console.WriteLine();
        }

        static void Main()
        {
            Serialize();
            Deserialize();
        }
    }
}
