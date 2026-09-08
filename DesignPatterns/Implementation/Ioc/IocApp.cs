using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Implementation.Ioc
{
	// =====================================================================
	// Материал занятия 3 «IoC и DI-контейнеры» и лабораторной работы 2.
	//
	// Три части:
	//   1. Ручной композиционный корень («DI для бедных») — контейнер не нужен,
	//      чтобы применять внедрение зависимостей.
	//   2. Тот же граф объектов через Microsoft.Extensions.DependencyInjection.
	//   3. Три времени жизни: Transient, Scoped, Singleton — на счётчиках.
	// =====================================================================

	public interface ILogFormatter
	{
		string Format(string severity, string message);
	}

	public interface ILogSink
	{
		void Write(string text);
	}

	public class PlainFormatter : ILogFormatter
	{
		public string Format(string severity, string message) =>
			$"{DateTime.Now:HH:mm:ss} [{severity}] {message}";
	}

	public class JsonFormatter : ILogFormatter
	{
		public string Format(string severity, string message) =>
			"{\"level\":\"" + severity.ToLowerInvariant() + "\",\"message\":\"" + message + "\"}";
	}

	public class ConsoleSink : ILogSink
	{
		public void Write(string text) => Console.WriteLine($"  консоль> {text}");
	}

	public class InMemorySink : ILogSink
	{
		public List<string> Lines { get; } = new List<string>();

		public void Write(string text) => Lines.Add(text);
	}

	/// <summary>
	/// Сервис не создаёт свои зависимости и не знает их конкретных типов.
	/// Ни одного new внутри — это и есть инверсия управления.
	/// </summary>
	public class LogService
	{
		private readonly ILogFormatter _formatter;
		private readonly ILogSink _sink;

		public LogService(ILogFormatter formatter, ILogSink sink)
		{
			_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
			_sink = sink ?? throw new ArgumentNullException(nameof(sink));
		}

		public void Log(string severity, string message) =>
			_sink.Write(_formatter.Format(severity, message));
	}

	// ---------------------------------------------------------------------
	// Часть 3: демонстрация времён жизни
	// ---------------------------------------------------------------------

	public interface ICounter
	{
		int Id { get; }
	}

	public class Counter : ICounter
	{
		private static int _created;

		public Counter() => Id = ++_created;

		public int Id { get; }

		public static void Reset() => _created = 0;
	}

	public class CounterConsumer
	{
		public CounterConsumer(ICounter counter) => Counter = counter;

		public ICounter Counter { get; }
	}

	public static class IocApp
	{
		public static void Run()
		{
			ManualCompositionRoot();
			Console.WriteLine();
			ContainerCompositionRoot();
		}

		// -----------------------------------------------------------------
		// 1. Ручной композиционный корень
		// -----------------------------------------------------------------
		private static void ManualCompositionRoot()
		{
			Console.WriteLine("--- 1. Ручной композиционный корень (без контейнера)");

			// Единственное место в программе, где вызывается new для сервисов.
			var service = new LogService(new PlainFormatter(), new ConsoleSink());
			service.Log("Info", "Приложение запущено");

			// Смена реализации — одна строка, сам LogService не изменился.
			var json = new LogService(new JsonFormatter(), new ConsoleSink());
			json.Log("Error", "Не удалось открыть файл");

			// В тесте подставляется приёмник без побочных эффектов.
			var memory = new InMemorySink();
			new LogService(new PlainFormatter(), memory).Log("Debug", "Тестовая запись");
			Console.WriteLine($"  в память записано строк: {memory.Lines.Count}");

			Console.WriteLine("  Контейнер для внедрения зависимостей не обязателен.");
			Console.WriteLine("  Обязателен один композиционный корень.");
		}

		// -----------------------------------------------------------------
		// 2. Тот же граф через контейнер, два профиля конфигурации
		// -----------------------------------------------------------------
		private static void ContainerCompositionRoot()
		{
			Console.WriteLine("--- 2. Тот же граф через DI-контейнер");

			foreach (var profile in new[] { "console", "json" })
			{
				var services = new ServiceCollection();

				services.AddSingleton<ILogSink, ConsoleSink>();

				if (profile == "json")
				{
					services.AddSingleton<ILogFormatter, JsonFormatter>();
				}
				else
				{
					services.AddSingleton<ILogFormatter, PlainFormatter>();
				}

				services.AddTransient<LogService>();

				using var provider = services.BuildServiceProvider();

				// Единственное обращение к контейнеру во всей программе.
				// Всё остальное получает зависимости через конструктор.
				var service = provider.GetRequiredService<LogService>();

				Console.WriteLine($"  профиль '{profile}':");
				service.Log("Warning", "Место на диске заканчивается");
			}

			Console.WriteLine("  Поведение поменялось без перекомпиляции LogService.");
		}

		// -----------------------------------------------------------------
		// 3. Времена жизни
		// -----------------------------------------------------------------
		public static void RunLifetimes()
		{
			Show("Transient — новый экземпляр на каждое разрешение",
				s => s.AddTransient<ICounter, Counter>());

			Show("Scoped — один экземпляр внутри области видимости",
				s => s.AddScoped<ICounter, Counter>());

			Show("Singleton — один экземпляр на весь контейнер",
				s => s.AddSingleton<ICounter, Counter>());

			Console.WriteLine();
			Console.WriteLine("Правило выбора:");
			Console.WriteLine("  Transient — сервисы без состояния и дешёвые в создании;");
			Console.WriteLine("  Scoped    — то, что живёт ровно один запрос или одну операцию");
			Console.WriteLine("              (соединение с БД, единица работы);");
			Console.WriteLine("  Singleton — конфигурация, кэши, дорогие клиенты.");
			Console.WriteLine();
			Console.WriteLine("Ловушка: singleton, который зависит от scoped-сервиса, «захватывает»");
			Console.WriteLine("его на всё время жизни приложения (captive dependency).");
			Console.WriteLine("BuildServiceProvider с validateScopes: true ловит это на старте.");
		}

		private static void Show(string title, Action<ServiceCollection> register)
		{
			Counter.Reset();

			var services = new ServiceCollection();
			register(services);
			services.AddTransient<CounterConsumer>();

			using var provider = services.BuildServiceProvider(validateScopes: true);

			Console.WriteLine();
			Console.WriteLine($"--- {title}");

			using (var scope1 = provider.CreateScope())
			{
				var a = scope1.ServiceProvider.GetRequiredService<CounterConsumer>();
				var b = scope1.ServiceProvider.GetRequiredService<CounterConsumer>();
				Console.WriteLine($"  область 1: экземпляры #{a.Counter.Id} и #{b.Counter.Id}");
			}

			using (var scope2 = provider.CreateScope())
			{
				var c = scope2.ServiceProvider.GetRequiredService<CounterConsumer>();
				Console.WriteLine($"  область 2: экземпляр  #{c.Counter.Id}");
			}
		}
	}
}
