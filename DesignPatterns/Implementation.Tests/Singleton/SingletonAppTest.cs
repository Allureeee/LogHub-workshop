using System;
using System.Diagnostics;
using Implementation.Creational.Singleton;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Implementation.Tests.Singleton
{
	/// <summary>
	/// SingletonApp запускает 100 потоков, каждый делает 100 инкрементов счётчика
	/// через IConfigService. Ожидаемый результат — ровно 10 000.
	/// </summary>
	[TestClass]
	public class SingletonAppTest
	{
		private const int ThreadsCount = 100;
		private const int IterationsPerThread = 100;
		private const int ExpectedTotal = ThreadsCount * IterationsPerThread;

		[TestMethod]
		public void Run_НаНастоящемХранилище_ВозвращаетТочноеЧислоИнкрементов()
		{
			// Мок здесь не годится: Run читает то, что сам записал,
			// поэтому нужен стаб с настоящим состоянием.
			var target = new SingletonApp(new ConfigServiceStub());

			var sw = Stopwatch.StartNew();
			var result = target.Run();
			sw.Stop();

			Console.WriteLine($"Время выполнения: {sw.Elapsed}");
			Assert.AreEqual(ExpectedTotal.ToString(), result);
		}

		[TestMethod]
		public void Run_ЧитаетСчётчикРовноПоРазуНаИтерациюПлюсДваРазаВКонце()
		{
			var storage = new ConfigServiceStub();
			var mock = new Mock<IConfigService>();
			mock.Setup(x => x.GetValue(It.IsAny<string>()))
				.Returns<string>(storage.GetValue);
			mock.Setup(x => x.SetValue(It.IsAny<string>(), It.IsAny<string>()))
				.Callback<string, string>(storage.SetValue);

			var target = new SingletonApp(mock.Object);
			target.Run();

			// 10 000 чтений внутри потоков + 2 чтения в конце Run:
			// одно для Console.WriteLine, одно для return.
			mock.Verify(x => x.GetValue("count"), Times.Exactly(ExpectedTotal + 2));

			// 1 запись в конструкторе + по одной на каждый инкремент.
			mock.Verify(x => x.SetValue("count", It.IsAny<string>()), Times.Exactly(ExpectedTotal + 1));
		}

		[TestMethod]
		public void Конструктор_СбрасываетСчётчикВНоль()
		{
			var mock = new Mock<IConfigService>();

			_ = new SingletonApp(mock.Object);

			mock.Verify(x => x.SetValue("count", "0"), Times.Once);
		}
	}
}
