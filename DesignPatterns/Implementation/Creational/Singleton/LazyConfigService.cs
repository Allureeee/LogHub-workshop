using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Implementation.Creational.Singleton
{
	/// <summary>
	/// Правильный вариант того же сервиса конфигурации.
	///
	/// Отличия от <see cref="ConfigService"/>:
	///   * единственность обеспечивает <see cref="Lazy{T}"/>, а не двойная проверка
	///     блокировки вручную — переупорядочивание записей невозможно по контракту типа;
	///   * инициализация отложенная: экземпляр создаётся при первом обращении к Value;
	///   * хранилище — ConcurrentDictionary, отдельная блокировка на каждую операцию не нужна;
	///   * есть конструктор для тестов, поэтому класс можно использовать и как обычную
	///     зависимость через DI, не трогая статику.
	///
	/// Разбор — Textbook/Порождающие/Singleton.md, раздел «Singleton против Dependency Injection».
	/// </summary>
	public sealed class LazyConfigService : IConfigService
	{
		private static readonly Lazy<LazyConfigService> Lazy =
			new Lazy<LazyConfigService>(() => new LazyConfigService());

		private readonly ConcurrentDictionary<string, string> _config =
			new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

		/// <summary>
		/// Конструктор публичный намеренно: в приложении класс регистрируется
		/// в DI-контейнере как singleton, а <see cref="Instance"/> остаётся только
		/// для кода, который ещё не переведён на внедрение зависимостей.
		/// </summary>
		public LazyConfigService()
		{
		}

		public static LazyConfigService Instance => Lazy.Value;

		public string GetValue(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (!_config.TryGetValue(key, out var value))
			{
				throw new KeyNotFoundException($"Ключ '{key}' не найден в конфигурации.");
			}

			return value;
		}

		public void SetValue(string key, string value)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			_config[key] = value;
		}
	}
}
