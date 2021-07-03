using FsCheck;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace AD.InMemoryStore.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void Add() =>
            Prop.ForAll<Dictionary<Guid, string>>(values =>
            {
                var sut = new InMemoryStore<Guid, string>();

                var actualValues = Task.WhenAll(values.Select(v => Task.Run(() => sut.Add(v.Key, v.Value)))).Result;

                foreach (var (expected, actual) in values.Zip(actualValues))
                {
                    Assert.AreEqual(expected.Value, actual.Value);
                }
            }).QuickCheckThrowOnFailure();

        [TestMethod]
        [ExpectedException(typeof(DuplicateKeyException<Guid>))]
        public void Cannot_add_a_duplicate_key()
        {
            var key = Guid.NewGuid();
            var sut = new InMemoryStore<Guid, string>();
            sut.Add(key, "A");
            sut.Add(key, "B");
        }

        [TestMethod]
        public void Get() =>
            Prop.ForAll<Dictionary<Guid, string>>(values =>
            {
                var sut = new InMemoryStore<Guid, string>();
                var expectedValues = Task.WhenAll(values.Select(v => Task.Run(() => sut.Add(v.Key, v.Value)))).Result;

                var actualValues = Task.WhenAll(values.Select(v => Task.Run(() => sut.Get(v.Key)))).Result;

                foreach (var (expected, actual) in expectedValues.Zip(actualValues))
                {
                    Assert.AreEqual(expected.Value, actual.Value);
                    Assert.AreEqual(expected.Version, actual.Version);
                }
            }).QuickCheckThrowOnFailure();

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException<Guid>))]
        public void Cannot_get_an_unknown_value()
        {
            var sut = new InMemoryStore<Guid, string>();
            sut.Get(Guid.NewGuid());
        }

        [TestMethod]
        public void GetAll() =>
            Prop.ForAll<Dictionary<Guid, string>>(values =>
            {
                var sut = new InMemoryStore<Guid, string>();
                var expectedValues = Task.WhenAll(values.Select(v => Task.Run(() =>
                {
                    var result = sut.Add(v.Key, v.Value);
                    return (v.Key, result.Value, result.Version);
                }))).Result.OrderBy(_ => _.Key);

                var actualValues = sut.GetAll().OrderBy(_ => _.Key);

                foreach (var (expected, actual) in expectedValues.Zip(actualValues))
                {
                    Assert.AreEqual(expected.Key, actual.Key);
                    Assert.AreEqual(expected.Value, actual.Value);
                    Assert.AreEqual(expected.Version, actual.Version);
                }
            }).QuickCheckThrowOnFailure();
    }
}