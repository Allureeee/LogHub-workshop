using System;
using System.Collections.Generic;
using System.Linq;

namespace Implementation
{

	using Creational_Singleton = global::Implementation.Creational.Singleton;
	using Creational_FactoryMethod = global::Implementation.Creational.FactoryMethod;
	using Creational_AbstractFactory = global::Implementation.Creational.AbstractFactory;
	using Creational_Builder = global::Implementation.Creational.Builder;
	using Creational_Prototype = global::Implementation.Creational.Prototype;
	using Structural_Adapter = global::Implementation.Structural.Adapter;
	using Structural_Composite = global::Implementation.Structural.Composite;
	using Structural_Decorator = global::Implementation.Structural.Decorator;
	using Structural_Facade = global::Implementation.Structural.Facade;
	using Structural_Proxy = global::Implementation.Structural.Proxy;
	using Behavioral_Strategy = global::Implementation.Behavioral.Strategy;
	using Behavioral_TemplateMethod = global::Implementation.Behavioral.TemplateMethod;
	using Behavioral_Iterator = global::Implementation.Behavioral.Iterator;
	using Behavioral_Observer = global::Implementation.Behavioral.Observer;
	using Behavioral_Visitor = global::Implementation.Behavioral.Visitor;
	using Behavioral_ChainOfResponsibility = global::Implementation.Behavioral.ChainOfResponsibility;
	using Behavioral_Mediator = global::Implementation.Behavioral.Mediator;
	using Ioc_ = global::Implementation.Ioc;
	using AntiPatterns_GodObject = global::Implementation.AntiPatterns.GodObject;
	using Concurrency_ReaderWriterLock = global::Implementation.Concurrency.ReaderWriterLock;
	using AntiPatterns_Enumerator = global::Implementation.AntiPatterns.Enumerator;
	/// <summary>
	/// Единая точка входа. Демо выбирается аргументом командной строки,
	/// править Main под каждый паттерн не нужно:
	///
	///     dotnet run --project Implementation -- strategy
	///     dotnet run --project Implementation -- --list
	/// </summary>
	public static class Program
	{
		public static int Main(string[] args)
		{
			if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
			{
				PrintUsage();
				return 0;
			}

			if (args[0] == "--list")
			{
				PrintList();
				return 0;
			}

			var key = args[0].Trim().ToLowerInvariant();

			if (!DemoRegistry.Demos.TryGetValue(key, out var demo))
			{
				Console.Error.WriteLine($"Неизвестное демо: '{key}'.");
				Console.Error.WriteLine("Список доступных: dotnet run --project Implementation -- --list");
				return 1;
			}

			Console.WriteLine($"=== {demo.Title} ===");
			Console.WriteLine($"    {demo.Description}");
			Console.WriteLine();
			demo.Run();
			Console.WriteLine();
			Console.WriteLine($"=== конец демо '{key}' ===");
			return 0;
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Учебные примеры паттернов проектирования.");
			Console.WriteLine();
			Console.WriteLine("Использование:");
			Console.WriteLine("  dotnet run --project Implementation -- <ключ>");
			Console.WriteLine("  dotnet run --project Implementation -- --list");
			Console.WriteLine();
			Console.WriteLine("Пример:");
			Console.WriteLine("  dotnet run --project Implementation -- strategy");
		}

		private static void PrintList()
		{
			Console.WriteLine("Доступные демо:");
			Console.WriteLine();

			foreach (var group in DemoRegistry.Demos
				         .GroupBy(kv => kv.Value.Group)
				         .OrderBy(g => DemoRegistry.GroupOrder.IndexOf(g.Key)))
			{
				Console.WriteLine($"  {group.Key}");

				foreach (var (key, demo) in group.OrderBy(kv => kv.Key))
				{
					Console.WriteLine($"    {key,-24} {demo.Title}");
				}

				Console.WriteLine();
			}

			Console.WriteLine("Учебник по каждому паттерну: ../Textbook/README.md");
		}
	}

	/// <summary>Одно демо: как называется, к чему относится и что запускать.</summary>
	public sealed class Demo
	{
		public Demo(string group, string title, string description, Action run)
		{
			Group = group;
			Title = title;
			Description = description;
			Run = run;
		}

		public string Group { get; }
		public string Title { get; }
		public string Description { get; }
		public Action Run { get; }
	}

	/// <summary>
	/// Реестр демо. Добавили новый пример — дописали одну строку сюда,
	/// Main трогать не нужно.
	/// </summary>
	public static class DemoRegistry
	{
		private const string Creational = "Порождающие";
		private const string Structural = "Структурные";
		private const string Behavioral = "Поведенческие";
		private const string Principles = "Принципы (SOLID, IoC)";
		private const string Other = "Прочее";
		private const string Anti = "Антипримеры (как НЕ надо)";

		public static readonly List<string> GroupOrder =
			new List<string> { Principles, Creational, Structural, Behavioral, Other, Anti };

		public static readonly Dictionary<string, Demo> Demos = new Dictionary<string, Demo>
		{
			// ---------- Принципы ----------
			["ioc"] = new Demo(
				Principles, "IoC и внедрение зависимостей",
				"Ручной композиционный корень и тот же граф через DI-контейнер.",
				Ioc_.IocApp.Run),

			["ioc-lifetimes"] = new Demo(
				Principles, "Времена жизни в DI-контейнере",
				"Transient, Scoped и Singleton на счётчиках экземпляров.",
				Ioc_.IocApp.RunLifetimes),

			["god-object"] = new Demo(
				Anti, "God object: LogProcessingModule",
				"АНТИПРИМЕР и вход к лабораторной 1: класс, который делает всё сразу.",
				GodObjectDemo.Run),

			// ---------- Порождающие ----------
			["singleton"] = new Demo(
				Creational, "Singleton (Одиночка)",
				"1000 потоков инкрементируют счётчик через единственный экземпляр.",
				Creational_Singleton.DemoSingletonApp.Run),

			["factory-method"] = new Demo(
				Creational, "Factory Method (Фабричный метод)",
				"Подкласс застройщика решает, дом какого типа построить.",
				Creational_FactoryMethod.DeveloperFactoryMethod.Run),

			["abstract-factory"] = new Demo(
				Creational, "Abstract Factory (Абстрактная фабрика)",
				"Семейства согласованных объектов: оружие и способ передвижения героя.",
				() => new Creational_AbstractFactory.App().Run()),

			["abstract-factory-canonical"] = new Demo(
				Creational, "Abstract Factory — каноническая схема",
				"Обезличенный вариант из книги GoF: ProductA1/B1 против ProductA2/B2.",
				Creational_AbstractFactory.CanonicalDemo.Run),

			["builder"] = new Demo(
				Creational, "Builder (Строитель)",
				"Директор и конкретный строитель собирают продукт по шагам.",
				Creational_Builder.Client.Run),

			["builder-bread"] = new Demo(
				Creational, "Builder — выпечка хлеба",
				"Один пекарь-директор и три строителя: ржаной, пшеничный, сладкий.",
				Creational_Builder.App.Run),

			["prototype"] = new Demo(
				Creational, "Prototype (Прототип)",
				"Клонирование фигур и реестр прототипов.",
				Creational_Prototype.App.Run),

			// ---------- Структурные ----------
			["adapter"] = new Demo(
				Structural, "Adapter (Адаптер)",
				"Приведение чужого API сохранения логов к интерфейсу ILogSaver.",
				Structural_Adapter.AdapterRunner.Run),

			["composite"] = new Demo(
				Structural, "Composite (Компоновщик)",
				"Дерево из листьев и составных узлов под общим интерфейсом.",
				Structural_Composite.CompositeProgram.Run),

			["composite-employees"] = new Demo(
				Structural, "Composite — оргструктура",
				"Сотрудники и подрядчики как единая древовидная структура.",
				Structural_Composite.CompositeEmployee.Run),

			["decorator"] = new Demo(
				Structural, "Decorator (Декоратор)",
				"Велосипед обрастает пакетами опций; декораторы складываются стопкой.",
				Structural_Decorator.BikeShop.UpgradeBike),

			["facade"] = new Demo(
				Structural, "Facade (Фасад)",
				"Одна точка входа поверх двух подсистем.",
				Structural_Facade.FacadeProgram.Run),

			["proxy"] = new Demo(
				Structural, "Proxy (Заместитель)",
				"Заместитель проверяет доступ и логирует обращения к реальному объекту.",
				Structural_Proxy.ProxyProgram.Run),

			// ---------- Поведенческие ----------
			["strategy"] = new Demo(
				Behavioral, "Strategy (Стратегия)",
				"Подмена алгоритма расчёта премии сотрудника в рантайме.",
				() => new Behavioral_Strategy.EmployeeRunner().Run()),

			["template-method"] = new Demo(
				Behavioral, "Template Method (Шаблонный метод)",
				"Общий алгоритм постройки дома с переопределяемыми шагами.",
				Behavioral_TemplateMethod.DeveloperTemplateMethod.Run),

			["iterator"] = new Demo(
				Behavioral, "Iterator (Итератор)",
				"Свой итератор поверх коллекции и обход через foreach.",
				Behavioral_Iterator.IteratorApp.Run),

			["iterator-enumerable"] = new Demo(
				Behavioral, "Iterator — через yield return",
				"Тот же обход, но реализованный компилятором из yield return.",
				Behavioral_Iterator.IteratorApp.RunEnumerable),

			["observer"] = new Demo(
				Behavioral, "Observer (Наблюдатель)",
				"Подписчики получают уведомления об изменении состояния источника.",
				Behavioral_Observer.ObserverApp.Run),

			["visitor"] = new Demo(
				Behavioral, "Visitor (Посетитель)",
				"Двойная диспетчеризация: новая операция без правки классов записей лога.",
				Behavioral_Visitor.VisitorApp.Run),

			["chain"] = new Demo(
				Behavioral, "Chain of Responsibility (Цепочка обязанностей)",
				"Сообщение идёт по цепочке логгеров, каждый решает — обработать или передать дальше.",
				Behavioral_ChainOfResponsibility.ChainOfResponsibilityDemo.Run),

			["mediator"] = new Demo(
				Behavioral, "Mediator (Посредник)",
				"Импортёр, конвейер и панель общаются только через посредника.",
				Behavioral_Mediator.MediatorApp.Run),

			// ---------- Прочее ----------
			["graph"] = new Demo(
				Other, "Обход графа: DFS и BFS",
				"Материал к лабораторной работе 3: обход в глубину и в ширину.",
				Graphs.GraphApp.Run),

			["rwlock"] = new Demo(
				Other, "ReaderWriterLock",
				"Конкурентный доступ на чтение и запись; к паттернам GoF отношения не имеет.",
				Concurrency_ReaderWriterLock.ReadWriteLockApp.Run),

			// ---------- Антипримеры ----------
			["anti-enumerator"] = new Demo(
				Anti, "Коллекция как собственный итератор",
				"АНТИПРИМЕР: вложенные foreach по одной коллекции ломаются. Разбор — Textbook.",
				AntiPatterns_Enumerator.EnumeratorApp.Run),
		};
	}

	/// <summary>
	/// Готовит временный файл логов и запускает god-класс из _AntiPatterns/GodObject,
	/// чтобы студент увидел исходное поведение до рефакторинга (лабораторная 1, часть B).
	/// </summary>
	internal static class GodObjectDemo
	{
		public static void Run()
		{
			var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "loghub-demo.log");

			System.IO.File.WriteAllLines(path, new[]
			{
				"2026-01-15T10:00:00|Debug|Запуск приложения",
				"2026-01-15T10:00:01|Info|Конфигурация загружена",
				"строка без разделителей — будет пропущена",
				"2026-01-15T10:00:02|Error|Не удалось открыть файл",
				"2026-01-15T10:00:03|Critical|Нет связи с базой данных"
			});

			Console.WriteLine("Исходное поведение до рефакторинга:");
			Console.WriteLine();

			new AntiPatterns.GodObject.LogProcessingModule().Run(path);

			Console.WriteLine();
			Console.WriteLine("Задача ЛР 1 (часть B): разделить этот класс на слои так,");
			Console.WriteLine("чтобы поведение не изменилось, а новый приёмник добавлялся");
			Console.WriteLine("без правки существующих файлов.");

			System.IO.File.Delete(path);
		}
	}
}
