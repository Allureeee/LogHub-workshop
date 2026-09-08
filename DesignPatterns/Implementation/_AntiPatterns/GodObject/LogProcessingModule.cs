using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Implementation.AntiPatterns.GodObject
{
	// =====================================================================
// АНТИПРИМЕР. Входные данные к части B лабораторной работы 1.
//
// Класс LogProcessingModule делает всё сразу: читает конфигурацию,
// подключается к хранилищу, разбирает строки, фильтрует их, форматирует,
// решает, куда отправить, и записывает.
//
// Ваша задача — разделить его на слои так, чтобы добавление нового
// приёмника не требовало правки существующих файлов, а разбор строки
// можно было протестировать без файловой системы.
//
// НЕ КОПИРУЙТЕ этот код в решение. Он здесь, чтобы его переписать.
//
// Посмотреть исходное поведение:
//     dotnet run --project Implementation -- god-object
//
// Задание: Практикум/ЛР1_Git_среда_и_SOLID.md
// Теория:  Textbook/SOLID.md
//
// Комментарии внутри подсказывают, где искать нарушения, но список
// неполный: часть нарушений не помечена никак.
// =====================================================================

public class LogProcessingModule
	{
		// Зависимость от конкретных типов и настроек прямо в поле.
		// Подменить их в тесте нельзя.
		private readonly string _configPath = "loghub.config";
		private readonly Dictionary<string, string> _config = new Dictionary<string, string>();

		// Модуль обработки логов сам управляет соединением с хранилищем.
		private FakeDbConnection _connection;

		// И сам держит файловый поток.
		private StreamWriter _fileWriter;

		private int _written;
		private int _skipped;

		// -----------------------------------------------------------------
		// Чтение конфигурации. Причина изменения №1:
		// поменялся формат конфигурационного файла.
		// -----------------------------------------------------------------
		private void LoadConfig()
		{
			// Прямое обращение к файловой системе из бизнес-логики.
			if (!File.Exists(_configPath))
			{
				// Значения по умолчанию зашиты в код.
				_config["sink"] = "console";
				_config["minSeverity"] = "Info";
				_config["format"] = "plain";
				return;
			}

			foreach (var line in File.ReadAllLines(_configPath))
			{
				if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
				{
					continue;
				}

				var parts = line.Split('=', 2);
				if (parts.Length == 2)
				{
					_config[parts[0].Trim()] = parts[1].Trim();
				}
			}
		}

		// -----------------------------------------------------------------
		// Открытие соединений. Причина изменения №2:
		// сменилось хранилище или способ подключения.
		// -----------------------------------------------------------------
		private void OpenSinks()
		{
			// Switch по типу приёмника — первый из трёх.
			// Новый приёмник = правка этого метода.
			switch (_config["sink"])
			{
				case "database":
					_connection = new FakeDbConnection(_config.GetValueOrDefault("connection", "local"));
					_connection.Open();
					break;

				case "file":
					// Имя файла вычисляется здесь же.
					_fileWriter = new StreamWriter($"logs-{DateTime.Now:yyyyMMdd}.txt", append: true);
					break;

				case "console":
					break;

				default:
					// Неизвестный приёмник роняет весь модуль на этапе открытия.
					throw new NotSupportedException($"Приёмник '{_config["sink"]}' не поддерживается");
			}
		}

		// -----------------------------------------------------------------
		// Разбор строки. Причина изменения №3:
		// поменялся формат логов.
		// -----------------------------------------------------------------
		private LogRecord ParseLine(string line)
		{
			// Формат зашит намертво: «дата|уровень|сообщение».
			// Поддержать CSV или JSON без правки этого метода нельзя.
			var parts = line.Split('|', 3);

			if (parts.Length != 3)
			{
				// Молча возвращает null. Вызывающий код обязан это помнить,
				// но из сигнатуры метода такого не следует.
				return null;
			}

			if (!DateTime.TryParse(parts[0], CultureInfo.InvariantCulture,
				    DateTimeStyles.None, out var date))
			{
				return null;
			}

			return new LogRecord
			{
				Date = date,
				Severity = parts[1].Trim(),
				Message = parts[2].Trim()
			};
		}

		// -----------------------------------------------------------------
		// Форматирование. Причина изменения №4:
		// понадобился новый формат вывода.
		// -----------------------------------------------------------------
		private string Format(LogRecord record)
		{
			// Switch по формату — второй.
			switch (_config["format"])
			{
				case "plain":
					return $"{record.Date:yyyy-MM-dd HH:mm:ss} [{record.Severity}] {record.Message}";

				case "json":
					return "{\"date\":\"" + record.Date.ToString("O") + "\"," +
					       "\"level\":\"" + record.Severity.ToLowerInvariant() + "\"," +
					       "\"message\":\"" + record.Message.Replace("\"", "\\\"") + "\"}";

				case "csv":
					return $"{record.Date:O};{record.Severity};{record.Message}";

				default:
					return record.Message;
			}
		}

		// -----------------------------------------------------------------
		// Маршрутизация и запись. Причина изменения №5:
		// добавился приёмник.
		// -----------------------------------------------------------------
		private void Write(LogRecord record)
		{
			var text = Format(record);

			// Switch по приёмнику — третий. Их уже три, и все надо
			// править синхронно при добавлении одного приёмника.
			switch (_config["sink"])
			{
				case "database":
					_connection.Execute(
						"INSERT INTO logs(date, severity, message) VALUES(@d, @s, @m)",
						record.Date, record.Severity, text);
					break;

				case "file":
					_fileWriter.WriteLine(text);
					// Сброс буфера на каждую запись — решение о
					// производительности принимается здесь же.
					_fileWriter.Flush();
					break;

				case "console":
					// И раскраска консоли тоже здесь.
					var previous = Console.ForegroundColor;
					Console.ForegroundColor = record.Severity == "Error" || record.Severity == "Critical"
						? ConsoleColor.Red
						: ConsoleColor.Gray;
					Console.WriteLine(text);
					Console.ForegroundColor = previous;
					break;
			}

			_written++;
		}

		// -----------------------------------------------------------------
		// Точка входа модуля.
		// -----------------------------------------------------------------
		public void Run(string logPath)
		{
			LoadConfig();
			OpenSinks();

			var minSeverity = SeverityToInt(_config["minSeverity"]);

			// Снова прямое чтение с диска. В тесте это означает
			// настоящий файл на настоящем диске.
			foreach (var line in File.ReadLines(logPath))
			{
				var record = ParseLine(line);

				// Проверка на null, о необходимости которой
				// не сказано нигде, кроме этой строки.
				if (record == null)
				{
					_skipped++;
					continue;
				}

				// Фильтрация. Причина изменения №6.
				if (SeverityToInt(record.Severity) < minSeverity)
				{
					_skipped++;
					continue;
				}

				Write(record);
			}

			// Освобождение ресурсов вручную, без using и без try/finally:
			//  исключение в середине цикла оставит файл открытым.
			_fileWriter?.Dispose();
			_connection?.Close();

			Console.WriteLine($"Записано: {_written}, пропущено: {_skipped}");
		}

		// Шкала уровней зашита в метод. Новый уровень — правка здесь.
		private static int SeverityToInt(string severity)
		{
			switch (severity)
			{
				case "Debug": return 0;
				case "Info": return 1;
				case "Warning": return 2;
				case "Error": return 3;
				case "Critical": return 4;
				default: return 1;
			}
		}
	}

	// «Универсальный» интерфейс приёмника: реализовать его целиком
	// не может ни один приёмник. Консоль не умеет транзакций,
	// база не умеет ротации файлов.
	// В отчёте по ЛР 1 этот интерфейс нужно разделить.
	public interface ILogSink
	{
		void Write(string text);
		void Flush();
		void BeginTransaction();
		void Commit();
		void Rollback();
		void Rotate();
		long GetFileSize();
		void SetConsoleColor(ConsoleColor color);
	}

	public class LogRecord
	{
		public DateTime Date { get; set; }
		public string Severity { get; set; }
		public string Message { get; set; }
	}

	/// <summary>
	/// Заглушка вместо настоящего клиента БД, чтобы пример собирался
	/// без внешних пакетов. В задании считайте её недоступной для изменения.
	/// </summary>
	internal class FakeDbConnection
	{
		private readonly string _connectionString;
		private bool _open;

		public FakeDbConnection(string connectionString) => _connectionString = connectionString;

		public void Open()
		{
			_open = true;
			Console.WriteLine($"[БД] соединение открыто: {_connectionString}");
		}

		public void Execute(string sql, DateTime date, string severity, string text)
		{
			if (!_open)
			{
				throw new InvalidOperationException("Соединение не открыто");
			}

			Console.WriteLine($"[БД] {sql} -> {date:O}, {severity}, {text}");
		}

		public void Close()
		{
			_open = false;
			Console.WriteLine("[БД] соединение закрыто");
		}
	}
}
