using System;

namespace Implementation.Behavioral.Mediator
{
	/// <summary>
	/// Конкретный посредник: знает всех коллег и все правила их взаимодействия.
	///
	/// Switch внутри Notify — не недостаток реализации, а суть паттерна:
	/// схема взаимодействия собрана в одном месте и читается целиком,
	/// не открывая файлы компонентов.
	/// </summary>
	public class LogHubMediator : ILogHubMediator
	{
		private readonly LogImporter _importer;
		private readonly Pipeline _pipeline;
		private readonly Dashboard _dashboard;

		public LogHubMediator(LogImporter importer, Pipeline pipeline, Dashboard dashboard)
		{
			_importer = importer ?? throw new ArgumentNullException(nameof(importer));
			_pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
			_dashboard = dashboard ?? throw new ArgumentNullException(nameof(dashboard));

			_importer.SetMediator(this);
			_pipeline.SetMediator(this);
			_dashboard.SetMediator(this);
		}

		public void Notify(object sender, string @event)
		{
			switch (@event)
			{
				case "imported":
					// Одно событие — две реакции. Импортёр про это не знает.
					_pipeline.Process(_importer.Buffer);
					_dashboard.ShowImported(_importer.Buffer.Count);
					break;

				case "overloaded":
					// Обратная связь: конвейер притормаживает импортёр,
					// хотя ссылки на него у конвейера нет.
					_importer.Pause();
					_dashboard.ShowWarning("конвейер перегружен, импорт приостановлен");
					break;

				default:
					Console.WriteLine($"[Посредник] неизвестное событие '{@event}' от {sender.GetType().Name}");
					break;
			}
		}
	}

	public class MediatorApp
	{
		public static void Run()
		{
			var importer = new LogImporter();
			var pipeline = new Pipeline();
			var dashboard = new Dashboard();

			// Единственное место, где компоненты встречаются друг с другом.
			var mediator = new LogHubMediator(importer, pipeline, dashboard);

			Console.WriteLine("--- Небольшая порция: обрабатывается штатно");
			importer.Import(3);

			Console.WriteLine();
			Console.WriteLine("--- Большая порция: конвейер сообщает о перегрузке,");
			Console.WriteLine("    посредник останавливает импортёр");
			importer.Import(8);

			Console.WriteLine();
			Console.WriteLine("--- Следующая попытка импорта уже не пройдёт");
			importer.Import(3);

			Console.WriteLine();
			importer.Resume();
			importer.Import(2);

			Console.WriteLine();
			Console.WriteLine($"Посредник: {mediator.GetType().Name}. " +
			                  "Компоненты не имеют ни одной ссылки друг на друга.");
		}
	}
}
