using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Implementation.Creational.Singleton;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Implementation.Tests.Singleton
{
	/// <summary>
	/// Тесты правильной реализации: единственность обеспечивает Lazy&lt;T&gt;,
	/// но конструктор открыт, поэтому каждый тест работает со своим экземпляром
	/// и не зависит от соседей.
	/// </summary>
	[TestClass]
	public class LazyConfigServiceTest
	{
		private LazyConfigService _target;

		[TestInitialize]
		public void Init()
		{
			// Свой экземпляр на каждый тест — состояние не переносится.
			_target = new LazyConfigService();
		}

		[TestMethod]
		public void SetValue_ЗатемGetValue_ВозвращаетЗаписанноеЗначение()
		{
			_target.SetValue("key", "value");

			Assert.AreEqual("value", _target.GetValue("key"));
		}

		[TestMethod]
		public void GetValue_НесуществующийКлюч_БросаетKeyNotFound()
		{
			Assert.ThrowsException<KeyNotFoundException>(() => _target.GetValue("отсутствует"));
		}

		[TestMethod]
		public void GetValue_Null_БросаетArgumentNull()
		{
			Assert.ThrowsException<ArgumentNullException>(() => _target.GetValue(null));
		}

		[TestMethod]
		public void Instance_ВозвращаетОдинИТотЖеЭкземпляр()
		{
			Assert.AreSame(LazyConfigService.Instance, LazyConfigService.Instance);
		}

		[TestMethod]
		public void Instance_ПриПараллельномОбращении_СоздаётсяРовноОдинЭкземпляр()
		{
			var instances = new LazyConfigService[64];

			Parallel.For(0, instances.Length, i => instances[i] = LazyConfigService.Instance);

			Assert.AreEqual(1, instances.Distinct().Count());
		}

		[TestMethod]
		public void SetValue_ИзМногихПотоков_НеТеряетЗаписи()
		{
			Parallel.For(0, 1000, i => _target.SetValue($"key{i}", i.ToString()));

			for (var i = 0; i < 1000; i++)
			{
				Assert.AreEqual(i.ToString(), _target.GetValue($"key{i}"));
			}
		}
	}
}
