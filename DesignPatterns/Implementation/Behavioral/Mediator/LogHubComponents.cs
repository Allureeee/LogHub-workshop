using System;
using System.Collections.Generic;
using Implementation.Behavioral.Strategy;

namespace Implementation.Behavioral.Mediator
{
	/// <summary>
	/// Интерфейс посредника. Коллеги знают только его и сообщают о событиях,
	/// не имея ни одной ссылки друг на друга.
	/// </summary>
	public interface ILogHubMediator
	{
		void Notify(object sender, string @event);
	}

	/// <summary>
	/// Базовый коллега: умеет только хранить ссылку на посредника.
	/// </summary>
	public abstract class LogHubComponent
	{
		protected ILogHubMediator Mediator;

		public void SetMediator(ILogHubMediator mediator) => Mediator = mediator;
	}

	/// <summary>
	/// Импортёр читает записи и сообщает, что порция готова.
	/// О конвейере и панели мониторинга не знает ничего.
	/// </summary>
	public class LogImporter : LogHubComponent
	{
		private static int _batch;

		public List<LogEntry> Buffer { get; } = new List<LogEntry>();

		public bool Paused { get; private set; }

		public void Import(int count)
		{
			if (Paused)
			{
				Console.WriteLine("[Импортёр] приостановлен, порция пропущена");
				return;
			}

			_batch++;
			Buffer.Clear();

			for (var i = 0; i < count; i++)
			{
				Buffer.Add(new LogEntry
				{
					Date = DateTime.Now,
					Severity = LogSeverity.Info,
					Message = $"Порция {_batch}, запись {i + 1}"
				});
			}

			Console.WriteLine($"[Импортёр] прочитано записей: {Buffer.Count}");

			// Не «запусти конвейер», а «я закончил». Что делать дальше — решает посредник.
			Mediator.Notify(this, "imported");
		}

		public void Pause()
		{
			Paused = true;
			Console.WriteLine("[Импортёр] импорт приостановлен");
		}

		public void Resume()
		{
			Paused = false;
			Console.WriteLine("[Импортёр] импорт возобновлён");
		}
	}

	/// <summary>
	/// Конвейер обрабатывает порцию и сообщает о перегрузке.
	/// Об импортёре, который эту перегрузку вызвал, он не знает.
	/// </summary>
	public class Pipeline : LogHubComponent
	{
		private const int OverloadThreshold = 5;

		public int Processed { get; private set; }

		public void Process(IReadOnlyList<LogEntry> entries)
		{
			Processed += entries.Count;
			Console.WriteLine($"[Конвейер] обработано записей: {entries.Count} (всего {Processed})");

			if (entries.Count > OverloadThreshold)
			{
				Console.WriteLine($"[Конвейер] порция больше {OverloadThreshold} — перегрузка");
				Mediator.Notify(this, "overloaded");
			}
		}
	}

	/// <summary>
	/// Панель мониторинга только показывает. Ни на что не влияет и ничего не запускает.
	/// </summary>
	public class Dashboard : LogHubComponent
	{
		public void ShowImported(int count) =>
			Console.WriteLine($"[Панель] импортировано: {count}");

		public void ShowWarning(string message) =>
			Console.WriteLine($"[Панель] ВНИМАНИЕ: {message}");
	}
}
