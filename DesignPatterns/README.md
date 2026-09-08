# Паттерны проектирования — примеры кода

Учебный репозиторий к разделу 1 «Паттерны программирования».

* Теория — [`../Textbook/README.md`](../Textbook/README.md)
* Лабораторные работы — [`../Практикум/README.md`](../Практикум/README.md)
* Здесь — только рабочий код примеров.

## Требования

- .NET SDK 10.0
- Пакеты подтягиваются при `dotnet restore` (Microsoft.Extensions.DependencyInjection, MSTest, Moq, AutoFixture)

Проверить: `dotnet --version`

## Сборка и запуск

```bash
dotnet build
dotnet test
dotnet run --project Implementation -- --list      # список демо
dotnet run --project Implementation -- strategy    # запуск конкретного демо
```

Править `Program.cs`, чтобы посмотреть другой паттерн, не нужно —
демо выбирается аргументом командной строки.

## Структура

```
DesignPatterns/
├── DesignPatterns.sln
├── Directory.Build.props            общий TargetFramework для всех проектов
├── Implementation/
│   ├── Program.cs                   точка входа и реестр демо
│   ├── Ioc/                         IoC, DI и времена жизни (занятие 3)
│   ├── Creational/                  порождающие паттерны
│   ├── Structural/                  структурные паттерны
│   ├── Behavioral/                  поведенческие паттерны
│   ├── Graphs/                      обход графа (к лабораторной 3)
│   ├── Concurrency/                 ReaderWriterLock, к GoF отношения не имеет
│   └── _AntiPatterns/               намеренно неправильный код
│       ├── Enumerator/             коллекция как собственный итератор
│       └── GodObject/              вход к лабораторной 1 для разбора
└── Implementation.Tests/            модульные тесты
```

Пространства имён повторяют структуру папок: `Implementation.Creational.Singleton`,
`Implementation.Behavioral.Strategy` и так далее.

## Принципы (занятия 1–3)

| Материал | Ключ запуска | Код | Учебник |
|---|---|---|---|
| SOLID: god-класс до рефакторинга | `god-object` | `_AntiPatterns/GodObject/` | [SOLID.md](../Textbook/SOLID.md) |
| IoC и внедрение зависимостей | `ioc` | `Ioc/` | [IoC_и_DI.md](../Textbook/IoC_и_DI.md) |
| Времена жизни в DI-контейнере | `ioc-lifetimes` | `Ioc/` | [IoC_и_DI.md](../Textbook/IoC_и_DI.md) |

## Карта паттернов

| Паттерн | Ключ запуска | Код | Учебник |
|---|---|---|---|
| Одиночка | `singleton` | `Creational/Singleton/` | [Singleton.md](../Textbook/Порождающие/Singleton.md) |
| Фабричный метод | `factory-method` | `Creational/FactoryMethod/` | [FactoryMethod.md](../Textbook/Порождающие/FactoryMethod.md) |
| Абстрактная фабрика | `abstract-factory` | `Creational/AbstractFactory/` | [AbstractFactory.md](../Textbook/Порождающие/AbstractFactory.md) |
| Строитель | `builder`, `builder-bread` | `Creational/Builder/` | [Builder.md](../Textbook/Порождающие/Builder.md) |
| Прототип | `prototype` | `Creational/Prototype/` | [Prototype.md](../Textbook/Порождающие/Prototype.md) |
| Адаптер | `adapter` | `Structural/Adapter/` | [Adapter.md](../Textbook/Структурные/Adapter.md) |
| Компоновщик | `composite`, `composite-employees` | `Structural/Composite/` | [Composite.md](../Textbook/Структурные/Composite.md) |
| Декоратор | `decorator` | `Structural/Decorator/` | [Decorator.md](../Textbook/Структурные/Decorator.md) |
| Фасад | `facade` | `Structural/Facade/` | [Facade.md](../Textbook/Структурные/Facade.md) |
| Заместитель | `proxy` | `Structural/Proxy/` | [Proxy.md](../Textbook/Структурные/Proxy.md) |
| Стратегия | `strategy` | `Behavioral/Strategy/` | [Strategy.md](../Textbook/Поведенческие/Strategy.md) |
| Шаблонный метод | `template-method` | `Behavioral/TemplateMethod/` | [TemplateMethod.md](../Textbook/Поведенческие/TemplateMethod.md) |
| Итератор | `iterator`, `iterator-enumerable` | `Behavioral/Iterator/` | [Iterator.md](../Textbook/Поведенческие/Iterator.md) |
| Наблюдатель | `observer` | `Behavioral/Observer/` | [Observer.md](../Textbook/Поведенческие/Observer.md) |
| Посетитель | `visitor` | `Behavioral/Visitor/` | [Visitor.md](../Textbook/Поведенческие/Visitor.md) |
| Цепочка обязанностей | `chain` | `Behavioral/ChainOfResponsibility/` | [ChainOfResponsibility.md](../Textbook/Поведенческие/ChainOfResponsibility.md) |
| Посредник | `mediator` | `Behavioral/Mediator/` | [Mediator.md](../Textbook/Поведенческие/Mediator.md) |
| Обход графа (DFS/BFS) | `graph` | `Graphs/` | — |
| ReaderWriterLock | `rwlock` | `Concurrency/ReaderWriterLock/` | — |

## Папка `_AntiPatterns`

Здесь лежит код с намеренными ошибками — он нужен для разбора на занятиях.
Каждый такой фрагмент помечен комментарием `// АНТИПРИМЕР:` и ссылкой на разбор в учебнике.

**Не копируйте этот код в лабораторные работы.** Найти все пометки:

```bash
grep -rn "АНТИПРИМЕР" --include=*.cs .
```

Сейчас помечены:

| Где | Что не так |
|---|---|
| `Creational/Singleton/Singleton.cs` | двойная проверка блокировки без `volatile`; `Instance1()` может вернуть `null` |
| `Creational/Singleton/SingletonApp.cs` | `lock(this)` — блокировка на публичном объекте |
| `Behavioral/TemplateMethod/LogReader.cs` | `new Random()` на каждый вызов |
| `_AntiPatterns/Enumerator/MyCollection.cs` | коллекция является собственным итератором |
| `_AntiPatterns/GodObject/LogProcessingModule.cs` | god-объект: 22 помеченных нарушения SOLID |

Рядом с каждым антипримером лежит правильный вариант: например,
`Creational/Singleton/LazyConfigService.cs` — потокобезопасный синглтон через `Lazy<T>`
с открытым конструктором, пригодный и для DI, и для тестов.

## Тесты

```bash
dotnet test
```

Один тест — `ConfigServiceTest.GetValue_ЗависитОтПорядкаВыполненияТестов` — помечен
`[Ignore]` намеренно. Он демонстрирует, что классический синглтон переносит состояние
между тестами и делает их зависимыми от порядка запуска. Снимите `[Ignore]`
и запустите тест в одиночку, чтобы увидеть падение.

Все остальные тесты должны быть зелёными.

