﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AAAATests : RealmInstanceTest
    {
        protected void MakeThreePeople()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Person
                {
                    FirstName = "John",
                    LastName = "Smith",
                    IsInteresting = true,
                    Email = "john@smith.com",
                    Salary = 67000,
                    Score = -0.9907f,
                    Latitude = 51.508530,
                    Longitude = 0.076132,
                    Birthday = new DateTimeOffset(1959, 3, 13, 0, 0, 0, TimeSpan.Zero),
                    PublicCertificateBytes = new byte[] { 0xca, 0xfe, 0xba, 0xbe },
                    OptionalAddress = "12 Cosgrove St.",
                    IsAmbivalent = true
                });
            });

            _realm.Write(() =>
            {
                _realm.Add(new Person
                {
                    FullName = "John Doe", // uses our setter which splits and maps to First/Lastname
                    IsInteresting = false,
                    Email = "john@doe.com",
                    Salary = 30000,
                    Score = 100.0f,
                    Latitude = 25.708534,
                    Longitude = -73.9748113,
                    Birthday = new DateTimeOffset(1963, 4, 14, 0, 0, 0, TimeSpan.Zero),
                    PublicCertificateBytes = new byte[] { 0xde, 0xad, 0xbe, 0xef },
                    OptionalAddress = string.Empty,
                    IsAmbivalent = false
                });
            });

            _realm.Write(() =>
            {
                _realm.Add(new Person
                {
                    FullName = "Peter Jameson",
                    Email = "peter@jameson.net",
                    Salary = 87000,
                    IsInteresting = true,
                    Score = 42.42f,
                    Latitude = 37.7798657,
                    Longitude = -122.394179,
                    Birthday = new DateTimeOffset(1989, 2, 25, 0, 0, 0, TimeSpan.Zero)
                });
            });
        }

        private static string GetDebugView(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }

            var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
            return propertyInfo.GetValue(exp) as string;
        }

        //[Test]
        //public void SimpleTest()
        //{
        //    var query = _realm.All<Person>().Where(p => p.Score == 100 && p.Longitude == 50)
        //        .OrderBy(p => p.Score);

        //    _ = query.ToArray();
        //}

        //[Test]
        //public void WhereFloatEqualTest()
        //{
        //    MakeThreePeople();
        //    var query = _realm.All<Person>().Where(p => p.Score == 100);

        //    foreach (var person in query)
        //    {
        //        Assert.That(person.FullName, Is.EqualTo("John Doe"));
        //    }

        //    _ = query.ToArray();
        //}

        [Test]
        public void WhereFloatNotEqualTest()
        {
            MakeThreePeople();
            var query = _realm.All<Person>().Where(p => p.Score != 100);

            foreach (var person in query)
            {
                Assert.That(person.FullName, Is.Not.EqualTo("John Doe"));
            }

            _ = query.ToArray();
        }

        [Test]
        public void WhereFloatGtAndLtThanTest()
        {
            MakeThreePeople();
            var query = _realm.All<Person>().Where(p => p.Score > 40
            && p.Score < 48);

            foreach (var person in query)
            {
                Assert.That(person.FullName, Is.EqualTo("Peter Jameson"));
            }

            _ = query.ToArray();
        }

        [Test]
        public void WhereFloatGteAndLteThanTest()
        {
            MakeThreePeople();
            var query = _realm.All<Person>().Where(p => p.Score >= 100.0f
            && p.Score <= 100.0f);

            foreach (var person in query)
            {
                Assert.That(person.FullName, Is.EqualTo("John Doe"));
            }

            _ = query.ToArray();
        }

        //[Test]
        //public void WhereStringEqualityTest()
        //{
        //    MakeThreePeople();
        //    var query = _realm.All<Person>().Where(p => p.LastName == "Doe");

        //    foreach (var person in query)
        //    {
        //        Assert.That(person.FullName, Is.EqualTo("John Doe"));
        //    }

        //    _ = query.ToArray();
        //}

        //[Test]
        //public void WhereNegationTest()
        //{
        //    MakeThreePeople();
        //    var query = _realm.All<Person>().Where(p => !(p.Score == -0.9907f
        //    || p.Score == 100));

        //    foreach (var person in query)
        //    {
        //        Assert.That(person.FullName, Is.EqualTo("Peter Jameson"));
        //    }

        //    _ = query.ToArray();
        //}

        //[Test]
        //public void WhereFloatEqualityReversedOrderTest()
        //{
        //    MakeThreePeople();
        //    var query = _realm.All<Person>().Where(p => 100 == p.Score);

        //    foreach (var person in query)
        //    {
        //        Assert.That(person.FullName, Is.EqualTo("John Doe"));
        //    }

        //    _ = query.ToArray();
        //}

        //[Test]
        //public void WhereBooleanAndTest()
        //{
        //    MakeThreePeople();
        //    var query = _realm.All<Person>().Where(p => p.Score == 100 && p.LastName == "Doe");

        //    foreach (var person in query)
        //    {
        //        Assert.That(person.FullName, Is.EqualTo("John Doe"));
        //    }

        //    _ = query.ToArray();
        //}

        //[Test]
        //public void WhereBooleanOrTest()
        //{
        //    MakeThreePeople();
        //    var query = _realm.All<Person>().Where(p => p.FirstName == "NonExistant"
        //    || p.Score == 42.42f);

        //    foreach (var person in query)
        //    {
        //        Assert.That(person.FullName, Is.EqualTo("Peter Jameson"));
        //    }

        //    _ = query.ToArray();
        //}

        //[Test]
        //public void StringStartsWithTest()
        //{
        //    MakeThreePeople();
        //    var query = _realm.All<Person>().Where(p => p.FirstName.StartsWith("Pet"));

        //    foreach (var person in query)
        //    {
        //        Assert.That(person.FullName, Is.EqualTo("Peter Jameson"));
        //    }
        //}

        //[Test]
        //public void StringEndsWIthTest()
        //{
        //    MakeThreePeople();
        //    var query = _realm.All<Person>().Where(p => p.FirstName.EndsWith("ter"));

        //    foreach (var person in query)
        //    {
        //        Assert.That(person.FullName, Is.EqualTo("Peter Jameson"));
        //    }
        //}

        //[Test]
        //public void StringContainsTest()
        //{
        //    MakeThreePeople();
        //    var query = _realm.All<Person>().Where(p => p.FirstName.Contains("ete"));

        //    foreach (var person in query)
        //    {
        //        Assert.That(person.FullName, Is.EqualTo("Peter Jameson"));
        //    }
        //}

        ////TODO Test has to be implemented with the decided Like syntax
        //[Test]
        //public void StringLikeTest()
        //{
        //    MakeThreePeople();
        //    var query = _realm.All<Person>().Where(p => p.FirstName.Like(""));

        //    foreach (var person in query)
        //    {
        //        Assert.That(person.FullName, Is.EqualTo("Peter Jameson"));
        //    }
        //}

        //[Test]
        //public void SimpleTest()
        //{

        //    var query = _realm.All<Person>().Where(p => p.Score == 100);
        //    foreach (var person in query)
        //    {
        //        var name = person.FullName;
        //        Console.WriteLine(name);
        //    }
        //}

        //[Test]
        //public void Ordering()
        //{
        //    var query = _realm.All<Person>()
        //        .Where(p => p.FirstName.StartsWith("abc") && p.IsInteresting)
        //        .Where(p => p.Birthday < System.DateTimeOffset.UtcNow)
        //        .OrderBy(p => p.FirstName)
        //        .ThenByDescending(p => p.Birthday);

        //    _ = query.ToArray();
        //}

        //[Test]
        //public void DictTest()
        //{
        //    var query = _realm.All<CollectionsObject>().Where(a => a.BooleanDict.Any(kvp => kvp.Key.StartsWith("abc")));
        //    var debugView = GetDebugView(query.Expression);
        //    Console.WriteLine(debugView);
        //    _ = query.ToArray();
        //}

        //[Test]
        //public void ListTest()
        //{
        //    var query = _realm.All<CollectionsObject>().Where(a => a.BooleanList.Count > 5);
        //    _ = query.ToArray();
        //}

        //[Test]
        //public void Iteration()
        //{
        //    _realm.Write(() =>
        //    {
        //        for (var i = 0; i < 10; i++)
        //        {
        //            _realm.Add(new IntPropertyObject
        //            {
        //                Int = i
        //            });
        //        }
        //    });

        //    var query = _realm.All<IntPropertyObject>().Where(a => a.Int > 5);
        //    foreach (var item in query)
        //    {
        //        System.Console.WriteLine(item.Int);
        //    }

        //    for (var i = 0; i < query.Count(); i++)
        //    {
        //        System.Console.WriteLine(query.ElementAt(i).Int);
        //    }
        //}
    }
}
