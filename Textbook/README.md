# Учебник раздела 1 «Паттерны программирования»

Справочник по паттернам проектирования на C#. Каждая глава написана по единой структуре,
которая задаётся на занятии 0 (слайд «Представление паттерна»):

1. Уникальное название
2. Описание решаемой проблемы
3. Описание способа решения
4. Диаграмма и способ реализации
5. Плюсы и минусы, **когда НЕ применять**
6. Код — ссылка на рабочий пример в репозитории
7. Вывод

По этой же структуре оформляются отчёты по лабораторным работам —
см. [практикум](../Практикум/README.md).

---

## Оглавление

### Обзор

| Материал | Что внутри | Занятие |
|---|---|---|
| [SOLID: пять принципов проектирования классов](SOLID.md) | SRP, OCP, LSP, ISP, DIP на домене логов; разбор god-класса из репозитория | 1–2 |
| [IoC и внедрение зависимостей](IoC_и_DI.md) | IoC, DI, [композиционный корень](IoC_и_DI.md#композиционный-корень), времена жизни, Service Locator как антипаттерн | 3 |
| [Каталог 23 паттернов GoF](Каталог_23_паттерна.md) | Все паттерны «Банды четырёх» одной таблицей: назначение и ключевая идея | 0 |

### Порождающие паттерны

Отвечают за создание объектов.

| Глава | Код | Занятие |
|---|---|---|
| [Singleton (Одиночка)](Порождающие/Singleton.md) | [`Creational/Singleton`](../DesignPatterns/Implementation/Creational/Singleton/) | 2, 6 |
| [Factory Method (Фабричный метод)](Порождающие/FactoryMethod.md) | [`Creational/FactoryMethod`](../DesignPatterns/Implementation/Creational/FactoryMethod/) | 6 |
| [Abstract Factory (Абстрактная фабрика)](Порождающие/AbstractFactory.md) | [`Creational/AbstractFactory`](../DesignPatterns/Implementation/Creational/AbstractFactory/) | 6 |
| [Builder (Строитель)](Порождающие/Builder.md) | [`Creational/Builder`](../DesignPatterns/Implementation/Creational/Builder/) | 6 |
| [Prototype (Прототип)](Порождающие/Prototype.md) | [`Creational/Prototype`](../DesignPatterns/Implementation/Creational/Prototype/) | 6 |

### Структурные паттерны

Отвечают за композицию классов и объектов.

| Глава | Код | Занятие |
|---|---|---|
| [Composite (Компоновщик)](Структурные/Composite.md) | [`Structural/Composite`](../DesignPatterns/Implementation/Structural/Composite/) | 7 |
| [Adapter (Адаптер)](Структурные/Adapter.md) | [`Structural/Adapter`](../DesignPatterns/Implementation/Structural/Adapter/) | 7 |
| [Facade (Фасад)](Структурные/Facade.md) | [`Structural/Facade`](../DesignPatterns/Implementation/Structural/Facade/) | 7 |
| [Decorator (Декоратор)](Структурные/Decorator.md) | [`Structural/Decorator`](../DesignPatterns/Implementation/Structural/Decorator/) | 7 |
| [Proxy (Заместитель)](Структурные/Proxy.md) | [`Structural/Proxy`](../DesignPatterns/Implementation/Structural/Proxy/) | 7 |

### Поведенческие паттерны

Отвечают за взаимодействие между объектами.

| Глава | Код | Занятие |
|---|---|---|
| [Observer (Наблюдатель)](Поведенческие/Observer.md) | [`Behavioral/Observer`](../DesignPatterns/Implementation/Behavioral/Observer/) | 5 |
| [Chain of Responsibility (Цепочка обязанностей)](Поведенческие/ChainOfResponsibility.md) | [`Behavioral/ChainOfResponsibility`](../DesignPatterns/Implementation/Behavioral/ChainOfResponsibility/) | 5 |
| [Strategy (Стратегия)](Поведенческие/Strategy.md) | [`Behavioral/Strategy`](../DesignPatterns/Implementation/Behavioral/Strategy/) | 4 |
| [Template Method (Шаблонный метод)](Поведенческие/TemplateMethod.md) | [`Behavioral/TemplateMethod`](../DesignPatterns/Implementation/Behavioral/TemplateMethod/) | 4 |
| [Iterator (Итератор)](Поведенческие/Iterator.md) | [`Behavioral/Iterator`](../DesignPatterns/Implementation/Behavioral/Iterator/) | 5 |
| [Visitor (Посетитель)](Поведенческие/Visitor.md) | [`Behavioral/Visitor`](../DesignPatterns/Implementation/Behavioral/Visitor/) | 5 |
| [Mediator (Посредник)](Поведенческие/Mediator.md) | [`Behavioral/Mediator`](../DesignPatterns/Implementation/Behavioral/Mediator/) | 4 |

Все девятнадцать глав написаны, у каждой есть рабочий пример кода в репозитории.

---

## Как пользоваться

**Перед лекцией** — прочитать разделы «Описание решаемой проблемы» и «Описание способа решения».
Достаточно понять, какая боль в коде приводит к появлению паттерна.

**После лекции** — разобрать «Диаграмма и способ реализации» вместе с кодом из репозитория.
Каждая глава содержит команду запуска рабочего примера:

```bash
dotnet run --project Implementation -- singleton
```

**Перед лабораторной** — прочитать «Плюсы и минусы» и «Когда НЕ применять».
На защите проекта отдельно спрашивают, где паттерн оказался лишним.

**Перед экзаменом** — [каталог 23 паттернов](Каталог_23_паттерна.md) и разделы «Сравнение» в главах.

---

## Диаграммы

Синтаксис PlantUML разобран в отдельной главе: [Диаграммы на PlantUML](PlantUML.md).
Там исходник и результат стоят парами — это то, что нужно для отчётов.

Диаграммы в остальных главах записаны на PlantUML внутри блоков ` ```plantuml `.
Чтобы увидеть их картинкой, а не текстом:

- **VS Code** — расширение PlantUML, `Alt+D` для предпросмотра;
- **IntelliJ / Rider** — плагин PlantUML Integration;
- **Без установки** — вставить исходник на <https://www.plantuml.com/plantuml/uml/>;
- **Локально в SVG:**
  ```bash
  plantuml -tsvg -o img Порождающие/*.md
  ```

---

## Соглашения

- Один паттерн — одна глава. Вариации реализации (Fluent Builder, Prototype Registry,
  Singleton против DI) — разделы `## Вариация:` внутри основной главы, а не отдельные файлы.
- Заголовки без эмодзи, одинаковый набор разделов во всех главах.
- Примеры кода в главах и в репозитории используют один и тот же домен —
  систему обработки логов из сквозного проекта практикума.
- Каждая глава заканчивается блоками «Когда НЕ применять», «Код» и «Вывод» — именно в этом порядке.

---

## Источники

Материал главы опирается на:

- Э. Гамма, Р. Хелм, Р. Джонсон, Д. Влиссидес. Приёмы объектно-ориентированного проектирования.
  Паттерны проектирования. — СПб.: Питер.
- С. Тепляков. Паттерны проектирования на платформе .NET. — СПб.: Питер.
- Э. Фримен, Э. Фримен. Head First. Паттерны проектирования. — СПб.: Питер.
- К. Ларман. Применение UML 2.0 и шаблонов проектирования.

Заимствованные формулировки и примеры должны сопровождаться ссылкой на источник
в конце соответствующей главы.
