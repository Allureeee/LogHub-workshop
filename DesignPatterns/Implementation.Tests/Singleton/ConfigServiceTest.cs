using System;
using Implementation.Creational.Singleton;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Implementation.Tests.Singleton
{
	/// <summary>
	/// Тесты для ConfigService — классического синглтона с приватным конструктором.
	///
	/// Главный вывод этого файла: такой класс невозможно протестировать изолированно.
	/// Экземпляр один на весь тестовый прогон, состояние переносится между тестами,
	/// а сбросить его штатными средствами нельзя — конструктор приватный.
	///
	/// Разбор — Textbook/Порождающие/Singleton.md, раздел
	/// «Singleton против Dependency Injection».
	/// </summary>
	[TestClass]
	public class ConfigServiceTest
	{
		[TestMethod]
		public void SetValue_ЗатемGetValue_ВозвращаетЗаписанноеЗначение()
		{
			var target = ConfigService.Instance();
			var key = Guid.NewGuid().ToString();   // уникальный ключ,
			                                       // чтобы не зависеть от других тестов

			target.SetValue(key, "value");

			Assert.AreEqual("value", target.GetValue(key));
		}

		[TestMethod]
		public void Instance_ВозвращаетОдинИТотЖеЭкземпляр()
		{
			Assert.AreSame(ConfigService.Instance(), ConfigService.Instance());
		}

		[TestMethod]
		public void GetValue_НесуществующийКлюч_БросаетИсключение()
		{
			var target = ConfigService.Instance();

			Assert.ThrowsException<Exception>(() => target.GetValue(Guid.NewGuid().ToString()));
		}

		/// <summary>
		/// АНТИПРИМЕР. Тест намеренно оставлен в проекте и намеренно отключён.
		///
		/// Он проходит, только если непосредственно перед ним отработал тест,
		/// записавший ключ "key" в тот же самый экземпляр. Запустите его в одиночку —
		/// получите «Key not found»; запустите после другого теста — может пройти.
		/// Именно так глобальное состояние превращает набор тестов в лотерею.
		///
		/// Правильное решение — не тестировать синглтон, а внедрять IConfigService
		/// через конструктор: см. NotSingletonConfigServiceTest и LazyConfigServiceTest.
		/// </summary>
		[TestMethod]
		[Ignore("Демонстрация проблемы: тест зависит от порядка выполнения. См. Singleton.md")]
		public void GetValue_ЗависитОтПорядкаВыполненияТестов()
		{
			var target = ConfigService.Instance();

			Assert.AreEqual("value", target.GetValue("key"));
		}
	}
}
